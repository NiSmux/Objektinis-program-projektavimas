namespace Rogue.Client.Input;

public class InputManager
{
    public bool Up { get; private set; }
    public bool Down { get; private set; }
    public bool Left { get; private set; }
    public bool Right { get; private set; }

    public void KeyDown(Keys key)
    {
        switch (key)
        {
            case Keys.W: Up = true; break;
            case Keys.S: Down = true; break;
            case Keys.A: Left = true; break;
            case Keys.D: Right = true; break;
        }
    }

    public void KeyUp(Keys key)
    {
        switch (key)
        {
            case Keys.W: Up = false; break;
            case Keys.S: Down = false; break;
            case Keys.A: Left = false; break;
            case Keys.D: Right = false; break;
        }
    }
}