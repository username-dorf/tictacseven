public enum AIDifficulty { Beginner, Easy, Normal, Hard, Insane }

public sealed class AzMctsSettings
{
    public int    Simulations      = 128;
    public int    TimeBudgetMs     = 0;
    public float  Cpuct            = 1.1f;
    public float  TauRoot          = 0.0f;

    //Dirichlet
    public bool   DisableRootNoise = true;   // if true — disable Dirichlet noise on root node
    public float  DirichletEps     = 0.0f;   // noise (if DisableRootNoise=false)
    public float  DirichletAlpha   = 0.3f;

    public bool   QInitFromPrior   = true;
    public float  QInitWeight      = 1.0f;   // 0.5–1.5

    public float  BlunderEps       = 0.0f;   // mistake chance (0..1)
    public int    BlunderTopK      = 3;      // top-K actions to apply blunder to (0 = all actions)
    public float  NoiseStd         = 0.0f;   

    public bool   UseEconomyPrior  = true;
    public float  EconomyAlpha     = 0.8f;   //economy prior weight (0 = no economy prior)

    public bool   RootTacticalBoost = true;
    public float  TacticalAlpha     = 0.8f;  //tactical prior weight (0 = no tactical prior)
}

public static class AzDifficultyPresets
{
    public static AzMctsSettings Get(AIDifficulty d) => d switch
    {
        AIDifficulty.Beginner => new AzMctsSettings {
            Simulations=0, TimeBudgetMs=0,                 // policy-only
            Cpuct=1.0f, TauRoot=1.2f,
            DisableRootNoise=false, DirichletEps=0.50f, DirichletAlpha=0.3f,
            QInitFromPrior=true, QInitWeight=1.0f,
            BlunderEps=0.30f, BlunderTopK=5, NoiseStd=0.35f,
            UseEconomyPrior=true, EconomyAlpha=0.6f,
            RootTacticalBoost=true, TacticalAlpha=0.6f
        },

        AIDifficulty.Easy => new AzMctsSettings {
            Simulations=48, Cpuct=1.1f, TauRoot=0.7f,
            DisableRootNoise=false, DirichletEps=0.35f, DirichletAlpha=0.3f,
            QInitFromPrior=true, QInitWeight=1.0f,
            BlunderEps=0.15f, BlunderTopK=4, NoiseStd=0.20f,
            UseEconomyPrior=true, EconomyAlpha=0.7f,
            RootTacticalBoost=true, TacticalAlpha=0.7f
        },

        AIDifficulty.Normal => new AzMctsSettings {
            Simulations=128, Cpuct=1.0f, TauRoot=0.15f,
            DisableRootNoise=true, DirichletEps=0.0f, DirichletAlpha=0.3f,
            QInitFromPrior=true, QInitWeight=1.0f,
            BlunderEps=0.02f, BlunderTopK=3, NoiseStd=0.0f,
            UseEconomyPrior=true, EconomyAlpha=0.8f,
            RootTacticalBoost=true, TacticalAlpha=0.8f
        },

        AIDifficulty.Hard => new AzMctsSettings {
            Simulations=384, Cpuct=1.0f, TauRoot=0.0f,
            DisableRootNoise=true, DirichletEps=0.0f, DirichletAlpha=0.3f,
            QInitFromPrior=true, QInitWeight=1.0f,
            BlunderEps=0.0f, BlunderTopK=2, NoiseStd=0.0f,
            UseEconomyPrior=true, EconomyAlpha=1.0f,
            RootTacticalBoost=true, TacticalAlpha=0.9f
        },

        _ /* Insane */ => new AzMctsSettings {
            Simulations=768, Cpuct=1.0f, TauRoot=0.0f,
            DisableRootNoise=true, DirichletEps=0.0f, DirichletAlpha=0.0f,
            QInitFromPrior=true, QInitWeight=1.0f,
            BlunderEps=0.0f, BlunderTopK=1, NoiseStd=0.0f,
            UseEconomyPrior=true, EconomyAlpha=1.0f,
            RootTacticalBoost=true, TacticalAlpha=1.0f
        },
    };
}
