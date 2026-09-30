namespace Rogue.Server.Map;

/// <summary>
/// Abstract factory: creates a family of matching tiles for one theme.
/// </summary>
public interface IMapThemeFactory
{
    string Name { get; }

    Wall CreateWall(int x, int y);

    Floor CreateFloor(int x, int y);
}