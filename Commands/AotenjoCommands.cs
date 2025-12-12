using Aotenjo.Console;
using LuckMod.Managers;

namespace LuckMod.Commands;

public class AotenjoCommands
{
    [AotenjoCommand("toggleLuckDebug")]
    public static void ToggleLuckDebug()
    {
        LuckManager.Instance.ShowDebugInfo = !LuckManager.Instance.ShowDebugInfo;
    }
    
    [AotenjoCommand("setBaseLuck", "ToInt")]
    public static void SetBaseLuck(int amount)
    {
        LuckManager.Instance.ManualLuck = amount;
    }
}