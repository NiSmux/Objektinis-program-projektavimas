using Rogue.Server.Combat;
using Rogue.Server.Game;
using Rogue.Shared.Models;

namespace Rogue.Server.Adapter;

/// <summary>
/// Console demo of the Adapter pattern: the external spear is plugged into the
/// game through <see cref="SpearWeaponAdapter"/> and used exactly like the whip.
/// Run with: dotnet run --project Rogue.Server -- --adapter-demo
/// </summary>
public static class AdapterDemo
{
    public static void Run()
    {
        var spear = new ThirdPartySpear();

        Console.WriteLine("=== Adaptee: ThirdPartySpear (3 methods, external units) ===\n");
        Console.WriteLine($"  GetAttackPower()      = {spear.GetAttackPower()}");
        Console.WriteLine($"  GetAttacksPerMinute() = {spear.GetAttacksPerMinute()}");
        Console.WriteLine($"  GetLengthInPixels()   = {spear.GetLengthInPixels()}\n");

        Console.WriteLine("=== Target: IWeapon (4 members, game units) ===\n");
        Print(new WhipWeapon());
        Print(new SpearWeaponAdapter(spear));
        Console.WriteLine();

        Console.WriteLine("=== Equipping a player: the server only sees IWeapon ===\n");
        var client = new ConnectedClient(new PlayerState { Id = 1 });
        Console.WriteLine($"  Default weapon:  {client.Weapon.GetType().Name}");
        client.Weapon = new SpearWeaponAdapter(spear);
        Console.WriteLine($"  After equipping: {client.Weapon.GetType().Name}  (reach {client.Weapon.Reach:0.00} tiles)\n");
    }

    private static void Print(IWeapon weapon) =>
        Console.WriteLine($"  {weapon.GetType().Name,-18}  Damage {weapon.Damage,4:0}  Interval {weapon.Interval:0.00}s" +
                          $"  Reach {weapon.Reach:0.00}  HalfWidth {weapon.HalfWidth:0.00}");
}
