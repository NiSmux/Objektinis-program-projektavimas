using Rogue.Client.Facade;

namespace Rogue.Client.Controllers;

/// <summary>
/// Client class #1 of the Facade:
/// Intercepts raw keyboard inputs and coordinates high-level actions through <see cref="IGameEngineFacade"/>.
/// </summary>
public class PlayerInputController
{
    private readonly IGameEngineFacade _facade;

    public bool Up { get; private set; }
    public bool Down { get; private set; }
    public bool Left { get; private set; }
    public bool Right { get; private set; }

    public PlayerInputController(IGameEngineFacade facade)
    {
        _facade = facade;
    }

    public void HandleKeyDown(Keys key)
    {
        switch (key)
        {
            case Keys.W: Up = true; break;
            case Keys.S: Down = true; break;
            case Keys.A: Left = true; break;
            case Keys.D: Right = true; break;

            case Keys.Space:
                _facade.Dash();
                break;

            case Keys.Q:
                _facade.UsePotion();
                break;

            case Keys.Z:
                _facade.Recall();
                break;
        }
    }

    public void HandleKeyUp(Keys key)
    {
        switch (key)
        {
            case Keys.W: Up = false; break;
            case Keys.S: Down = false; break;
            case Keys.A: Left = false; break;
            case Keys.D: Right = false; break;
        }
    }

    /// <summary>
    /// Processes continuous directional movement once per game tick.
    /// </summary>
    public void Tick(float dt)
    {
        float dx = (Right ? 1f : 0f) - (Left ? 1f : 0f);
        float dy = (Down ? 1f : 0f) - (Up ? 1f : 0f);

        _facade.MoveLocalPlayer(dx, dy, dt);
    }
}
