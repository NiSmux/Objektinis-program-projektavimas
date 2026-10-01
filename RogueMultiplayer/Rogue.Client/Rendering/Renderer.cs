using Rogue.Client.Entities;
using Rogue.Shared.Models;

namespace Rogue.Client.Rendering;

public class Renderer
{
    public const int TileSize = 32;
    public const int GridSize = 9;

    private static readonly Brush WaterWallBrush = new SolidBrush(Color.FromArgb(30, 80, 170));
    private static readonly Brush WaterFloorBrush = new SolidBrush(Color.FromArgb(130, 180, 235));
    private static readonly Brush LavaWallBrush = new SolidBrush(Color.FromArgb(210, 60, 0));
    private static readonly Brush LavaFloorBrush = new SolidBrush(Color.FromArgb(90, 35, 25));

    public void Draw(
        Graphics g,
        IEnumerable<Player> players,
        IEnumerable<EnemyState> enemies,
        IEnumerable<TileState> tiles)
    {
        foreach (var tile in tiles)
            DrawTile(g, tile);

        DrawGrid(g);

        foreach (var enemy in enemies)
            DrawEnemy(g, enemy);

        foreach (var player in players)
            DrawPlayer(g, player);
    }

    private void DrawTile(Graphics g, TileState tile)
    {
        Brush brush = tile.Type switch
        {
            TileType.WaterWall => WaterWallBrush,
            TileType.WaterFloor => WaterFloorBrush,
            TileType.LavaWall => LavaWallBrush,
            TileType.LavaFloor => LavaFloorBrush,
            _ => Brushes.Magenta
        };

        int px = tile.X * TileSize;
        int py = tile.Y * TileSize;

        g.FillRectangle(brush, px, py, TileSize, TileSize);

        bool isWall = tile.Type is TileType.WaterWall or TileType.LavaWall;
        if (isWall)
        {
            g.DrawRectangle(Pens.Black, px + 2, py + 2, TileSize - 4, TileSize - 4);
            string mark = tile.Type == TileType.WaterWall ? "~" : "^";
            g.DrawString(mark, SystemFonts.DefaultFont, Brushes.White, px + 11, py + 9);
        }
    }

    private void DrawGrid(Graphics g)
    {
        Pen pen = Pens.Gray;

        for (int i = 0; i <= GridSize; i++)
        {
            g.DrawLine(pen, i * TileSize, 0, i * TileSize, GridSize * TileSize);
            g.DrawLine(pen, 0, i * TileSize, GridSize * TileSize, i * TileSize);
        }
    }

    private void DrawPlayer(Graphics g, Player player)
    {
        g.FillEllipse(
            Brushes.DodgerBlue,
            player.X * TileSize,
            player.Y * TileSize,
            TileSize,
            TileSize);
    }

    private void DrawEnemy(Graphics g, EnemyState enemy)
    {
        Brush brush = enemy.Type switch
        {
            EnemyType.Basic => Brushes.OrangeRed,
            EnemyType.Fast => Brushes.Yellow,
            EnemyType.Tank => Brushes.MediumPurple,
            _ => Brushes.White
        };

        string label = enemy.Type switch
        {
            EnemyType.Basic => "B",
            EnemyType.Fast => "F",
            EnemyType.Tank => "T",
            _ => "?"
        };

        float px = enemy.X * TileSize;
        float py = enemy.Y * TileSize;

        g.FillRectangle(brush, px, py, TileSize, TileSize);
        g.DrawString(label, SystemFonts.DefaultFont, Brushes.Black, px + 10, py + 8);
    }
}