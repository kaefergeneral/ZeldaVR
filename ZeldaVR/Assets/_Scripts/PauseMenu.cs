using UnityEngine;
using Immersio.Utility;


public class PauseMenu : Singleton<PauseMenu>
{
    const int NumOptions = 5;
    const int ButtonHeight = 40;


    public bool IsShowing { get; private set; }


    StereoscopicGUI _stereoscopicGUI;

    int _cursorIndex;
    bool _cursorCooldownActive = false;
    int _cursorCooldownDuration = 6;
    int _cursorCooldownCounter = 0;

    string[] _buttonNames = { "Resume", "Toggle Music", "View Controls", "Look Sensitivity", "Save & Quit" };


    void Awake()
    {
        Hide();
    }


    public void Show()
    {
        _cursorIndex = 0;
        IsShowing = true;
    }

    public void Hide()
    {
        IsShowing = false;
    }


    void Update()
    {
        if (IsShowing)
        {
            UpdateCursor();

            if (ZeldaInput.GetButtonUp(ZeldaInput.Button.Start))
            {
                SelectOption(_cursorIndex);
            }
        }
    }

    void UpdateCursor()
    {
        if (_cursorCooldownActive)
        {
            if (++_cursorCooldownCounter >= _cursorCooldownDuration) { _cursorCooldownActive = false; }
        }
        else
        {
            float vertAxis = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveVertical);
            int direction = 0;
            if (vertAxis != 0) { direction = vertAxis < 0 ? 1 : -1; }

            if (direction == 0)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) { direction = -1; }
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) { direction = 1; }
            }

            if (direction != 0)
            {
                MoveCursor(direction);
            }
        }
    }

    void MoveCursor(int direction)
    {
        SetCursorIndex(_cursorIndex + direction);
    }

    void SetCursorIndex(int index)
    {
        _cursorIndex = index;
        if (_cursorIndex < 0) { _cursorIndex = NumOptions - 1; }
        else if (_cursorIndex > NumOptions - 1) { _cursorIndex = 0; }

        SoundFx sfx = SoundFx.Instance;
        sfx.PlayOneShot(sfx.cursor);

        _cursorCooldownActive = true;
        _cursorCooldownCounter = 0;
    }


    void SelectOption(int index)
    {
        switch (index)
        {
            case 0: Resume(); break;
            case 1: ToggleMusic(); break;
            case 2: ViewControls(); break;
            case 3: EditLookSensitivity(); break;
            case 4: SaveAndQuit(); break;
            default: break;
        }
    }

    void Resume()
    {
        Pause.Instance.ForceHideMenu();
    }

    void ToggleMusic()
    {
        Music.Instance.ToggleEnabled();
    }

    void ViewControls()
    {
        // TODO
    }

    void EditLookSensitivity()
    {
        // TODO
    }

    void SaveAndQuit()
    {
        Pause.Instance.ForceHideMenu();
        Pause.Instance.ForceHideInventory();
        SaveManager.Instance.SaveGame();
        Locations.Instance.LoadTitleScreen();
    }

    
    void OnStereoscopicGUI(StereoscopicGUI stereoscopicGUI)
    {
        _stereoscopicGUI = stereoscopicGUI;

        if (IsShowing) { GUIShowMenu(); }
    }

    void GUIShowMenu()
    {
        int fontSize = 24;
        TextAnchor alignment = TextAnchor.UpperCenter;

        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
        int y = (int)(center.y - ButtonHeight * 0.5f);

        int yInc = ButtonHeight + 10;
        string text;
        
        int storedFontSize = GUI.skin.label.fontSize;
        TextAnchor storedAlignment = alignment;

        GUI.skin.label.fontSize = fontSize;
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        {
            for (int i = 0; i < NumOptions; i++)
            {
                bool highlight = (i == _cursorIndex);
                DrawButton(y, _buttonNames[i], highlight);
                y += yInc;
            }
        }
        GUI.skin.label.fontSize = storedFontSize;
        GUI.skin.label.alignment = storedAlignment;
    }

    void DrawButton(int y, string text, bool highlighted = false)
    {
        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
        int w = 0;
        int h = ButtonHeight;
        int x = (int)(center.x - w * 0.5f);
        
        Color color = highlighted ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1.0f);
        _stereoscopicGUI.GuiHelper.StereoLabel(x, y, w, h, ref text, color);
    }

}
