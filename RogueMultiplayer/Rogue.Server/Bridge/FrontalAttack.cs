using System.Numerics;
using Rogue.Server.Combat;
using Rogue.Server.Enemies;
using Rogue.Shared.Models;

namespace Rogue.Server.Bridge;

/// <summary>
/// Refined abstraction: hits an oriented rectangle in front of the player,
/// Reach long and 2 × HalfWidth wide, in any of the 8 facing directions.
/// Enemies standing on top of the player are hit too. This is the game's default attack.
/// </summary>
public class FrontalAttack : AttackPattern
{
    private const float OnTopRadius = 0.7f;
    private const float BehindTolerance = 0.3f;

    public FrontalAttack(IWeapon weapon) : base(weapon) { }

    protected override bool IsInArea(PlayerState player, Enemy enemy)
    {
        Vector2 facing = new(player.FacingDx, player.FacingDy);
        facing = facing.LengthSquared() < 0.0001f ? Vector2.UnitX : Vector2.Normalize(facing);

        // Perpendicular vector for the width of the attack rectangle.
        Vector2 perp = new(-facing.Y, facing.X);

        Vector2 toEnemy = enemy.Position - new Vector2(player.X, player.Y);

        // Project onto the facing vector and the perpendicular vector.
        float forwardDist = Vector2.Dot(toEnemy, facing);
        float sideDist = MathF.Abs(Vector2.Dot(toEnemy, perp));

        bool onTopOfPlayer = toEnemy.LengthSquared() <= OnTopRadius * OnTopRadius;
        bool insideRect = forwardDist >= -BehindTolerance && forwardDist <= Weapon.Reach && sideDist <= Weapon.HalfWidth;

        return onTopOfPlayer || insideRect;
    }
}
