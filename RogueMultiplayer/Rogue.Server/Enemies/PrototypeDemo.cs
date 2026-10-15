using System.Numerics;
using Rogue.Server.Strategy;
using Rogue.Shared.Models;

namespace Rogue.Server.Enemies;

/// <summary>
/// Console demo of the Prototype pattern: shallow vs deep copy of an enemy,
/// then spawning enemies from the prototype registry.
/// Run with: dotnet run --project Rogue.Server -- --prototype-demo
/// </summary>
public static class PrototypeDemo
{
    private const float DeltaTime = 0.05f;
    private static readonly Vector2 Player = new(8f, 8f);

    public static void Run()
    {
        Enemy original = new TankEnemy { Id = 1, X = 1f, Y = 1f };

        Console.WriteLine("=== Shallow copy (MemberwiseClone) ===\n");
        Enemy shallow = original.ShallowCopy();
        shallow.Id = 2;
        shallow.X = 5f;
        PrintComparison(original, shallow);

        Console.WriteLine("=== Deep copy (MemberwiseClone + cloned strategies) ===\n");
        Enemy deep = original.DeepCopy();
        deep.Id = 3;
        deep.X = 5f;
        PrintComparison(original, deep);

        // Why it matters: DashStrategy keeps its wind-up timer inside the strategy object,
        // so a shallow copy shares that timer with the original.
        Console.WriteLine("=== Effect of a shared strategy (DashStrategy timer) ===\n");
        Enemy dasher = new BasicEnemy { Id = 10, X = 1f, Y = 1f };
        dasher.SetMovementStrategy(new DashStrategy(windUpTime: 0.5f));
        Enemy shallowDasher = dasher.ShallowCopy();
        Enemy deepDasher = dasher.DeepCopy();

        // Only the ORIGINAL is updated for 0.5 s; its wind-up finishes.
        for (int i = 0; i < 10; i++)
            dasher.Update(Player, DeltaTime);

        // One tick for each copy: the shallow copy dashes immediately (timer already consumed
        // by the original), the deep copy is still winding up and stands still.
        Vector2 shallowBefore = shallowDasher.Position;
        Vector2 deepBefore = deepDasher.Position;
        shallowDasher.Update(Player, DeltaTime);
        deepDasher.Update(Player, DeltaTime);
        Console.WriteLine($"  Shallow copy moved {Vector2.Distance(shallowBefore, shallowDasher.Position):0.00} tiles in one tick (shares the original's timer)");
        Console.WriteLine($"  Deep copy    moved {Vector2.Distance(deepBefore, deepDasher.Position):0.00} tiles in one tick (own timer, still winding up)\n");

        Console.WriteLine("=== Spawning from EnemyPrototypeRegistry ===\n");
        var registry = new EnemyPrototypeRegistry(new EnemyFactory());
        int nextId = 100;
        foreach (var type in Enum.GetValues<EnemyType>())
        {
            Enemy prototype = registry.GetPrototype(type);
            Enemy spawned = registry.CreateEnemy(type, nextId++, 2f, 3f);
            Console.WriteLine($"  {type,-6} id={spawned.Id}  HP {spawned.Health}/{spawned.MaxHealth}  speed {spawned.Speed}" +
                              $"  same object as prototype: {ReferenceEquals(prototype, spawned)}" +
                              $"  same strategy: {ReferenceEquals(prototype.GetMovementStrategy(), spawned.GetMovementStrategy())}");
        }
        Console.WriteLine();
    }

    private static void PrintComparison(Enemy original, Enemy copy)
    {
        Console.WriteLine($"  Original: id={original.Id}  X={original.X}  object #{original.GetHashCode()}  strategy #{original.GetMovementStrategy().GetHashCode()}");
        Console.WriteLine($"  Copy:     id={copy.Id}  X={copy.X}  object #{copy.GetHashCode()}  strategy #{copy.GetMovementStrategy().GetHashCode()}");
        Console.WriteLine($"  Same enemy object:    {ReferenceEquals(original, copy)}");
        Console.WriteLine($"  Same strategy object: {ReferenceEquals(original.GetMovementStrategy(), copy.GetMovementStrategy())}\n");
    }
}
