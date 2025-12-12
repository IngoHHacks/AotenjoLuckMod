using LuckMod.Artifacts;
using LuckMod.Utils;

namespace LuckMod.Managers;

public class LuckManager : MonoBehaviour
{
    public static readonly int[][] BASE_PROBS_ARTIFACT = new int[][]
    {
//       comn,rare,epic
        [100 , 16 , 2  ], // Level 1 (East)
        [65  , 25 , 4  ], // Level 2 (South)
        [45  , 30 , 12 ], // Level 3 (West)
        [30  , 40 , 20 ]  // Level 4+ (North, Infinite Challenge)
    }; 
//                                                                    comn,rare,epic,legn,anct
    public static readonly int[] BASE_PROBS_YAKU_WIND   = new int[] { 500 , 60 , 10 , 5  , 1   };
    public static readonly int[] BASE_PROBS_YAKU_FOREST = new int[] { 500 , 90 , 23 , 5  , 1   };
    public static readonly int[] BASE_PROBS_YAKU_FIRE   = new int[] { 500 , 70 , 20 , 5  , 0   };
    public static readonly int[] BASE_PROBS_YAKU_EARTH  = new int[] { 540 , 90 , 40 , 10 , 1   };
    
    public static readonly int[,] YAKU_PROB_MODS_PER_LEVEL = new int[,]
    {
//        comn,rare,epic,legn,anct
        { 200 , 100, 100, 100, 100 }, // Level 1 (East)
        { 200 , 300, 100, 100, 100 }, // Level 2 (South)
        { 100 , 300, 200, 100, 100 }, // Level 3 (West)
        { 100 , 100, 400, 400, 100 }, // Level 4 (North)
        { 20  , 100, 100, 200, 100 } // Infinite Challenge
    };
    
    private static LuckManager _instance;
    
    public static LuckManager Instance 
    {
        get
        {
            if (_instance != null) return _instance;
            var obj = new GameObject("LuckManager");
            DontDestroyOnLoad(obj);
            return obj.AddComponent<LuckManager>();
        }
    }
    
    public static int Level => (GameManager.Instance.player.Level / 4) + 1;
    public static int LevelIndex => Level - 1;
    public static int LevelClamped => Mathf.Clamp(Level, 1, 4);
    public static int LevelClampedWithEndless => Mathf.Clamp(Level, 1, 5);
    public static int LevelIndexClamped => Mathf.Clamp(LevelIndex, 0, 3);
    
    public static int LevelIndexClampedWithEndless => Mathf.Clamp(LevelIndex, 0, 4);
    
    public static string WindName => Level switch
    {
        1 => "East",
        2 => "South",
        3 => "West",
        4 => "North",
        _ => "Infinite Challenge"
    };
    
    public bool ShowDebugInfo { get; set; } = false;

    public float Luck
    {
        get
        {
            float totalLuck = ManualLuck;
            foreach (var mod in ArtifactLuckModifiers.Values)
            {
                totalLuck += mod;
            }
            return totalLuck;
        }
    }

