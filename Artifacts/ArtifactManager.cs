using LuckMod.Managers;

namespace LuckMod.Artifacts;

public static class ArtifactManager
{
    public static Artifact ModifyLuckTestArtifact;
    
    public static void RegisterArtifacts()
    {
        ModifyLuckTestArtifact = LuaArtifactBuilder.Create("ingoh.luck_mod:modify_luck_test", Rarity.EPIC)
            .WithName((_, _) => Loc("artifact_ingoh.luck_mod:modify_luck_test_name"))
            .WithDescription((_, _, _) => Loc("ingoh.luck_mod:modify_luck_test.description"))
            .WithSpriteID((_, _) => "artifact:ingoh.Aotenjo.luck_mod:modify_luck_test_sprite")
            .OnSubscribeToPlayer((player, _) =>
            {
                LuckManager.Instance.RefreshLuckModifiers(player);
                player.PreRemoveArtifact += RefreshLuckModifiersAfterRemoval;
            })
            .OnUnsubscribeToPlayer((player, _) =>
            {
                LuckManager.Instance.RefreshLuckModifiers(player);
                player.PreRemoveArtifact -= RefreshLuckModifiersAfterRemoval;
            })
            .BuildAndRegister();
    }
    
    private static void RefreshLuckModifiersAfterRemoval(PlayerArtifactEvent evt)
    {
        LuckManager.Instance.RefreshLuckModifiers(evt.player, evt.artifact);
    }
}