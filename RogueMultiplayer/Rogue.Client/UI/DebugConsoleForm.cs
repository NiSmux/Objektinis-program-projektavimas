using System.Drawing;
using Rogue.Client.Facade;

namespace Rogue.Client.UI;

/// <summary>
/// Client class #2 of the Facade:
/// An interactive diagnostics & developer console window that consumes <see cref="IGameEngineFacade"/>
/// to inspect runtime game metrics, trigger actions remotely, upgrade weapons via the Decorator pattern,
/// and display event logs.
/// </summary>
public sealed class DebugConsoleForm : Form
{
    private readonly IGameEngineFacade _facade;

    private readonly Label _lblStatus = new();
    private readonly Label _lblPlayer = new();
    private readonly Label _lblHealth = new();
    private readonly Label _lblEnemies = new();
    private readonly Label _lblWeaponStats = new();
    private readonly Button _btnUpgradeDmg = new();
    private readonly Button _btnUpgradeSpeed = new();
    private readonly Button _btnUpgradeRange = new();
    private readonly Button _btnResetWeapon = new();
    private readonly ListBox _lstLog = new();
    private readonly System.Windows.Forms.Timer _refreshTimer = new();

    public DebugConsoleForm(IGameEngineFacade facade)
    {
        _facade = facade;

        Text = "Rogue Developer Console (F1)";
        Size = new Size(380, 560);
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(100, 100);
        TopMost = true;

        InitializeComponents();

        _facade.NotificationReceived += OnNotificationReceived;
        _facade.StateChanged += UpdateMetrics;

        _refreshTimer.Interval = 200;
        _refreshTimer.Tick += (s, e) => UpdateMetrics();
        _refreshTimer.Start();
    }

    private void InitializeComponents()
    {
        var panelInfo = new GroupBox
        {
            Text = "Dungeon Metrics",
            Location = new Point(12, 12),
            Size = new Size(340, 115)
        };

        _lblStatus.Location = new Point(12, 22);
        _lblStatus.Size = new Size(310, 20);
        _lblStatus.Text = "Status: Connecting...";

        _lblPlayer.Location = new Point(12, 44);
        _lblPlayer.Size = new Size(310, 20);
        _lblPlayer.Text = "Player Pos: (0, 0)";

        _lblHealth.Location = new Point(12, 66);
        _lblHealth.Size = new Size(310, 20);
        _lblHealth.Text = "Health: 100 / 100";

        _lblEnemies.Location = new Point(12, 88);
        _lblEnemies.Size = new Size(310, 20);
        _lblEnemies.Text = "Enemies: 0";

        panelInfo.Controls.AddRange([_lblStatus, _lblPlayer, _lblHealth, _lblEnemies]);

        // Weapon Upgrades Group (Decorator Pattern)
        var panelUpgrades = new GroupBox
        {
            Text = "Weapon Upgrades (Decorator Pattern)",
            Location = new Point(12, 135),
            Size = new Size(340, 115)
        };

        _lblWeaponStats.Location = new Point(10, 18);
        _lblWeaponStats.Size = new Size(320, 32);
        _lblWeaponStats.Text = "Base: Whip | Current: Whip\nDamage: 25.0 | Interval: 1.00s | Reach: 1.60 | Width: 1.00";

        _btnUpgradeDmg.Text = "+10 Damage";
        _btnUpgradeDmg.Location = new Point(10, 52);
        _btnUpgradeDmg.Size = new Size(98, 28);
        _btnUpgradeDmg.Click += (s, e) => _facade.UpgradeWeapon("damage");

        _btnUpgradeSpeed.Text = "x1.5 Speed";
        _btnUpgradeSpeed.Location = new Point(114, 52);
        _btnUpgradeSpeed.Size = new Size(112, 28);
        _btnUpgradeSpeed.Click += (s, e) => _facade.UpgradeWeapon("speed");

        _btnUpgradeRange.Text = "+Range";
        _btnUpgradeRange.Location = new Point(232, 52);
        _btnUpgradeRange.Size = new Size(98, 28);
        _btnUpgradeRange.Click += (s, e) => _facade.UpgradeWeapon("range");

        _btnResetWeapon.Text = "Reset Weapon (Base Whip)";
        _btnResetWeapon.Location = new Point(10, 82);
        _btnResetWeapon.Size = new Size(320, 24);
        _btnResetWeapon.Click += (s, e) => _facade.UpgradeWeapon("reset");

        panelUpgrades.Controls.AddRange([_lblWeaponStats, _btnUpgradeDmg, _btnUpgradeSpeed, _btnUpgradeRange, _btnResetWeapon]);

        // Action Buttons calling Facade directly
        var btnDash = new Button
        {
            Text = "Dash (Space)",
            Location = new Point(12, 258),
            Size = new Size(105, 30)
        };
        btnDash.Click += (s, e) => _facade.Dash();

        var btnRecall = new Button
        {
            Text = "Recall (Z)",
            Location = new Point(125, 258),
            Size = new Size(105, 30)
        };
        btnRecall.Click += (s, e) => _facade.Recall();

        var btnPotion = new Button
        {
            Text = "Potion (Q)",
            Location = new Point(238, 258),
            Size = new Size(114, 30)
        };
        btnPotion.Click += (s, e) => _facade.UsePotion();

        var btnReconnect = new Button
        {
            Text = "Reconnect Server",
            Location = new Point(12, 294),
            Size = new Size(340, 28)
        };
        btnReconnect.Click += (s, e) => _ = _facade.StartAsync();

        // Event Log
        var lblLog = new Label
        {
            Text = "Action / Combat Log:",
            Location = new Point(12, 328),
            Size = new Size(340, 18)
        };

        _lstLog.Location = new Point(12, 348);
        _lstLog.Size = new Size(340, 150);

        Controls.AddRange([panelInfo, panelUpgrades, btnDash, btnRecall, btnPotion, btnReconnect, lblLog, _lstLog]);
    }

