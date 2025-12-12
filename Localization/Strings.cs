using AotenjoAPIPlus.Localization;
using AotenjoAPIPlus.Tooltips;
using LuckMod.Artifacts;
using LuckMod.Managers;
using LuckMod.Utils;

namespace LuckMod.Localization;

public static class Strings
{
    public static void RegisterTooltips()
    {
        TooltipManager.RegisterStyleTooltip("ingoh_luck_mod_luck",
            () => Loc("ingoh.luck_mod:tooltip.luck.header"),
            () => Loc("ingoh.luck_mod:tooltip.luck.content"),
            Color.green
        );
    }

    public static void RegisterSubstitutions()
    {
        RegisterSubstitution("ingoh.luck_mod:test_artifact_luck_info",
            (Func<string>)(() => Loc("ingoh.luck_mod:dynamic.luck.from_artifact"))
        );
        RegisterSubstitution("ingoh.luck_mod:luck_info",
            (Func<string>)(() => Loc("ingoh.luck_mod:dynamic.luck.current"))
        );
        RegisterSubstitution("ingoh.luck_mod:test_artifact_luck_value",
            (Func<string>)(() => ColorUtils.ColorFromSign(LuckManager.LuckFromArtifact(ArtifactManager.ModifyLuckTestArtifact)))
        );
        RegisterSubstitution("ingoh.luck_mod:luck_value",
            (Func<string>)(() => ColorUtils.ColorFromSign(LuckManager.Instance.Luck))
        );
    }
}