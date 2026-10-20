namespace Rogue.Shared.Models;

public class PlayerState
{
    public int Id { get; set; }

    public float X { get; set; }

    public float Y { get; set; }

    public float Health { get; set; } = 1000f;

    public float MaxHealth { get; set; } = 1000f;

    public float FacingDx { get; set; } = 1f;

    public float FacingDy { get; set; } = 0f;

    public float AttackAnimTimer { get; set; } = 0f;

    public float WeaponDamage { get; set; } = 25f;

    public float WeaponInterval { get; set; } = 1.0f;

    public float WeaponReach { get; set; } = 1.6f;

    public float WeaponHalfWidth { get; set; } = 1.0f;

    public string WeaponName { get; set; } = "Whip";

    public string BaseWeaponName { get; set; } = "Whip";
}