    private void UpdateMetrics()
    {
        if (InvokeRequired)
        {
            BeginInvoke(UpdateMetrics);
            return;
        }

        var player = _facade.LocalPlayer;
        _lblStatus.Text = $"Status: {_facade.ConnectionStatus} (ID: {_facade.LocalPlayerId})";
        _lblPlayer.Text = $"Player Pos: ({player.X:0.00}, {player.Y:0.00}) Facing: ({player.FacingDx:0.0}, {player.FacingDy:0.0})";
        _lblHealth.Text = $"Health: {player.Health:0} / {player.MaxHealth:0}";
        _lblEnemies.Text = $"Active Enemies: {_facade.EnemyCount} | Other Players: {_facade.PlayerCount}";
        _lblWeaponStats.Text = $"Base: {player.BaseWeaponName} | Stack: {player.WeaponName}\n" +
                               $"Damage: {player.WeaponDamage:0.0} | Interval: {player.WeaponInterval:0.00}s | " +
                               $"Reach: {player.WeaponReach:0.00} | Width: {player.WeaponHalfWidth:0.00}";

        bool canSpeed = player.WeaponInterval > 0.151f;
        _btnUpgradeSpeed.Enabled = canSpeed;
        _btnUpgradeSpeed.Text = canSpeed ? "x1.5 Speed" : "Speed (MAX)";

        bool canDmg = player.WeaponDamage < 249.9f;
        _btnUpgradeDmg.Enabled = canDmg;
        _btnUpgradeDmg.Text = canDmg ? "+10 Damage" : "Dmg (MAX)";

        bool canRange = player.WeaponReach < 4.99f;
        _btnUpgradeRange.Enabled = canRange;
        _btnUpgradeRange.Text = canRange ? "+Range" : "Range (MAX)";
    }

    private void OnNotificationReceived(string message, Color color)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => OnNotificationReceived(message, color));
            return;
        }

        string time = DateTime.Now.ToString("HH:mm:ss");
        _lstLog.Items.Insert(0, $"[{time}] {message}");

        if (_lstLog.Items.Count > 50)
            _lstLog.Items.RemoveAt(_lstLog.Items.Count - 1);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Hide instead of disposing so it can be re-opened with F1
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
        else
        {
            base.OnFormClosing(e);
        }
    }
}
