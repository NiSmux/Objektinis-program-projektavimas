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
        float px = player.X * TileSize;
        float py = player.Y * TileSize;

        // Player body
        g.FillEllipse(
            player.Health > 0 ? Brushes.DodgerBlue : Brushes.DimGray,
            px,
            py,
            TileSize,
            TileSize);

        // Facing direction & visual line of attack
        float fLen = MathF.Sqrt(player.FacingDx * player.FacingDx + player.FacingDy * player.FacingDy);
        if (fLen > 0.001f && player.Health > 0)
        {
            float ndx = player.FacingDx / fLen;
            float ndy = player.FacingDy / fLen;

            float centerX = px + TileSize / 2f;
            float centerY = py + TileSize / 2f;

            // Attack line & slash ONLY appears when an attack is actively being executed
            if (player.AttackAnimTimer > 0f)
            {
                float reachPixels = 1.2f * TileSize;
                float endX = centerX + ndx * reachPixels;
                float endY = centerY + ndy * reachPixels;

                // Bright golden attack beam
                using var attackPen = new Pen(Color.FromArgb(240, 255, 215, 0), 3f);
                g.DrawLine(attackPen, centerX, centerY, endX, endY);

                // Whip slash cross-bar showing sweep area
                float perpX = -ndy;
                float perpY = ndx;
                float halfWidthPixels = 0.6f * TileSize;
                using var slashPen = new Pen(Color.FromArgb(255, 255, 240, 120), 2.5f);
                g.DrawLine(
                    slashPen,
                    endX - perpX * halfWidthPixels,
                    endY - perpY * halfWidthPixels,
                    endX + perpX * halfWidthPixels,
                    endY + perpY * halfWidthPixels);
            }

            // Always visible: small indicator dot on the player showing facing orientation
            float indicatorX = centerX + ndx * (TileSize / 2f - 4) - 3;
            float indicatorY = centerY + ndy * (TileSize / 2f - 4) - 3;
            g.FillEllipse(Brushes.White, indicatorX, indicatorY, 6, 6);
        }

        // Health bar above player
        DrawHealthBar(g, px, py - 6, TileSize, 4, player.Health, player.MaxHealth, Brushes.LimeGreen);
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

        // Enemy health bar above enemy
        DrawHealthBar(g, px, py - 6, TileSize, 4, enemy.Health, enemy.MaxHealth, Brushes.Red);
    }

    private static void DrawHealthBar(Graphics g, float x, float y, float width, float height, float current, float max, Brush fillBrush)
    {
        if (max <= 0f) return;

        // Background
        g.FillRectangle(Brushes.Black, x, y, width, height);

        // Health fill
        float pct = Math.Clamp(current / max, 0f, 1f);
        if (pct > 0f)
        {
            g.FillRectangle(fillBrush, x, y, width * pct, height);
        }

        // Border
        g.DrawRectangle(Pens.DarkSlateGray, x, y, width, height);
    }
}