    public float ManualLuck { get; set; } = 0f;
    public Dictionary<Artifact, float> ArtifactLuckModifiers { get; } = new();
    public float LuckClamped => Mathf.Clamp(Luck, -1000f, 1000f);
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnGUI()
    {
        if (ShowDebugInfo)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                normal = { textColor = Color.white}
            };
            var hdrStyle = new GUIStyle(style)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.UpperCenter
            };
            GUI.skin.label = style;
            if (GameManager.Instance?.player == null)
            {
                GUI.DrawTexture(new Rect(10, 10, 1220, 100), TextureUtils.GetTransparentTexture(new Color(0f, 0f, 0f, 0.5f)));
                GUI.Label(new Rect(20, 20, 1200, 40), "-- Luck Debug Info --", hdrStyle);
                GUI.Label(new Rect(20, 60, 1200, 40), "Not in-game.");
                return;
            }
            GUI.DrawTexture(new Rect(10, 10, 1220, 300), TextureUtils.GetTransparentTexture(new Color(0f, 0f, 0f, 0.5f)));
            GUI.Label(new Rect(20, 20, 1200, 40), "-- Luck Debug Info --", hdrStyle);
            GUI.Label(new Rect(20, 60, 1200, 40), $"Current Level Group: {WindName}");
            var artifactProbs = WeightsToProbabilities(BASE_PROBS_ARTIFACT[LevelIndexClamped]);
            GUI.Label(new Rect(20, 100, 1200, 40), $"Current Base Artifact Probs: [{artifactProbs[0]*100f:0.##}%, {artifactProbs[1]*100f:0.##}%, {artifactProbs[2]*100f:0.##}%]");
            var yakuWeights = SumArrays(
                BASE_PROBS_YAKU_WIND,
                BASE_PROBS_YAKU_FOREST,
                BASE_PROBS_YAKU_FIRE,
                BASE_PROBS_YAKU_EARTH
            );
            for (int i = 0; i < yakuWeights.Length; i++)
            {
                var f = yakuWeights[i] * YAKU_PROB_MODS_PER_LEVEL[LevelIndexClamped, i] / 100f;
                yakuWeights[i] = (int)f;
            }
            var yakuProbs = WeightsToProbabilities(yakuWeights);
            GUI.Label(new Rect(20, 140, 1200, 40), $"Current Base Yaku Probs (Avg): [{yakuProbs[0]*100f:0.##}%, {yakuProbs[1]*100f:0.##}%, {yakuProbs[2]*100f:0.##}%, {yakuProbs[3]*100f:0.##}%, {yakuProbs[4]*100f:0.##}%]");
            GUI.Label(new Rect(20, 180, 1200, 40), $"Player Luck: {this.LuckClamped:0.##}");
            if (this.LuckClamped != 0f)
            {
                var adjustedArtifactWeights = ApplyLuckToRarities(BASE_PROBS_ARTIFACT[LevelIndexClamped]);
                var adjustedArtifactProbs = WeightsToProbabilities(adjustedArtifactWeights);
                GUI.Label(new Rect(20, 220, 1200, 40), $"Adjusted Artifact Probs: [{ToStringWithDiff(artifactProbs[0], adjustedArtifactProbs[0])}, {ToStringWithDiff(artifactProbs[1], adjustedArtifactProbs[1])}, {ToStringWithDiff(artifactProbs[2], adjustedArtifactProbs[2])}]");
                var adjustedYakuWeights = ApplyLuckToRarities(yakuWeights);
                var adjustedYakuProbs = WeightsToProbabilities(adjustedYakuWeights);
                GUI.Label(new Rect(20, 260, 1200, 40), $"Adjusted Yaku Probs (Avg): [{ToStringWithDiff(yakuProbs[0], adjustedYakuProbs[0])}, {ToStringWithDiff(yakuProbs[1], adjustedYakuProbs[1])}, {ToStringWithDiff(yakuProbs[2], adjustedYakuProbs[2])}, {ToStringWithDiff(yakuProbs[3], adjustedYakuProbs[3])}, {ToStringWithDiff(yakuProbs[4], adjustedYakuProbs[4])}]");
            }
        }
    }

    private int[] SumArrays(params int[][] arrays)
    {
        if (arrays.Length == 0) return [];
        int length = arrays[0].Length;
        int[] result = new int[length];
        foreach (var array in arrays)
        {
            if (array.Length != length)
            {
                throw new ArgumentException("All arrays must have the same length to be summed.");
            }
            for (int i = 0; i < length; i++)
            {
                result[i] += array[i];
            }
        }
        return result;
    }
    
    private string ToStringWithDiff(double original, double adjusted)
    {
        double diff = adjusted - original;
        if (Math.Abs(diff) < 0.000001)
        {
            return $"{adjusted * 100f:0.##}% (±0.00%)";
        }
        string sign = diff >= 0f ? "+" : "-";
        if (sign == "+")
        {
            return $"{adjusted * 100f:0.##}% <color=green>({sign}{diff * 100f:0.##}%)</color>";
        }
        return $"{adjusted * 100f:0.##}% <color=red>({sign}{-diff * 100f:0.##}%)</color>";
    }

    public double[] WeightsToProbabilities(params int[] weights)
    {
        return WeightsToProbabilities(Array.ConvertAll(weights, w => (double)w));
    }
    
    public double[] WeightsToProbabilities(params double[] weights)
    {
        double totalWeight = 0.0;
        foreach (var weight in weights)
        {
            totalWeight += weight;
        }
        double[] probabilities = new double[weights.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            probabilities[i] = weights[i] / totalWeight;
        }
        return probabilities;
    }
    
    public double[] FlattenProbabilities(params double[] probabilities)
    {
        return WeightsToProbabilities(probabilities);
    }
    
    public int[] ProbabilitiesToWeights(params double[] probabilities)
    {
        return ProbabilitiesToWeights(probabilities, 1000f);
    }
    
    // ReSharper disable once MethodOverloadWithOptionalParameter (intended)
    public int[] ProbabilitiesToWeights(double[] probabilities, double totalWeight = 1000.0)
    {
        double totalProbability = 0.0;
        foreach (var prob in probabilities)
        {
            totalProbability += prob;
        }
        int[] weights = new int[probabilities.Length];
        for (int i = 0; i < probabilities.Length; i++)
        {
            weights[i] = (int)Math.Round((probabilities[i] / totalProbability) * totalWeight);
            if (weights[i] == 0 && probabilities[i] > 0.0)
            {
                weights[i] = 1;
            }
        }
        return weights;
    }
     
    public int[] ApplyLuckToRarities(int[] weights, float luckFactor = 0.1f)
    {
        int numRarities = weights.Length;
        double[] probabilities = WeightsToProbabilities(weights).Select(p => (double)p).ToArray();
        // The lowest rarity gets multiplied by (1 + luckFactor * -1) ^ Luck
        // The highest rarity gets multiplied by (1 + luckFactor * 1) ^ Luck
        // Intermediate rarities get a linear interpolation of the (luckFactor * x) term
        for (int i = 0; i < numRarities; i++)
        {
            // We need to use double here to avoid overflow for high luck values
            double rarityFactor = -1.0 + (2.0 * i / (numRarities - 1));
            double modifier = Math.Pow(1.0 + (luckFactor * rarityFactor), (double)this.LuckClamped);
            if (double.IsInfinity(modifier) || double.IsNaN(modifier))
            {
                modifier = 1e300; // Cap to a very large number
            }
            probabilities[i] *= modifier;
        }
        probabilities = FlattenProbabilities(probabilities);
        return ProbabilitiesToWeights(probabilities, 10000.0);
    }

    public void RefreshLuckModifiers(Player player, Artifact removedArtifact = null)
    {
        this.ArtifactLuckModifiers.Clear();
        var modTestLuckIdx = player.GetArtifacts().IndexOf(ArtifactManager.ModifyLuckTestArtifact);
        var removedArtifactIdx = removedArtifact != null
            ? player.GetArtifacts().IndexOf(removedArtifact)
            : -1;
        // There isn't a 'PostRemoveArtifact' event, so we need to adjust the index if an artifact was removed
        if (removedArtifactIdx >= 0 && removedArtifactIdx < modTestLuckIdx)
        {
            modTestLuckIdx--;
        }
        if (modTestLuckIdx >= 0)
        {
            this.ArtifactLuckModifiers[ArtifactManager.ModifyLuckTestArtifact] = 20f - (modTestLuckIdx * 10f);
        }
    }

    public static float LuckFromArtifact(Artifact artifact)
    {
        if (Instance.ArtifactLuckModifiers.TryGetValue(artifact, out float mod))
        {
            return mod;
        }
        return 0f;
    }
}