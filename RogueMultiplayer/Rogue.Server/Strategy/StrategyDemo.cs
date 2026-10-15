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

    private static readonly Vector2 StandingPlayer = new(6f, 6f);

    // Player running right along the bottom row, for the Chase vs Intercept comparison.
    private static readonly Vector2 RunningPlayerStart = new(1f, 7f);
    private static readonly Vector2 RunningPlayerVelocity = new(2f, 0f);

    public static void Run()
    {
        Enemy enemy = new BasicEnemy { Id = 1 };
        Console.WriteLine($"Enemy speed {enemy.Speed} tiles/s\n");
        Console.WriteLine($"=== Player standing still at {Format(StandingPlayer)} ===\n");

        // 1. Default strategy assigned in the Enemy constructor.
        RunPhase(enemy, new Vector2(1f, 1f), "default", StandingPlayer, Vector2.Zero);

        // 2-4. The client (this demo) swaps the strategy on the same object.
        enemy.SetMovementStrategy(new ZigZagStrategy());
        RunPhase(enemy, new Vector2(1f, 1f), "set by client", StandingPlayer, Vector2.Zero);

        enemy.SetMovementStrategy(new DashStrategy());
        RunPhase(enemy, new Vector2(1f, 1f), "set by client", StandingPlayer, Vector2.Zero);

        enemy.SetMovementStrategy(new OrbitStrategy());
        RunPhase(enemy, new Vector2(6f, 3f), "set by client", StandingPlayer, Vector2.Zero);

        Console.WriteLine($"=== Player running right from {Format(RunningPlayerStart)} at {RunningPlayerVelocity.X} tiles/s ===\n");

        // 5. Chase against a moving player: it aims where the player is now and trails behind.
        enemy.SetMovementStrategy(new ChaseStrategy());
        RunPhase(enemy, new Vector2(5f, 2f), "set by client", RunningPlayerStart, RunningPlayerVelocity);

        // 6. The enemy decides by itself: damage drops it below 25% HP, it gets enraged.
        enemy.TakeDamage(80f);
        Console.WriteLine($"Enemy took 80 damage, HP {enemy.Health}/{enemy.MaxHealth}");
        RunPhase(enemy, new Vector2(5f, 2f), "chosen by the enemy itself", RunningPlayerStart, RunningPlayerVelocity);
    }

    private static void RunPhase(Enemy enemy, Vector2 enemyStart, string decidedBy,
                                 Vector2 playerStart, Vector2 playerVelocity)
    {
        enemy.X = enemyStart.X;
        enemy.Y = enemyStart.Y;
        Vector2 player = playerStart;

        Console.WriteLine($"--- {enemy.GetMovementStrategy().GetType().Name} ({decidedBy}) ---");
        Print(0f, enemy, player);

        int ticks = (int)MathF.Round(PhaseSeconds / DeltaTime);
        for (int tick = 1; tick <= ticks; tick++)
        {
            player += playerVelocity * DeltaTime;
            enemy.Update(player, DeltaTime);

            if (tick % PrintEveryTicks == 0)
                Print(tick * DeltaTime, enemy, player);
        }

        Console.WriteLine();
    }

    private static void Print(float time, Enemy enemy, Vector2 player) =>
        Console.WriteLine($"  t={time:0.0}s  enemy {Format(enemy.Position)}  player {Format(player)}  dist {Vector2.Distance(enemy.Position, player):0.00}");

    private static string Format(Vector2 v) => $"({v.X,5:0.00}, {v.Y,5:0.00})";
}
