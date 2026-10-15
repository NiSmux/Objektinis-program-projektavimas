using Rogue.Client.Controllers;
using Rogue.Client.Facade;
using Rogue.Client.Rendering;
using Rogue.Client.UI;

namespace Rogue.Client;

/// <summary>
/// Client class #3 of the Facade:
/// Main game window and viewport. Interacts strictly through <see cref="IGameEngineFacade"/>,
/// completely decoupled from networking, command internals, entity collections, and rendering math.
/// </summary>
public partial class GameForm : Form
{
    private readonly IGameEngineFacade _gameFacade;
    private readonly PlayerInputController _inputController;
    private readonly System.Windows.Forms.Timer _gameTimer = new();
    private DebugConsoleForm? _debugConsole;

    public GameForm() : this(new GameEngineFacade())
    {
    }

    public GameForm(IGameEngineFacade gameFacade)
    {
        InitializeComponent();

        _gameFacade = gameFacade;
        _inputController = new PlayerInputController(_gameFacade);

        DoubleBuffered = true;
        KeyPreview = true;

        ClientSize = new Size(
            Renderer.GridSize * Renderer.TileSize,
            Renderer.GridSize * Renderer.TileSize + 44);

        KeyDown += OnKeyDown;
        KeyUp += (s, e) => _inputController.HandleKeyUp(e.KeyCode);

        _gameFacade.StateChanged += OnStateChanged;

        _gameTimer.Interval = 16; // ~60 FPS
        _gameTimer.Tick += GameLoop;
        _gameTimer.Start();

        _ = _gameFacade.StartAsync();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F1)
        {
            ToggleDebugConsole();
            return;
        }

        _inputController.HandleKeyDown(e.KeyCode);
    }

    private void ToggleDebugConsole()
    {
        if (_debugConsole == null || _debugConsole.IsDisposed)
        {
            _debugConsole = new DebugConsoleForm(_gameFacade);
            _debugConsole.Show(this);
        }
        else if (_debugConsole.Visible)
        {
            _debugConsole.Hide();
        }
        else
        {
            _debugConsole.Show(this);
        }
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        const float dt = 1f / 60f;

        _inputController.Tick(dt);
        _gameFacade.Update(dt);

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        _gameFacade.Render(e.Graphics);
    }

    private void OnStateChanged()
    {
        if (InvokeRequired)
        {
            BeginInvoke(OnStateChanged);
            return;
        }

        Text = $"Rogue Multiplayer - {_gameFacade.ConnectionStatus}";
        Invalidate();
    }
}