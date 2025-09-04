
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.User;
using Unity.Sentis;
using UnityEngine;

public class AgentThinkingAIController : IDisposable
{
    public bool IsInitialized { get; private set; }
    private Worker worker;
    private readonly AgentAIModelAssetProvider _modelAssetProvider;

    private const int OBS_DIM = 32;
    private const int ACT_DIM = 63;

    // ONNX model output names
    private const string OUT_POLICY = "logits";
    private const string OUT_VALUE  = "value";

    public AgentThinkingAIController(AgentAIModelAssetProvider modelAssetProvider)
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

    public (float[] logits, float value) Evaluate(float[] input, bool[] actionMask = null)
    {
        if (input.Length != OBS_DIM)
        {
            Debug.LogWarning($"[AgentAI] Obs mismatch {input.Length} vs {OBS_DIM}. Resizing.");
            Array.Resize(ref input, OBS_DIM);
        }

        using var inputTensor = new Tensor<float>(new TensorShape(1, OBS_DIM), input);
        worker.Schedule(inputTensor);

        // policy
        using var logitsTensor  = string.IsNullOrEmpty(OUT_POLICY) ? 
                                  worker.PeekOutput() as Tensor<float> : 
                                  worker.PeekOutput(OUT_POLICY) as Tensor<float>;
        using var logitsRB = logitsTensor.ReadbackAndClone();
        var logits = logitsRB.AsReadOnlySpan().ToArray(); // 63

        float value = 0f;
        try
        {
            using var valueTensor = worker.PeekOutput(OUT_VALUE) as Tensor<float>;
            if (valueTensor != null)
            {
                using var valueRB = valueTensor.ReadbackAndClone();
                var arr = valueRB.AsReadOnlySpan().ToArray();
                value = arr.Length > 0 ? arr[0] : 0f;
            }
        }
        catch
        {
            
        }

        if (actionMask != null && actionMask.Length == ACT_DIM)
        {
            for (int i = 0; i < ACT_DIM; i++)
                if (!actionMask[i]) logits[i] = float.NegativeInfinity;
        }

        return (logits, value);
    }

    public void Dispose()
    {
        worker?.Dispose();
    }
}
