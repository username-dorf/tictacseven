using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Field;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace Game.User
{
    public class AgentAIInstaller : Installer<AgentAIInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<AgentAIModelAssetProvider>().AsSingle().NonLazy();
            Container.Bind<AgentAIController>().AsSingle().NonLazy();
            Container.Bind<AgentThinkingAIController>().AsSingle().NonLazy();
        }
    }

    public class AgentAIModelAssetProvider : IDisposable
    {
        private const string MODEL_ADDRESS = "ppo_actor";

        public Model Model { get; private set; }
        private ModelAsset _modelAsset;
        private AsyncOperationHandle<ModelAsset> _handle;

        public async UniTask<Model> LoadModelAsync(CancellationToken token)
        {
            {
                _handle = Addressables.LoadAssetAsync<ModelAsset>(MODEL_ADDRESS);

                try
                {
                    await _handle
                        .ToUniTask(cancellationToken: token)
                        .SuppressCancellationThrow();
                }
                catch (OperationCanceledException)
                {
                    Addressables.Release(_handle);
                    throw;
                }

                var asset = _handle.Result;
                Model = ModelLoader.Load(asset);

                if (Model == null)
                {
                    Addressables.Release(_handle);
                    throw new Exception($"Failed to load model from address: {MODEL_ADDRESS}");
                }

                return Model;
            }
        }

        public void Dispose()
        {
            if (_handle.IsValid())
            {
                Addressables.Release(_handle);
            }
        }
    }

    public enum PolicyDifficulty
    {
        Beginner,
        Easy,
        Normal,
        Hard,
        Insane
    }

    public sealed class PolicyBotSettings
    {
        public int TopK = 1;
        public float TopP = 0.0f; // nucleus: 0=disable; 0.15..0.9 typically
        public float Temperature = 0.0f;
        public bool UniformInPool = false;

        public float EpsilonRandom = 0.0f; // chance of random action (0.0f = no random)
        public float NoiseStd = 0.0f;

        public int OpeningPlies = 0;
        public float OpeningTopP = 0.0f;
        public float OpeningTemp = 0.0f;

        public int? GameSeed = null;
    }

    public static class PolicyDifficultyPresets
    {
        public static PolicyBotSettings Get(PolicyDifficulty d) => d switch
        {
            PolicyDifficulty.Beginner => new PolicyBotSettings
            {
                TopK = 5, TopP = 0.90f, Temperature = 0.9f, UniformInPool = true,
                EpsilonRandom = 0.30f, NoiseStd = 0.30f,
                OpeningPlies = 4, OpeningTopP = 0.95f, OpeningTemp = 1.0f
            },
            PolicyDifficulty.Easy => new PolicyBotSettings
            {
                TopK = 4, TopP = 0.70f, Temperature = 0.6f,
                EpsilonRandom = 0.12f, NoiseStd = 0.15f,
                OpeningPlies = 3, OpeningTopP = 0.80f, OpeningTemp = 0.8f
            },
            PolicyDifficulty.Normal => new PolicyBotSettings
            {
                TopK = 3, TopP = 0.50f, Temperature = 0.35f,
                EpsilonRandom = 0.04f, NoiseStd = 0.0f,
                OpeningPlies = 2, OpeningTopP = 0.60f, OpeningTemp = 0.6f
            },
            PolicyDifficulty.Hard => new PolicyBotSettings
            {
                TopK = 2, TopP = 0.25f, Temperature = 0.15f,
                EpsilonRandom = 0.0f, NoiseStd = 0.0f,
                OpeningPlies = 1, OpeningTopP = 0.35f, OpeningTemp = 0.25f
            },
            _ /* Insane */ => new PolicyBotSettings
            {
                TopK = 1, TopP = 0.15f, Temperature = 0.0f,
                EpsilonRandom = 0.0f, NoiseStd = 0.0f,
                OpeningPlies = 0
            },
        };
    }

    public class AgentAIController : IDisposable
    {
        public bool IsInitialized { get; private set; }
        private Worker worker;
        private readonly AgentAIModelAssetProvider _modelAssetProvider;
        private System.Random _rng = new System.Random();

        private const int OBS_DIM = 32;
        private const int ACT_DIM = 63;

        public AgentAIController(AgentAIModelAssetProvider modelAssetProvider)
        {
            _modelAssetProvider = modelAssetProvider;
        }

        public async UniTask InitializeAsync(CancellationToken token)
        {
            if (_modelAssetProvider.Model == null)
                await _modelAssetProvider.LoadModelAsync(token);

            worker = new Worker(_modelAssetProvider.Model, BackendType.CPU);
            IsInitialized = true;
        }

        public void StartNewGame(int? seed = null)
        {
            _rng = seed.HasValue
                ? new System.Random(seed.Value)
                : new System.Random(unchecked(Environment.TickCount * 397) ^ GetHashCode());
        }

        public (int v, int row, int col) ChooseActionVRC(
            FieldModel field,
            UserEntitiesModel agentModel,
            UserEntitiesModel playerModel,
            int unityPlayer,
            PolicyBotSettings cfg,
            bool debugLog = false)
        {
            if (cfg.GameSeed.HasValue)
                _rng = new System.Random(cfg.GameSeed.Value);

            float[] state = FieldModel.BuildState(field, agentModel, playerModel, unityPlayer);
            var validTriples = BuildValidActions(field, agentModel, unityPlayer);
            if (validTriples.Count == 0) return (1, 1, 1);

            var mask = new bool[ACT_DIM];
            foreach (var (vv, r, c) in validTriples)
                mask[ToIndex(vv, r, c)] = true;

            var logits = EvaluateLogits(state, mask);

            if (cfg.EpsilonRandom > 0f && _rng.NextDouble() < cfg.EpsilonRandom)
            {
                var rnd = validTriples[_rng.Next(validTriples.Count)];
                return rnd;
            }

            if (cfg.NoiseStd > 1e-6f)
            {
                for (int i = 0; i < ACT_DIM; i++)
                    if (mask[i])
                        logits[i] = AddGaussian(logits[i], cfg.NoiseStd);
            }

            float tau = Math.Max(1e-6f, cfg.Temperature);
            var validIdx = validTriples.Select(t => ToIndex(t.v, t.row, t.col)).ToArray();
            var probsAll = SoftmaxMasked(logits, mask, tau);

            int ply = EstimatePly(field);
            float useTopP = cfg.TopP;
            float useTau = tau;
            if (cfg.OpeningPlies > 0 && ply < cfg.OpeningPlies)
            {
                useTopP = Math.Max(useTopP, cfg.OpeningTopP);
                useTau = Math.Max(useTau, cfg.OpeningTemp);
            }

            var pool = BuildPool(validIdx, probsAll, useTopP, cfg.TopK);

            int chosenIdx;
            if (pool.Length == 1 || (useTau <= 1e-6f && !cfg.UniformInPool))
            {
                chosenIdx = pool[0]; // argmax
            }
            else if (cfg.UniformInPool)
            {
                chosenIdx = pool[_rng.Next(pool.Length)];
            }
            else
            {
                var p = Renorm(probsAll, pool);
                chosenIdx = Sample(pool, p);
            }

            var (v, row, col) = FromIndex(chosenIdx);

            if (debugLog)
            {
                string view = string.Join(", ",
                    pool.Select(i => $"{i}:{probsAll[i]:0.003}"));
                Debug.Log(
                    $"[PolicyBot] ply={ply} chosen={chosenIdx} -> (v={v},r={row},c={col}) pool[{pool.Length}]={view}");
            }

            return (v, row, col);
        }

        public (int v, int row, int col) ChooseActionVRC(
            FieldModel field,
            UserEntitiesModel agentModel,
            UserEntitiesModel playerModel,
            int unityPlayer,
            PolicyDifficulty difficulty,
            bool debugLog = false)
        {
            var cfg = PolicyDifficultyPresets.Get(difficulty);
            return ChooseActionVRC(field, agentModel, playerModel, unityPlayer, cfg, debugLog);
        }


        public float[] EvaluateLogits(float[] input, bool[] actionMask = null)
        {
            if (input.Length != OBS_DIM)
            {
                Debug.LogWarning($"[AgentAI] Obs dim mismatch {input.Length} vs {OBS_DIM}. Resizing.");
                Array.Resize(ref input, OBS_DIM);
            }

            using var inputTensor = new Tensor<float>(new TensorShape(1, OBS_DIM), input);
            worker.Schedule(inputTensor);

            using var outputTensor = worker.PeekOutput() as Tensor<float>;
            using var resultTensor = outputTensor.ReadbackAndClone();

            var logits = resultTensor.AsReadOnlySpan().ToArray();
            if (actionMask != null && actionMask.Length == ACT_DIM)
            {
                for (int i = 0; i < ACT_DIM; i++)
                    if (!actionMask[i])
                        logits[i] = float.NegativeInfinity;
            }

            for (int i = 0; i < ACT_DIM; i++)
                if (!float.IsFinite(logits[i]))
                    logits[i] = float.NegativeInfinity;
            return logits;
        }

        public void Dispose() => worker?.Dispose();

        private static System.Collections.Generic.List<(int v, int row, int col)> BuildValidActions(
            FieldModel field, UserEntitiesModel agentModel, int unityCurrentPlayer)
        {
            int current = (unityCurrentPlayer == 1) ? +1 : -1;
            var actions = new System.Collections.Generic.List<(int, int, int)>();

            var hand = agentModel.Entities
                .Select(e => e.Data.Merit.Value)
                .Where(v => v >= 1 && v <= 7)
                .OrderBy(v => v)
                .ToArray();
            if (hand.Length == 0) return actions;

            for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
            {
                var cell = field.Entities[new Vector2Int(row, col)];
                int o = cell.Data.Owner.Value; // 0,1,2
                int ownerF = (o == 1 ? +1 : o == 2 ? -1 : 0); // +1/-1/0
                int cellVal = Math.Abs(cell.Data.Merit.Value); // 0..7

                foreach (int v in hand)
                {
                    bool legal = (ownerF == 0) || (ownerF == -current && cellVal < v);
                    if (legal) actions.Add((v, row, col));
                }
            }

            return actions;
        }

        private static int ToIndex(int v, int row, int col)
        {
            return (v - 1) * 9 + (row * 3 + col);
        }

        private static (int v, int row, int col) FromIndex(int idx)
        {
            int v = (idx / 9) + 1;
            int pos = idx % 9;
            int row = pos / 3;
            int col = pos % 3;
            return (v, row, col);
        }

        private static float[] SoftmaxMasked(float[] logits, bool[] mask, float tau)
        {
            var p = new float[logits.Length];
            float max = float.NegativeInfinity;
            for (int i = 0; i < logits.Length; i++)
                if (mask[i] && logits[i] > max)
                    max = logits[i];
            if (!float.IsFinite(max)) max = 0f;

            double sum = 0.0;
            var e = new double[logits.Length];
            for (int i = 0; i < logits.Length; i++)
            {
                if (!mask[i])
                {
                    e[i] = 0;
                    continue;
                }

                double x = Math.Exp((logits[i] - max) / Math.Max(1e-6f, tau));
                e[i] = x;
                sum += x;
            }

            if (sum <= 1e-18)
            {
                int cnt = mask.Count(m => m);
                float pr = cnt > 0 ? 1f / cnt : 0f;
                for (int i = 0; i < logits.Length; i++)
                    if (mask[i])
                        p[i] = pr;
                return p;
            }

            for (int i = 0; i < logits.Length; i++) p[i] = (float) (e[i] / sum);
            return p;
        }

        private static int[] BuildPool(int[] validIdx, float[] probs, float topP, int topK)
        {
            var sorted = validIdx.OrderByDescending(i => probs[i]).ToArray();

            // nucleus
            int cut = sorted.Length;
            if (topP > 1e-6f)
            {
                double acc = 0.0;
                for (int z = 0; z < sorted.Length; z++)
                {
                    acc += probs[sorted[z]];
                    if (acc >= topP)
                    {
                        cut = Math.Max(1, z + 1);
                        break;
                    }
                }
            }

            int K = Math.Max(1, Math.Min(topK <= 0 ? sorted.Length : topK, cut));
            return sorted.Take(K).ToArray();
        }

        private static float[] Renorm(float[] probs, int[] pool)
        {
            double s = 0.0;
            var outp = new float[pool.Length];
            for (int i = 0; i < pool.Length; i++) s += probs[pool[i]];
            if (s <= 1e-18)
            {
                float pr = 1f / pool.Length;
                for (int i = 0; i < pool.Length; i++) outp[i] = pr;
                return outp;
            }

            for (int i = 0; i < pool.Length; i++) outp[i] = (float) (probs[pool[i]] / s);
            return outp;
        }

        private int Sample(int[] idxs, float[] probs)
        {
            double u = _rng.NextDouble();
            double acc = 0.0;
            for (int z = 0; z < idxs.Length; z++)
            {
                acc += probs[z];
                if (u <= acc) return idxs[z];
            }

            return idxs[^1];
        }

        private float AddGaussian(float x, float std)
        {
            double u1 = 1.0 - _rng.NextDouble();
            double u2 = 1.0 - _rng.NextDouble();
            double g = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            double y = x + std * g;
            return float.IsFinite((float) y) ? (float) y : x;
        }

        private static int EstimatePly(FieldModel field)
        {
            int cnt = 0;
            for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                if (field.Entities[new Vector2Int(r, c)].Data.Owner.Value != 0)
                    cnt++;
            return cnt;
        }

        [Obsolete("Use ChooseActionVRC instead Run")]
        public float[] Run(float[] input, bool[] actionMask = null, bool deterministic = true)
        {
            var logits = EvaluateLogits(input, actionMask);
            if (deterministic)
            {
                int argmax = 0;
                float best = logits[0];
                for (int i = 1; i < ACT_DIM; i++)
                    if (logits[i] > best)
                    {
                        best = logits[i];
                        argmax = i;
                    }

                Debug.Log($"Argmax UNITY = {argmax}");
            }

            return logits;
        }
    }
}