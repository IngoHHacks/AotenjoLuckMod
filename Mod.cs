using LuckMod.Artifacts;
using LuckMod.Localization;

namespace LuckMod
{
    public class Mod : HarmonyMod
    {
        public override string ModName => "Luck Mod";
        public override string ModAuthor => "IngoH";
        public override string ModVersion => "1.0.0";
        public override string ModGuid => "ingoh.Aotenjo.LuckMod";
        
        public override void Init()
        {
            Logger.Log("Luck Mod initialized.");
            ArtifactManager.RegisterArtifacts();
            Strings.RegisterTooltips();
            Strings.RegisterSubstitutions();
        }
    }
}