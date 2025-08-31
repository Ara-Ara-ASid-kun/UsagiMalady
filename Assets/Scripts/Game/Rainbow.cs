using UnityEngine;

public enum ShapeType { Square, Circle, Triangle }
public enum RainbowColor { Red, Orange, Yellow, Green, Blue, Indigo, Violet }

public static class Rainbow
{
    public static Color ToColor(this RainbowColor rc)
    {
        switch (rc)
        {
            case RainbowColor.Red:    return new Color(0.95f, 0.15f, 0.2f);
            case RainbowColor.Orange: return new Color(1.0f, 0.55f, 0.0f);
            case RainbowColor.Yellow: return new Color(1.0f, 0.92f, 0.23f);
            case RainbowColor.Green:  return new Color(0.2f, 0.8f, 0.3f);
            case RainbowColor.Blue:   return new Color(0.2f, 0.4f, 1.0f);
            case RainbowColor.Indigo: return new Color(0.29f, 0.0f, 0.51f);
            case RainbowColor.Violet: return new Color(0.56f, 0.0f, 1.0f);
            default: return Color.white;
        }
    }
}
