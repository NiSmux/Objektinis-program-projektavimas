namespace Rogue.Shared.Models;

public class EnemyState
{
    public int Id { get; set; }

    public float X { get; set; }

    public float Y { get; set; }

    public EnemyType Type { get; set; }

    public float Health { get; set; }

    public float MaxHealth { get; set; }
}
