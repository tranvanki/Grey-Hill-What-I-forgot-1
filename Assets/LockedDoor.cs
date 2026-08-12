using UnityEngine;
using UnityEngine.SceneManagement;

public class LockedDoor : MonoBehaviour
{
    [Header("Quest Required")]
    public bool requireMrGravesQuest = false;

    [Header("Scene")]
    public string nextScene = "NightmarePuzzle";

    [Header("Feedback")]
    public GameObject lockedHint;
    public float hintDuration = 2f;

    [Header("Interaction")]
    public float interactRange = 3f;
    private Transform player;
    private bool _inRange = false;

    [Header("Hint")]
    [SerializeField] private bool showInteractHint = true;
    [SerializeField] private string hintText = "Left click to interact";
    private GUIStyle _hintStyle;

    private float _hintTimer;

    void Start()
    {
        if (lockedHint != null) lockedHint.SetActive(false);
    }

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        _inRange = player != null &&
            Vector2.Distance(transform.position, player.position) <= interactRange;

        if (_hintTimer > 0f)
        {
            _hintTimer -= Time.deltaTime;
            if (_hintTimer <= 0f && lockedHint != null)
                lockedHint.SetActive(false);
        }

        if (UnityEngine.InputSystem.Mouse.current == null) return;
        if (!UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(
            UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
            {
                TryOpenDoor();
                break;
            }
        }
    }
    private void TryOpenDoor()
    {
        if (requireMrGravesQuest)
        {
            if (GameState.TryGet(out GameState state))
            {
                if (!state.MrGravesQuestComplete)
                {
                    ShowHint();
                    Debug.Log("[LockedDoor] Locked: Mr. Graves quest not completed.");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("[LockedDoor] GameState not found in scene!");
                return;
            }
        }

        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.ClickDoor);
        UnlockDoor();
    }

    private void ShowHint()
    {
        if (lockedHint != null)
        {
            lockedHint.SetActive(true);
            _hintTimer = hintDuration;
        }
    }

    public void UnlockDoor()
    {
        SceneManager.LoadScene(nextScene);
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