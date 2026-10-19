using Rogue.Server.Adapter;
using Rogue.Server.Combat;
using Rogue.Server.Enemies;
using Rogue.Shared.Models;

namespace Rogue.Server.Bridge;

/// <summary>
/// Console demo of the Bridge pattern: 2 attack patterns × 2 weapons = 4 combinations
/// from only 4 classes, then swapping the weapon of an existing pattern at runtime.
/// Run with: dotnet run --project Rogue.Server -- --bridge-demo
/// </summary>
public static class BridgeDemo
{
    public static void Run()
    {
        // Player in the middle facing right; one enemy in front, behind, and beside.
        var player = new PlayerState { X = 4f, Y = 4f, FacingDx = 1f, FacingDy = 0f };
        Console.WriteLine("Player at (4, 4) facing right. Enemies: front (5, 4), behind (3, 4), side (4, 5)\n");

        IWeapon[] weapons = { new WhipWeapon(), new SpearWeaponAdapter(new ThirdPartySpear()) };
        Func<IWeapon, AttackPattern>[] patterns = { w => new FrontalAttack(w), w => new SpinAttack(w) };

        Console.WriteLine("=== Every pattern works with every weapon ===\n");
        foreach (var makePattern in patterns)
        {
            foreach (var weapon in weapons)
            {
                AttackPattern attack = makePattern(weapon);
                List<Enemy> enemies = CreateEnemies();
                int hits = attack.Execute(player, enemies);

                Console.WriteLine($"  {attack.GetType().Name,-13} + {weapon.GetType().Name,-18}" +
                                  $"  hits {hits}  damage taken: front {Lost(enemies[0])}, behind {Lost(enemies[1])}, side {Lost(enemies[2])}");
            }
        }

        Console.WriteLine("\n=== Swapping the implementor at runtime ===\n");
        AttackPattern frontal = new FrontalAttack(new WhipWeapon());
        Console.WriteLine($"  FrontalAttack with {frontal.Weapon.GetType().Name,-18} interval {frontal.Interval:0.00}s");
        frontal.Weapon = new SpearWeaponAdapter(new ThirdPartySpear());
        Console.WriteLine($"  FrontalAttack with {frontal.Weapon.GetType().Name,-18} interval {frontal.Interval:0.00}s\n");
    }

    private static List<Enemy> CreateEnemies()
    {
        var factory = new EnemyFactory();
        return new List<Enemy>
        {
            factory.CreateEnemy(EnemyType.Tank, 1, 5f, 4f), // front
            factory.CreateEnemy(EnemyType.Tank, 2, 3f, 4f), // behind
            factory.CreateEnemy(EnemyType.Tank, 3, 4f, 5f)  // side
        };
    }

    private static string Lost(Enemy enemy) => $"{enemy.MaxHealth - enemy.Health,4:0}";
}
