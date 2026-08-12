using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class BedInteraction : MonoBehaviour
{
    [Header("Scene")]
    
    public string nightmareSceneName = "NightmareScene";

    [Header("Interaction")]
    public float interactRange = 2f;

    [Header("Sleep Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 2f;

    [Header("Sleep")]
    
    public GameObject notReadyHint;
    public float hintDuration = 2f;

    private bool isSleeping = false;
    [Header("Hint")]
    private GUIStyle _hintStyle;
    public bool showInteractHint = true;
    public string hintText = "Left click to sleep";
    private bool _inRange = false;
    void Start()
    {
        
        if(fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

void Update()
{
    if (isSleeping) return;

    // Update _inRange based on player position
    GameObject player = GameObject.FindWithTag("Player");
    _inRange = player != null &&
        Vector2.Distance(transform.position, player.transform.position) <= interactRange;

    if (UnityEngine.InputSystem.Mouse.current == null) return;
    if (!UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) return;

    Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(
        UnityEngine.InputSystem.Mouse.current.position.ReadValue());
    Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);

    bool clickedMe = false;
    foreach (var h in hits)
        if (h.gameObject == gameObject) { clickedMe = true; break; }

    if (!clickedMe || !_inRange) return;

    TrySleep();
}
void OnGUI()
{
    if (!showInteractHint || !_inRange || isSleeping) return;
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

    float w = 160f, h = 36f;
    Rect rect = new Rect(screenPos.x - w / 2f, Screen.height - screenPos.y - h, w, h);
    GUI.Box(rect, hintText, _hintStyle);
}

    void TrySleep()
    {
        if (GameState.Instance == null || !GameState.Instance.DoctorQuestComplete)
        {
            ShowNotReadyHint();
            return;
        }

        isSleeping = true;
        StartCoroutine(SleepSequence());
    }

    IEnumerator SleepSequence()
    {
        // Fade to black
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            fadeCanvasGroup.blocksRaycasts =  true;
            fadeCanvasGroup.gameObject.SetActive(true);
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }
        else
        {
            // No fade object — just wait a beat
            yield return new WaitForSeconds(fadeDuration);
        }

        SceneManager.LoadScene(nightmareSceneName);
    }

    void ShowNotReadyHint()
    {
        if (notReadyHint != null)
            StartCoroutine(ShowHintCoroutine());
        else
            Debug.Log("[BedInteraction] Doctor quest not complete yet.");
    }

    IEnumerator ShowHintCoroutine()
    {
        notReadyHint.SetActive(true);
        yield return new WaitForSeconds(hintDuration);
        notReadyHint.SetActive(false);
    }


    public void CompleteQuestAndEnableBed()
    {
        if (GameState.Instance != null)
            GameState.Instance.CompleteDoctorQuest();

        Debug.Log("[BedInteraction] Doctor quest complete — bed is now usable.");
    }
}
