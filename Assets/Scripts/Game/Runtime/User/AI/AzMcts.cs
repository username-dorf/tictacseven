using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

public sealed class AzMcts
{
    private readonly AgentThinkingAIController _nn;
    private readonly System.Random _rng;
    private readonly AzMctsSettings _cfg;

    class Node
    {
        public AzState State;
        public Dictionary<int, Child> Children;
        public bool Expanded;
        public float Value;

        public Node(AzState s)
        {
            State = s;
            Children = new Dictionary<int, Child>(64);
            Expanded = false;
            Value = 0f;
        }
    }

    class Child
    {
        public Node Node;
        public float P; // prior
        public int N; // visits
        public float W; // total value
        public float Q => N > 0 ? W / Math.Max(1, N) : 0f;
    }

    public AzMcts(AgentThinkingAIController nn, AzMctsSettings cfg, int? seed = null)
    {
        _nn = nn;
        _cfg = cfg;
        _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
    }

    public (int v, int r, int c) SearchBestMove(AzState rootState)
    {
        var root = new Node(rootState);
        Expand(root, isRoot: true);

        // policy-only
        if (_cfg.Simulations <= 0)
        {
            int bestA = ArgmaxPrior(root);
            if (bestA >= 0) return AzState.IndexToAction(bestA);
            var valids = rootState.GetValidActions();
            return valids.Count > 0 ? valids[_rng.Next(valids.Count)] : (1, 1, 1);
        }

        for (int s = 0; s < _cfg.Simulations; s++) Simulate(root);

        int chosen = SelectFromVisits(root, _cfg.TauRoot);
        if (_cfg.BlunderEps > 0f && _rng.NextDouble() < _cfg.BlunderEps)
            chosen = PickBlunder(root, chosen, _cfg.BlunderTopK);

        return AzState.IndexToAction(chosen);
    }

