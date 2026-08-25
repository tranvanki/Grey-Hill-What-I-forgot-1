using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Elevator : MonoBehaviour, IPointerDownHandler
{
    private Light2D globalLightComponent;

    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    private Transform player;
    private bool _inRange = false;

    [Header("Hint")]
    [SerializeField] private bool showInteractHint = true;
    [SerializeField] private string hintText = "Left click to interact";
    private GUIStyle _hintStyle;

    void Start()
    {
        GameObject globalLight = GameObject.Find("Global Light 2D");
        if (globalLight != null)
        {
            globalLightComponent = globalLight.GetComponent<Light2D>();
        }

        Debug.Log("[Elevator] Start() called. Ready to receive clicks.", this);
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

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (Camera.main == null)
        {
            return;
        }

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.gameObject == gameObject)
            {
                Debug.Log("[Elevator] Physics click detected!", this);
                HandleClick();
                return;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData != null && eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("[Elevator] UI Pointer click detected!", this);
            HandleClick();
        }
    }

    private void HandleClick()
    {
        if (!GameState.TryGet(out GameState state))
        {
            Debug.LogWarning("[Elevator] Click ignored because GameState is missing.", this);
            return;
        }

        Debug.Log($"[Elevator] HandleClick: WelcomeComplete={state.ReceptionistWelcomeComplete}, ElectricityOut={state.ElectricityOut}", this);

        if (!state.ReceptionistWelcomeComplete)
        {
            Debug.Log("[Elevator] You need to talk to the receptionist first (Welcome not complete).", this);
            return;
        }

        if (!state.ElectricityOut)
        {
            Debug.Log("[Elevator] Triggering blackout! Power failure!", this);
            state.TriggerBlackout();

            if (globalLightComponent != null)
            {
                globalLightComponent.intensity = 0.02f;
            }

            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlaySFX(SFXManager.SFXType.PowerDown);
            }
        }
        else if(state.ElevatorUnlocked)
        {
            Debug.Log("[Elevator] Elevator is unlocked! Loading next scene...", this);
            SceneManager.LoadScene("Corrider_Left"); // Replace with your actual scene name
        }
        
        else
        {
            Debug.Log("[Elevator] Power is out! Fix the electrical system first.", this);
        }
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