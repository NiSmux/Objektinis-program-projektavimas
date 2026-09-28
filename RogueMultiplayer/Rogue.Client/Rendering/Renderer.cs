using Rogue.Client.Entities;
using Rogue.Shared.Models;

namespace Rogue.Client.Rendering;

public class Renderer
{
    public const int TileSize = 32;
    public const int GridSize = 9;

    public void Draw(Graphics g, IEnumerable<Player> players, IEnumerable<EnemyState> enemies)
    {
        DrawGrid(g);

        foreach (var enemy in enemies)
            DrawEnemy(g, enemy);

        foreach (var player in players)
            DrawPlayer(g, player);
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
            EnemyType.Fast  => Brushes.Yellow,
            EnemyType.Tank  => Brushes.MediumPurple,
            _               => Brushes.White
        };

        string label = enemy.Type switch
        {
            EnemyType.Basic => "B",
            EnemyType.Fast  => "F",
            EnemyType.Tank  => "T",
            _               => "?"
        };

        float px = enemy.X * TileSize;
        float py = enemy.Y * TileSize;

        g.FillRectangle(brush, px, py, TileSize, TileSize);
        g.DrawString(label, SystemFonts.DefaultFont, Brushes.Black, px + 10, py + 8);
    }
}
