using LuckMod.Managers;
using System.Reflection.Emit;

namespace LuckMod.Patches;

[HarmonyPatch]
internal class LuckPatches
{
    [HarmonyPatch(typeof(Player), nameof(Player.TryDrawRandomArtifact))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Player_TryDrawRandomArtifact_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var artifactListCtor = AccessTools.Constructor(typeof(List<Artifact>));
        var dictAdderMethod = AccessTools.Method(typeof(Dictionary<int, int[]>), nameof(Dictionary<int, int[]>.Add));
        var matcher = new CodeMatcher(codes)
            .MatchForward(false,
                new CodeMatch(OpCodes.Callvirt, dictAdderMethod),
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Newobj, artifactListCtor)
            )
            .ThrowIfInvalid("Failed to find target instruction sequence in Player.TryDrawRandomArtifact")
            .Advance(2) // After Ldloc_2
            .Insert(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LuckPatches), nameof(RewriteLuckDict))),
                new CodeInstruction(OpCodes.Stloc_2), // Store the modified dictionary back
                new CodeInstruction(OpCodes.Ldloc_2)  // Load the modified dictionary again since Stloc removes it from the stack
            );
        return matcher.InstructionEnumeration();
    }
    
    private static Dictionary<int, int[]> RewriteLuckDict(Dictionary<int, int[]> originalDict)
    {
        var newDict = new Dictionary<int, int[]>();
        foreach (var kvp in originalDict)
        {
            newDict[kvp.Key] = LuckManager.Instance.ApplyLuckToRarities(kvp.Value);
        }
        return newDict;
    }
    
    [HarmonyPatch(typeof(YakuPack), nameof(YakuPack.Draw))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> YakuPack_Draw_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var matcher = new CodeMatcher(codes)
            .MatchForward(true,
                new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(LotteryPool<List<YakuType>>))),
                new CodeMatch(OpCodes.Stloc_3)
            )
            .ThrowIfInvalid("Failed to find target instruction sequence in YakuPack.Draw")
            .InsertAfter(
                new CodeInstruction(OpCodes.Ldarg_0), // Load 'this' (YakuPack instance)
                new CodeInstruction(OpCodes.Ldloc_2), // Load the luck values array
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LuckPatches), nameof(AdjustLuckValues))),
                new CodeInstruction(OpCodes.Stloc_2)  // Store the modified array back
            );
        return matcher.InstructionEnumeration();
    }
    
    private static int[] AdjustLuckValues(YakuPack yakuPack, int[] luckValues)
    {
        var newLuckValues = LuckManager.Instance.ApplyLuckToRarities(luckValues);
        if (yakuPack.common?.Count == 0)
        {
            luckValues[0] = 0;
        }
        if (yakuPack.rare?.Count == 0)
        {
            luckValues[1] = 0;
        }
        if (yakuPack.epic?.Count == 0)
        {
            luckValues[2] = 0;
        }
        if (yakuPack.legendary?.Count == 0)
        {
            luckValues[3] = 0;
        }
        if (yakuPack.ancient?.Count == 0)
        {
            luckValues[4] = 0;
        }
        return newLuckValues;
    }
}