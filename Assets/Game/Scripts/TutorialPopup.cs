using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialPopup : MonoBehaviour
{
    [Header("Tutorial Content")]
    [TextArea(3, 6)]
    [SerializeField] private string tutorialText =
        "F - Pick up item\nLeft Click - Open / Interact\nShift - Run";

    [Header("Popup Settings")]
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private string closeHintText = "Click anywhere or press any key to continue";

    private bool _isOpen = false;
    private GUIStyle _boxStyle;
    private GUIStyle _titleStyle;
    private GUIStyle _hintStyle;

    void Start()
    {
        if (showOnStart)
        {
            OpenTutorial();
        }
    }

    void Update()
    {
        if (!_isOpen) return;

        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        if (clicked || keyPressed)
        {
            CloseTutorial();
        }
    }

    public void OpenTutorial()
    {
        _isOpen = true;
        Time.timeScale = 0f;
    }

    private void CloseTutorial()
    {
        _isOpen = false;
        Time.timeScale = 1f;
    }

    private void OnGUI()
    {
        if (!_isOpen) return;

        if (_boxStyle == null)
        {
            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.fontSize = 24;
            _boxStyle.alignment = TextAnchor.MiddleCenter;
            _boxStyle.normal.textColor = Color.white;
            _boxStyle.wordWrap = true;

            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 34;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.normal.textColor = Color.white;

            _hintStyle = new GUIStyle(GUI.skin.label);
            _hintStyle.fontSize = 20;
            _hintStyle.fontStyle = FontStyle.Italic;
            _hintStyle.alignment = TextAnchor.MiddleCenter;
            _hintStyle.normal.textColor = Color.gray;
        }

        float w = 800f;
        float h = 420f;
        float x = (Screen.width - w) / 2f;
        float y = (Screen.height - h) / 2f;

        GUI.Box(new Rect(x, y, w, h), GUIContent.none, _boxStyle);
        GUI.Label(new Rect(x, y + 40f, w, 60f), "How to Play", _titleStyle);
        GUI.Label(new Rect(x + 40f, y + 110f, w - 80f, h - 190f), tutorialText, _boxStyle);
        GUI.Label(new Rect(x, y + h - 60f, w, 40f), closeHintText, _hintStyle);
    }
}