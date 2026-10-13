using System.Drawing;

namespace Rogue.Client.Subsystems;

/// <summary>
/// Data model representing a temporary floating combat message (e.g. damage, heal, spell name).
/// </summary>
public class FloatingCombatText
{
    public string Text { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public Color Color { get; set; } = Color.White;
    public float Lifetime { get; set; } = 1.0f;
    public float MaxLifetime { get; set; } = 1.0f;
}

/// <summary>
/// Subsystem class responsible for managing visual combat feedback, floating damage numbers,
/// and transient combat announcements.
/// </summary>
public class CombatFeedbackManager
{
    private readonly List<FloatingCombatText> _activeTexts = new();
    private readonly object _lock = new();

    public void AddText(string text, float x, float y, Color color, float duration = 1.0f)
    {
        lock (_lock)
        {
            _activeTexts.Add(new FloatingCombatText
            {
                Text = text,
                X = x,
                Y = y,
                Color = color,
                Lifetime = duration,
                MaxLifetime = duration
            });
        }
    }

    public void Update(float dt)
    {
        lock (_lock)
        {
            for (int i = _activeTexts.Count - 1; i >= 0; i--)
            {
                var item = _activeTexts[i];
                item.Lifetime -= dt;
                item.Y -= 0.6f * dt; // Float gently upwards
                if (item.Lifetime <= 0f)
                {
                    _activeTexts.RemoveAt(i);
                }
            }
        }
    }

    public List<FloatingCombatText> GetActiveTextsSnapshot()
    {
        lock (_lock)
        {
            return new List<FloatingCombatText>(_activeTexts);
        }
    }
}
