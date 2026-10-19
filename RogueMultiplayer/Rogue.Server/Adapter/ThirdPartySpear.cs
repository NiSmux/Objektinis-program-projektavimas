namespace Rogue.Server.Adapter;

/// <summary>
/// Adaptee: a weapon class from an "external library" that we cannot change.
/// It uses different units (pixels, attacks per minute) and has only 3 methods,
/// while our <see cref="Combat.IWeapon"/> expects 4 members.
/// </summary>
public class ThirdPartySpear
{
    public int GetAttackPower() => 40;

    public int GetAttacksPerMinute() => 40;

    public int GetLengthInPixels() => 80;
}
