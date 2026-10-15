namespace Rogue.Shared.Models;

public class PlayerState
{
    public int Id { get; set; }

    public float X { get; set; }

    public float Y { get; set; }

    public float Health { get; set; } = 100f;

    public float MaxHealth { get; set; } = 100f;

    public float FacingDx { get; set; } = 1f;

    public float FacingDy { get; set; } = 0f;

    public float AttackAnimTimer { get; set; } = 0f;
}