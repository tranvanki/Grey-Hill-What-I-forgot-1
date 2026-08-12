using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class ItemPickup : MonoBehaviour
{
    [Header("Item")]
    public ItemData itemData;

    [Header("Persistence")]
    public string pickupID;

    [Header("Interaction")]
    public float pickupRange = 2.5f;
    public bool showInteractHint = true;
    public string hintTextPickup = "press F to pickup";
    private GUIStyle _hintStyle;
    [Header("Debug")]
    public bool verboseLogs = true;
    public bool bypassPersistence = false;

    [Header("Post-Pickup Actions")]
    public bool unlocksFlashlight = false;
    public UnityEngine.Events.UnityEvent onPickup;
    private Transform _player;
    private bool _inRange;
    private bool _isPickedUp          = false;
    private bool _awaitingDialogueClose = false;

    void Start()
    {   if (unlocksFlashlight && FlashlightController.Instance == null)
    {
        Debug.LogError($"[ItemPickup] {gameObject.name} unlocks flashlight but FlashlightController.Instance is NULL!", this);
    }
        if (!bypassPersistence && !string.IsNullOrEmpty(pickupID))
        {
            if (GameState.TryGet(out GameState state))
            {
                if (state.GetInventoryIDs().Contains(pickupID))
                {
                   
                    gameObject.SetActive(false);
                    return;
                }
                else
                {
                    if (verboseLogs)
                    {
                        Debug.Log($"[ItemPickup] {gameObject.name} (pickupID={pickupID}) not picked up. Showing normally.", this);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[ItemPickup] GameState not existed, skipping persistence check.", this);
            }
        }
        else if (bypassPersistence && verboseLogs)
        {
            Debug.Log($"[ItemPickup] {gameObject.name}: bypassPersistence=true, item will not be hidden even if picked up.", this);
        }

    }

    void Update()
    {
        if (_player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) _player = playerGO.transform;
        }
        
        //reset save data for testing
        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (GameState.TryGet(out GameState state))
            {
                state.DeleteSave();
                Debug.LogWarning("[DEV] Save Data deleted successfully! Start the game from the beginning.");
            }
        }


        if (_player == null || itemData == null || _isPickedUp) return;

        float dist = Vector2.Distance(_player.position, transform.position);
        _inRange = dist <= pickupRange;

       

        if (_inRange && Input.GetKeyDown(KeyCode.F))
        {
            if (verboseLogs) Debug.Log($"[ItemPickup] F key pressed, attempting pickup.", this);
            TryPickup();
            return;
        }

        // Physics2D click detection (backup for OnMouseDown)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Camera.main != null)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);

            foreach (Collider2D hit in hits)
            {
                if (hit != null && hit.gameObject == gameObject)
                {
                    if (verboseLogs) Debug.Log($"[ItemPickup] Physics2D click detected on {itemData?.itemName}!", this);
                    if (_inRange) TryPickup();
                    else Debug.Log($"[ItemPickup] Click detected but out of range ({dist:F2} > {pickupRange})", this);
                    return;
                }
            }
        }
    }
    void OnGUI()
{
    if (!showInteractHint || itemData == null || _isPickedUp || !_inRange) return;
    if (_awaitingDialogueClose) return; 

    if (_hintStyle == null)
    {
        _hintStyle = new GUIStyle(GUI.skin.box);
        _hintStyle.fontSize = 18;
        _hintStyle.fontStyle = FontStyle.Bold;
        _hintStyle.normal.textColor = Color.white;
        _hintStyle.alignment = TextAnchor.MiddleCenter;
        _hintStyle.wordWrap = false;
    }

    if (Camera.main == null) return;

    Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.6f);
    if (screenPos.z < 0f) return; 

    float w = 160f;
    float h = 36f;
    Rect rect = new Rect(screenPos.x - w / 2f, Screen.height - screenPos.y - h, w, h);

    GUI.Box(rect, hintTextPickup, _hintStyle);
}
    void OnMouseDown()
    {
        if (verboseLogs)
        {
            Debug.Log($"[ItemPickup] OnMouseDown: inRange={_inRange}, pickedUp={_isPickedUp}", this);
        }

        if (_inRange && !_isPickedUp && !_awaitingDialogueClose) TryPickup();
    }
    private void TryPickup()
    {
        if (verboseLogs)
        {
            Debug.Log($"[ItemPickup] TryPickup called for {itemData?.itemName}", this);
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[ItemPickup] InventoryManager not found!");
            return;
        }
        bool success = InventoryManager.Instance.AddItem(itemData);
        if (!success)
        {
            if (verboseLogs) Debug.LogWarning($"[ItemPickup] Failed to add {itemData.itemName} to inventory.", this);
            return;
        }
        if (verboseLogs)
        {
            Debug.Log($"[ItemPickup] Successfully picked up {itemData.itemName}!", this);
        }
        _isPickedUp = true;
        if (!string.IsNullOrEmpty(pickupID))
        {
            if (GameState.TryGet(out GameState state))
            {
                state.AddItem(pickupID);
                Debug.Log($"[ItemPickup] Saved {pickupID} to GameState for persistence.", this);
            }
        }
        if (unlocksFlashlight)
        {
            FlashlightController.Instance?.UnlockFlashlight();
        }
        onPickup?.Invoke();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Collider2D col = GetComponent<Collider2D>();
        if (sr != null) 
        {
            sr.enabled = false;
            if (verboseLogs) Debug.Log($"[ItemPickup] Disabled SpriteRenderer on {gameObject.name}", this);
        }
        if (col != null) 
        {
            col.enabled = false;
            if (verboseLogs) Debug.Log($"[ItemPickup] Disabled Collider2D on {gameObject.name}", this);
        }
      
        if (verboseLogs) Debug.Log($"[ItemPickup] Pickup complete. GameObject still active: {gameObject.activeInHierarchy}", this);
    }
    }


