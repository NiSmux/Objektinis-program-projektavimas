using Rogue.Server.Combat;
using Rogue.Server.Enemies;
using Rogue.Shared.Models;

namespace Rogue.Server.Bridge;

/// <summary>
/// Bridge (GoF) — Abstraction: decides WHERE an attack lands (its shape).
/// The weapon it holds is the Implementor (<see cref="IWeapon"/>): it decides
/// HOW MUCH damage, how often and how far. Both hierarchies vary independently,
/// so any attack pattern works with any weapon (whip, adapted spear, ...)
/// without classes like FrontalWhipAttack or SpinSpearAttack.
/// </summary>
public abstract class AttackPattern
{
    protected AttackPattern(IWeapon weapon)
    {
        Weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
    }

    /// <summary>The implementor. Can be swapped at runtime without changing the pattern.</summary>
    public IWeapon Weapon { get; set; }

    /// <summary>Seconds between attacks, taken from the weapon.</summary>
    public float Interval => Weapon.Interval;

    /// <summary>
    /// Damages every living enemy inside this pattern's area, at most once per enemy.
    /// </summary>
    /// <returns>Number of enemies hit.</returns>
    public int Execute(PlayerState player, IEnumerable<Enemy> enemies)
    {
        int hits = 0;
        float damage = Weapon.Damage * DamageMultiplier;

        foreach (var enemy in enemies)
        {
            if (enemy.Health <= 0 || !IsInArea(player, enemy))
                continue;

            enemy.TakeDamage(damage);
            hits++;
        }

        return hits;
    }

    /// <summary>Scales the weapon's damage for this pattern (e.g. a wide attack hits softer).</summary>
    protected virtual float DamageMultiplier => 1f;

    /// <summary>True if <paramref name="enemy"/> is inside the area this pattern covers.</summary>
    protected abstract bool IsInArea(PlayerState player, Enemy enemy);
}
