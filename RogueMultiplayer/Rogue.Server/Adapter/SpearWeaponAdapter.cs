using Rogue.Server.Combat;

namespace Rogue.Server.Adapter;

/// <summary>
/// Adapter: makes <see cref="ThirdPartySpear"/> usable wherever the game expects an
/// <see cref="IWeapon"/>. Converts units and supplies the missing HalfWidth value.
/// </summary>
public class SpearWeaponAdapter : IWeapon
{
    /// <summary>Same tile size as the client renderer.</summary>
    private const float PixelsPerTile = 32f;

    /// <summary>A spear is a narrow thrust; the adaptee has no width at all.</summary>
    private const float SpearHalfWidth = 0.4f;

    private readonly ThirdPartySpear _spear;

    public SpearWeaponAdapter(ThirdPartySpear spear) => _spear = spear;

    public float Damage => _spear.GetAttackPower();

    // Attacks per minute -> seconds between attacks.
    public float Interval => 60f / _spear.GetAttacksPerMinute();

    // Pixels -> grid tiles.
    public float Reach => _spear.GetLengthInPixels() / PixelsPerTile;

    public float HalfWidth => SpearHalfWidth;
}
