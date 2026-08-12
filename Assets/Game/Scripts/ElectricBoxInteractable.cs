using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))] // BẮT BUỘC PHẢI CÓ COLLIDER2D ĐỂ CLICK
public class ElectricBoxInteractable : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string puzzleSceneName = "ElectricPuzzle";

    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    private Transform player;

    [Header("Hint")]
    [SerializeField] private bool showInteractHint = true;
    [SerializeField] private string hintText = "Left click to interact";
    private bool _inRange = false;
    private GUIStyle _hintStyle;

    private void Update()
    {
        // Cache player transform 1 lần
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Cập nhật _inRange mỗi frame để OnGUI biết có nên hiện hint không
        _inRange = player != null &&
            Vector2.Distance(transform.position, player.position) <= interactRange;
    }

    private void OnMouseDown()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player == null)
        {
            Debug.LogWarning("[ElectricBox] Player not found!");
            return;
        }

        // Check distance
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > interactRange)
        {
            Debug.Log("[ElectricBox] You are too far from the electric box!");
            return;
        }

        // Check if electricity is out
        if (GameState.TryGet(out GameState state))
        {
            if (!state.ElectricityOut)
            {
                Debug.Log("[ElectricBox] The electrical system is working fine.");
                return;
            }

            // Require monster to have appeared (adds tension)
            if (!MonsterAI.HasMonsterAppeared)
            {
                Debug.Log("[ElectricBox] Something feels wrong... maybe wait a moment.");
                return;
            }

            state.SetPreviousScene(SceneManager.GetActiveScene().name);
        }

        // Check if player has toolbox
        if (InventoryManager.Instance == null || !InventoryManager.Instance.HasItem("tool_box"))
        {
            Debug.Log("[ElectricBox] Need a toolbox to open this!");
            return;
        }

        Debug.Log($"[ElectricBox] Loading puzzle scene: {puzzleSceneName}");
        SceneManager.LoadScene(puzzleSceneName);
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