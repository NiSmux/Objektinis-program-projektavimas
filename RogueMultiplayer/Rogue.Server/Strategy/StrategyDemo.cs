using System.Numerics;
using Rogue.Server.Enemies;

namespace Rogue.Server.Strategy;

/// <summary>
/// Console demo of the Strategy pattern (like MyProgram.java): one enemy object,
/// its movement strategy swapped at runtime, the resulting path printed.
/// Run with: dotnet run --project Rogue.Server -- --strategy-demo
/// </summary>
public static class StrategyDemo
{
    private const float DeltaTime = 0.05f;   // same as the server tick
    private const float PhaseSeconds = 1.6f;
    private const int PrintEveryTicks = 4;   // print every 0.2 s

    private static readonly Vector2 PlayerPosition = new(6f, 6f);

    public static void Run()
    {
        Enemy enemy = new BasicEnemy { Id = 1 };
        Console.WriteLine($"Player at {Format(PlayerPosition)}; enemy speed {enemy.Speed} tiles/s\n");

        // 1. Default strategy assigned in the Enemy constructor.
        RunPhase(enemy, new Vector2(1f, 1f), "default");

        // 2-4. The client (this demo) swaps the strategy on the same object.
        enemy.SetMovementStrategy(new ZigZagStrategy());
        RunPhase(enemy, new Vector2(1f, 1f), "set by client");

        enemy.SetMovementStrategy(new DashStrategy());
        RunPhase(enemy, new Vector2(1f, 1f), "set by client");

        enemy.SetMovementStrategy(new OrbitStrategy());
        RunPhase(enemy, new Vector2(6f, 3f), "set by client");

        // 5. The enemy decides by itself: damage drops it below 25% HP.
        enemy.TakeDamage(80f);
        Console.WriteLine($"Enemy took 80 damage, HP {enemy.Health}/{enemy.MaxHealth}");
        RunPhase(enemy, new Vector2(4f, 4f), "chosen by the enemy itself");
    }

    private static void RunPhase(Enemy enemy, Vector2 start, string decidedBy)
    {
        enemy.X = start.X;
        enemy.Y = start.Y;

        Console.WriteLine($"--- {enemy.GetMovementStrategy().GetType().Name} ({decidedBy}) ---");
        Console.WriteLine($"  t={0f:0.0}s  pos {Format(enemy.Position)}  dist {Distance(enemy):0.00}");

        int ticks = (int)MathF.Round(PhaseSeconds / DeltaTime);
        for (int tick = 1; tick <= ticks; tick++)
        {
            enemy.Update(PlayerPosition, DeltaTime);

            if (tick % PrintEveryTicks == 0)
                Console.WriteLine($"  t={tick * DeltaTime:0.0}s  pos {Format(enemy.Position)}  dist {Distance(enemy):0.00}");
        }

        Console.WriteLine();
    }

    private static float Distance(Enemy enemy) => Vector2.Distance(enemy.Position, PlayerPosition);

    private static string Format(Vector2 v) => $"({v.X,5:0.00}, {v.Y,5:0.00})";
}
