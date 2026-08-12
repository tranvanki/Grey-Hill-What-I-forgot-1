using UnityEngine;

public class BackToCorridor : MonoBehaviour
{
    [Header("Interaction")]
    public float interactRange = 3f;
    private Transform player;
    private bool _inRange = false;

    [Header("Hint")]
    [SerializeField] private bool showInteractHint = true;
    [SerializeField] private string hintText = "Left click to interact";
    private GUIStyle _hintStyle;

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        _inRange = player != null &&
            Vector2.Distance(transform.position, player.position) <= interactRange;
    }

    void OnMouseDown()
    {
        BackToCorridorScene();
    }

    public void BackToCorridorScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Corrider_Left");
    }

    private void OnGUI()
    {
        if (!showInteractHint || !_inRange) return;
        if (Camera.main == null) return;

        if (_hintStyle == null)
        {
            _hintStyle = new GUIStyle(GUI.skin.box);
            _hintStyle.fontSize = 18;
            _hintStyle.fontStyle = FontStyle.Bold;
            _hintStyle.normal.textColor = Color.white;
            _hintStyle.alignment = TextAnchor.MiddleCenter;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.6f);
        if (screenPos.z < 0f) return;

        float w = 200f, h = 36f;
        Rect rect = new Rect(screenPos.x - w / 2f, Screen.height - screenPos.y - h, w, h);
        GUI.Box(rect, hintText, _hintStyle);
    }
}