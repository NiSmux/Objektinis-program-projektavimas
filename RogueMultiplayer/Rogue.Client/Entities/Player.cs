namespace Rogue.Client.Entities;

public class Player
{
    public float X { get; set; } = 3;
    public float Y { get; set; } = 3;

    public float Speed { get; set; } = 4f;

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
