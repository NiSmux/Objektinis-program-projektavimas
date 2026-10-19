using System.Numerics;
using Rogue.Server.Combat;
using Rogue.Server.Enemies;
using Rogue.Shared.Models;

namespace Rogue.Server.Bridge;

/// <summary>
/// Refined abstraction: spins the weapon in a full circle of radius Reach around
/// the player. Ignores facing and hits in every direction, but softer than a frontal strike.
/// </summary>
public class SpinAttack : AttackPattern
{
    public SpinAttack(IWeapon weapon) : base(weapon) { }

    protected override float DamageMultiplier => 0.6f;

    protected override bool IsInArea(PlayerState player, Enemy enemy) =>
        Vector2.Distance(enemy.Position, new Vector2(player.X, player.Y)) <= Weapon.Reach;
}
