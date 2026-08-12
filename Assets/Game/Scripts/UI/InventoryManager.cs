using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [Header("Item Database")]
    public List<ItemData> itemDatabase;
    public static InventoryManager Instance;

    [Header("Inventory Grid")]
    public Transform inventoryGrid;

    [Header("Scene Restriction")]
    [SerializeField] private string[] disabledScenes = { "MainMenuScene" };

    private InventorySlotUI[] _slots;
    private int _selectedSlot = -1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        CollectSlots();
        var saveIDs = GameState.Instance?.GetInventoryIDs();
        if (saveIDs != null && saveIDs.Count > 0)
            RestoreItems(saveIDs);
    }

    public bool IsInventoryEnabled()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        foreach (var scene in disabledScenes)
            if (currentScene == scene) return false;
        return true;
    }

    private void CollectSlots()
    {
        if (inventoryGrid == null)
        {
            Debug.LogWarning("[InventoryManager] inventoryGrid is not assigned!");
            return;
        }

        _slots = inventoryGrid.GetComponentsInChildren<InventorySlotUI>(includeInactive: true);
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].slotIndex = i;
            _slots[i].ClearSlot();
        }
    }

    public bool AddItem(ItemData data)
    {
        if (_slots == null) CollectSlots();

        foreach (var slot in _slots)
        {
            if (!slot.HasItem)
            {
                slot.SetItem(data);
                SFXManager.Instance?.PlaySFX(SFXManager.SFXType.PickupItem);
                return true;
            }
        }
        Debug.Log("[Inventory] Inventory is full!");
        return false;
    }

    private ItemData RemoveItem(int slotIndex)
    {
        if (!IsValidSlot(slotIndex) || !_slots[slotIndex].HasItem)
            return null;

        ItemData item = _slots[slotIndex].Item;
        _slots[slotIndex].ClearSlot();
        return item;
    }

    public bool HasItem(string itemID)
    {
        if (_slots == null) return false;
        foreach (var slot in _slots)
            if (slot.HasItem && slot.Item.itemID == itemID) return true;
        return false;
    }

    public ItemData ConsumeItem(string itemID)
    {
        if (_slots == null) return null;
        for (int i = 0; i < _slots.Length; i++)
            if (_slots[i].HasItem && _slots[i].Item.itemID == itemID)
                return RemoveItem(i);
        return null;
    }

    public ItemData GetSelectedItem()
    {
        return IsValidSlot(_selectedSlot) ? _slots[_selectedSlot].Item : null;
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return;

        if (!_slots[slotIndex].HasItem) { Deselect(); return; }

        if (_selectedSlot == slotIndex) Deselect();
        else Select(slotIndex);
    }

    private void Select(int slotIndex)
    {
        Deselect();
        _selectedSlot = slotIndex;
        _slots[slotIndex].SetHighlight(true);
    }

    private void Deselect()
    {
        if (IsValidSlot(_selectedSlot))
            _slots[_selectedSlot].SetHighlight(false);
        _selectedSlot = -1;
    }

    private bool IsValidSlot(int index) =>
        _slots != null && index >= 0 && index < _slots.Length;

    public List<string> GetItemIDs()
    {
        var ids = new List<string>();
        if (_slots == null) return ids;
        foreach (var slot in _slots)
            if (slot.HasItem) ids.Add(slot.Item.itemID);
        return ids;
    }

    public void RestoreItems(List<string> ids)
    {
        if (ids == null) return;
        foreach (var id in ids)
        {
            var item = itemDatabase?.Find(d => d.itemID == id);
            if (item != null) AddItem(item);
            else Debug.LogWarning($"[InventoryManager] RestoreItems: No item with ID '{id}' found in database!");
        }
    }
}