    public async UniTask<(int v, int r, int c)> SearchBestMoveAsync(
        AzState rootState, CancellationToken ct, int simsPerSlice = 12)
    {
        var rootValid = rootState.GetValidActions();
        if (rootValid == null || rootValid.Count == 0)
            return (1, 1, 1);

        var root = new Node(rootState);
        Expand(root, isRoot: true);

        // policy-only
        if (_cfg.Simulations <= 0)
        {
            int a = SelectFromVisits(root, _cfg.TauRoot);
            return AzState.IndexToAction(a);
        }

        var deadline = (_cfg.TimeBudgetMs > 0)
            ? DateTime.UtcNow.AddMilliseconds(_cfg.TimeBudgetMs)
            : (DateTime?) null;

        int simsLeft = _cfg.Simulations;

        while (!ct.IsCancellationRequested)
        {
            int batch = (_cfg.TimeBudgetMs > 0)
                ? simsPerSlice
                : Math.Min(simsPerSlice, simsLeft);

            for (int i = 0; i < batch; i++)
            {
                Simulate(root);
                if (ct.IsCancellationRequested) break;
            }

            if (_cfg.TimeBudgetMs > 0)
            {
                if (DateTime.UtcNow >= deadline) break;
            }
            else
            {
                simsLeft -= batch;
                if (simsLeft <= 0) break;
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        int chosen = SelectFromVisits(root, _cfg.TauRoot);
        if (_cfg.BlunderEps > 0f && _rng.NextDouble() < _cfg.BlunderEps)
            chosen = PickBlunder(root, chosen, _cfg.BlunderTopK);

        return AzState.IndexToAction(chosen);
    }


    private void Simulate(Node node)
    {
        var path = new List<(Node node, int action)>(64);
        var cur = node;

        // selection
        while (cur.Expanded && !cur.State.Done && cur.Children.Count > 0)
        {
            int a = SelectAction(cur);
            if (a < 0) break;
            path.Add((cur, a));
            cur = cur.Children[a].Node;
        }

        float v;
        if (cur.State.Done)
        {
            v = cur.State.Winner == 0 ? 0f : -1f;
        }
        else if (cur.Expanded && cur.Children.Count == 0)
        {
            v = -1f;
        }
        else
        {
            v = Expand(cur, isRoot: false);
        }

        // backup
        for (int i = path.Count - 1; i >= 0; i--)
        {
            var (nd, a) = path[i];
            if (!nd.Children.TryGetValue(a, out var ch)) continue;
            ch.N += 1;
            ch.W += v;
            v = -v;
        }
    }

    private int SelectAction(Node n)
    {
        if (n.Children.Count == 0) return -1;
        double sqrtNs = Math.Sqrt(Math.Max(1, TotalVisits(n)));
        int bestA = -1;
        double bestScore = double.NegativeInfinity;

        foreach (var kv in n.Children)
        {
            var ch = kv.Value;
            double u = ch.Q + _cfg.Cpuct * ch.P * (sqrtNs / (1 + ch.N));
            if (u > bestScore)
            {
                bestScore = u;
                bestA = kv.Key;
            }
        }

        return bestA;
    }

    private int TotalVisits(Node n)
    {
        int s = 0;
        foreach (var ch in n.Children.Values) s += ch.N;
        return s;
    }

    private float Expand(Node n, bool isRoot)
    {
        var features = n.State.ToFeatures32();
        var validActs = n.State.GetValidActions();

        var mask = new bool[63];
        foreach (var (v, r, c) in validActs) mask[AzState.ActionToIndex(v, r, c)] = true;

        var (logits, value) = _nn.Evaluate(features, mask);
        if (isRoot && _cfg.NoiseStd > 1e-6f)
        {
            for (int i = 0; i < logits.Length; i++)
                logits[i] += (float) NextGaussian(0.0, _cfg.NoiseStd);
        }

        var priors = SoftmaxMasked(logits, mask);

        if (isRoot && !_cfg.DisableRootNoise && _cfg.DirichletEps > 1e-6f)
        {
            var noise = DirichletSample(_cfg.DirichletAlpha, priors.Length);
            float sumMask = 0f;
            for (int i = 0; i < priors.Length; i++)
                if (mask[i])
                    sumMask += noise[i];
            if (sumMask <= 1e-8f) sumMask = 1f;
            for (int i = 0; i < priors.Length; i++)
                if (mask[i])
                    priors[i] = (1 - _cfg.DirichletEps) * priors[i] + _cfg.DirichletEps * (noise[i] / sumMask);
        }

        if (_cfg.UseEconomyPrior)
            ApplyEconomyPrior(validActs, priors);

        if (isRoot && _cfg.RootTacticalBoost)
            ApplyRootTactics(n.State, validActs, priors, _cfg.TacticalAlpha);

        n.Children.Clear();
        foreach (var (v, r, c) in validActs)
        {
            int idx = AzState.ActionToIndex(v, r, c);
            var next = n.State.Clone();
            next.StepUnchecked(v, r, c);

            var ch = new Child {Node = new Node(next), P = priors[idx], N = 0, W = 0f};

            if (_cfg.QInitFromPrior)
            {
                ch.N = 1;
                ch.W = _cfg.QInitWeight * ch.P; // Q≈P
            }

            n.Children[idx] = ch;
        }

        n.Expanded = true;
        n.Value = value;
        return value;
    }

    private int ArgmaxPrior(Node root)
    {
        int bestA = -1;
        float bestP = float.NegativeInfinity;
        foreach (var kv in root.Children)
            if (kv.Value.P > bestP)
            {
                bestP = kv.Value.P;
                bestA = kv.Key;
            }

        return bestA;
    }


    private static float[] SoftmaxMasked(float[] logits, bool[] mask)
    {
        var p = new float[logits.Length];

        for (int i = 0; i < logits.Length; i++)
            if (!float.IsFinite(logits[i]))
                logits[i] = -1e9f;

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

            double x = Math.Exp(logits[i] - max);
            e[i] = x;
            sum += x;
        }

        if (sum <= 1e-18)
        {
            int cnt = 0;
            for (int i = 0; i < mask.Length; i++)
                if (mask[i])
                    cnt++;
            if (cnt == 0) return p;
            float pr = 1f / cnt;
            for (int i = 0; i < mask.Length; i++)
                if (mask[i])
                    p[i] = pr;
            return p;
        }

        for (int i = 0; i < p.Length; i++)
            p[i] = (float) (e[i] / sum);
        return p;
    }

    private void ApplyEconomyPrior(List<(int v, int r, int c)> acts, float[] priors)
    {
        var minV = new Dictionary<(int r, int c), int>();
        foreach (var a in acts)
        {
            var key = (a.r, a.c);
            if (!minV.TryGetValue(key, out var cur) || a.v < cur) minV[key] = a.v;
        }

        double sum = 0.0;
        foreach (var a in acts)
        {
            int idx = AzState.ActionToIndex(a.v, a.r, a.c);
            int vmin = minV[(a.r, a.c)];
            float w = (float) Math.Pow((float) vmin / (float) a.v, Math.Max(0.0f, _cfg.EconomyAlpha));
            priors[idx] *= w;
        }

        foreach (var a in acts) sum += priors[AzState.ActionToIndex(a.v, a.r, a.c)];
        if (sum <= 1e-12)
        {
            float p = 1f / Math.Max(1, acts.Count);
            foreach (var a in acts) priors[AzState.ActionToIndex(a.v, a.r, a.c)] = p;
        }
        else
        {
            foreach (var a in acts)
            {
                int idx = AzState.ActionToIndex(a.v, a.r, a.c);
                priors[idx] = (float) (priors[idx] / sum);
            }
        }
    }

    private void ApplyRootTactics(AzState s, List<(int v, int r, int c)> acts, float[] priors, float alpha)
    {
        //win-in-1
        var wins = new List<int>();
        foreach (var a in acts)
        {
            var next = s.Clone();
            next.StepUnchecked(a.v, a.r, a.c);
            if (next.Done && next.Winner != 0)
                wins.Add(AzState.ActionToIndex(a.v, a.r, a.c));
        }

        if (wins.Count > 0)
        {
            MixHardMask(priors, wins, alpha);
            return;
        }

        bool anyLosing = false, anySafe = false;
        var safe = new List<int>();

        foreach (var a in acts)
        {
            var next = s.Clone();
            next.StepUnchecked(a.v, a.r, a.c);

            bool oppHasWin = OppHasWinInOne(next);
            if (oppHasWin) anyLosing = true;
            else
            {
                anySafe = true;
                safe.Add(AzState.ActionToIndex(a.v, a.r, a.c));
            }
        }

        if (anyLosing && anySafe && safe.Count > 0)
        {
            MixHardMask(priors, safe, alpha);
        }
    }

    private void MixHardMask(float[] priors, List<int> goodIdxs, float alpha)
    {
        if (alpha <= 0f) return;
        var mask = new float[priors.Length];
        float p = 1f / goodIdxs.Count;
        foreach (var i in goodIdxs) mask[i] = p;
        for (int i = 0; i < priors.Length; i++)
            priors[i] = (1f - alpha) * priors[i] + alpha * mask[i];
        float sum = 0f;
        for (int i = 0; i < priors.Length; i++) sum += priors[i];
        if (sum > 1e-12f)
            for (int i = 0; i < priors.Length; i++)
                priors[i] /= sum;
    }

    private bool OppHasWinInOne(AzState sAfterOurMove)
    {
        var oppActs = sAfterOurMove.GetValidActions();
        foreach (var (v2, r2, c2) in oppActs)
        {
            var s2 = sAfterOurMove.Clone();
            s2.StepUnchecked(v2, r2, c2);
            if (s2.Done && s2.Winner != 0) return true;
        }

        return false;
    }


    private int SelectFromVisits(Node root, float tau)
    {
        if (root.Children.Count == 0)
        {
            var fallback = root.State.GetValidActions();
            if (fallback != null && fallback.Count > 0)
                return AzState.ActionToIndex(fallback[0].v, fallback[0].r, fallback[0].c);
            return AzState.ActionToIndex(1, 1, 1);
        }

        var actions = new List<int>(root.Children.Keys);
        var visits = actions.Select(a => (float) root.Children[a].N).ToArray();

        if (tau <= 1e-6f)
        {
            int bestA = actions[0];
            int bestN = root.Children[bestA].N;
            for (int i = 1; i < actions.Count; i++)
            {
                int a = actions[i];
                int n = root.Children[a].N;
                if (n > bestN)
                {
                    bestN = n;
                    bestA = a;
                }
            }

            return bestA;
        }

        // softmax N^(1/tau)
        var probs = new float[visits.Length];
        float max = 0f;
        for (int i = 0; i < visits.Length; i++)
            if (visits[i] > max)
                max = visits[i];
        double sum = 0;
        for (int i = 0; i < visits.Length; i++)
        {
            double x = Math.Pow(Math.Max(0.0, visits[i] - max) + max, 1.0 / Math.Max(1e-6, tau));
            probs[i] = (float) x;
            sum += x;
        }

        if (sum <= 1e-12)
        {
            for (int i = 0; i < probs.Length; i++) probs[i] = 1f / probs.Length;
        }
        else
        {
            for (int i = 0; i < probs.Length; i++) probs[i] = (float) (probs[i] / sum);
        }

        double u = _rng.NextDouble();
        double acc = 0;
        for (int i = 0; i < probs.Length; i++)
        {
            acc += probs[i];
            if (u <= acc) return actions[i];
        }

        return actions[^1];
    }

    private int PickBlunder(Node root, int chosen, int topK)
    {
        if (topK <= 0 || root.Children.Count <= 1) return chosen;
        var arr = new List<(int a, float q)>();
        foreach (var kv in root.Children) arr.Add((kv.Key, kv.Value.Q));
        arr.Sort((l, r) => l.q.CompareTo(r.q));

        int count = Math.Min(topK, arr.Count);
        for (int i = 0; i < count; i++)
            if (arr[i].a == chosen)
                return chosen;
        int idx = _rng.Next(count);
        return arr[idx].a;
    }

    // --- Dirichlet ---

    private float[] DirichletSample(float alpha, int n)
    {
        var g = new double[n];
        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            double x = SampleGamma(alpha);
            g[i] = x;
            sum += x;
        }

        var d = new float[n];
        if (sum <= 1e-18)
        {
            for (int i = 0; i < n; i++) d[i] = 0f;
            return d;
        }

        for (int i = 0; i < n; i++) d[i] = (float) (g[i] / sum);
        return d;
    }

    private double SampleGamma(double alpha)
    {
        if (alpha < 1.0)
            return SampleGamma(alpha + 1.0) * Math.Pow(_rng.NextDouble(), 1.0 / alpha);

        double d = alpha - 1.0 / 3.0;
        double c = 1.0 / Math.Sqrt(9.0 * d);
        while (true)
        {
            double u1 = _rng.NextDouble();
            double u2 = _rng.NextDouble();
            double r = Math.Sqrt(-2.0 * Math.Log(u1));
            double x = r * Math.Cos(2.0 * Math.PI * u2);
            double v = Math.Pow(1.0 + c * x, 3);
            if (v > 0)
            {
                double u = _rng.NextDouble();
                if (u < 1.0 - 0.0331 * Math.Pow(x, 4)) return d * v;
                if (Math.Log(u) < 0.5 * x * x + d * (1.0 - v + Math.Log(v))) return d * v;
            }
        }
    }

    private double NextGaussian(double mean, double std)
    {
        double u1 = Math.Max(1e-12, _rng.NextDouble());
        double u2 = _rng.NextDouble();
        double mag = Math.Sqrt(-2.0 * Math.Log(u1));
        return mean + std * mag * Math.Cos(2.0 * Math.PI * u2);
    }
}