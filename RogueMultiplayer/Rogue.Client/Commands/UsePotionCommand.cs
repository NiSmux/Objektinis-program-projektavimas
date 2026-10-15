using System.Drawing;
using Rogue.Client.Entities;
using Rogue.Client.Subsystems;

namespace Rogue.Client.Commands;

/// <summary>
/// Roguelike item command: Consumes a healing potion to restore player health.
/// Fully reversible with Undo() (Command pattern).
/// </summary>
public sealed class UsePotionCommand : ICommand
{
    private readonly Player _player;
    private readonly float _healAmount;
    private readonly CombatFeedbackManager? _feedback;

    private float _actualHealed;
    private bool _executed;

    public UsePotionCommand(Player player, float healAmount = 25f, CombatFeedbackManager? feedback = null)
    {
        _player = player;
        _healAmount = healAmount;
        _feedback = feedback;
    }

    public void Execute()
    {
        float previousHp = _player.Health;
        _player.Health = MathF.Min(_player.MaxHealth, _player.Health + _healAmount);
        _actualHealed = _player.Health - previousHp;
        _executed = true;

        if (_actualHealed > 0f)
        {
            _feedback?.AddText($"+{_actualHealed:0} HP", _player.X, _player.Y - 0.5f, Color.LimeGreen, 1.0f);
        }
        else
        {
            _feedback?.AddText("FULL HP", _player.X, _player.Y - 0.5f, Color.White, 0.8f);
        }
    }

    public void Undo()
    {
        if (!_executed || _actualHealed <= 0f) return;

        _player.Health = MathF.Max(0f, _player.Health - _actualHealed);
        _feedback?.AddText($"-{_actualHealed:0} HP (UNDONE)", _player.X, _player.Y - 0.5f, Color.Orange, 1.0f);
    }
}
