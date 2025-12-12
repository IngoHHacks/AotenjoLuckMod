namespace LuckMod.Utils;

public static class ColorUtils
{
    public static string ColorFromSign(int number, bool prependPlus = false)
    {
        if (number > 0)
        {
            return $"<color=green>{(prependPlus ? "+" : "")}{number}</color>";
        }
        if (number < 0)
        {
            return $"<color=red>{number}</color>";
        }
        return number.ToString();
    }
    
    public static string ColorFromSign(float number, bool prependPlus = false)
    {
        if (number > 0f)
        {
            return $"<color=green>{(prependPlus ? "+" : "")}{number}</color>";
        }
        if (number < 0f)
        {
            return $"<color=red>{number}</color>";
        }
        return $"{number:0.##}";
    }
    
    public static string ColorFromBool(bool value)
    {
        return value ? "<color=green>True</color>" : "<color=red>False</color>";
    }
}