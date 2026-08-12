using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] public Image iconImage;       // Image child that shows the item icon
    [SerializeField] public Image highlightImage;  // Border overlay shown when slot is selected

    [HideInInspector] public int slotIndex;        // Set automatically by InventoryManager

    private ItemData _item;                        // Item currently stored in this slot (null = empty)

    // ─────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────

    public bool HasItem => _item != null;
    public ItemData Item => _item;

    /// <summary>Place an item in this slot and show its icon.</summary>
    public void SetItem(ItemData data)
    {
        _item = data;
        if (iconImage != null)
        {
            iconImage.sprite = data != null ? data.icon : null;
            // Hiện icon khi có item, trong suốt khi trống (không tắt Image để slot nền vẫn hiện)
            Color c = iconImage.color;
            c.a = data != null ? 1f : 0f;
            iconImage.color = c;
        }
    }

    /// <summary>Remove the item from this slot and clear the icon.</summary>
    public void ClearSlot()
    {
        _item = null;
        if (iconImage != null)
        {
            iconImage.sprite = null;
            Color c = iconImage.color;
            c.a = 0f;
            iconImage.color = c;
        }
        SetHighlight(false);
    }

    /// <summary>Toggle the selection highlight border on/off.</summary>
    public void SetHighlight(bool active)
    {
        if (highlightImage != null)
            highlightImage.enabled = active;
    }

    // ─────────────────────────────────────────────────────────────
    //  Pointer events (required by the interfaces)
    // ─────────────────────────────────────────────────────────────

    // Called by Unity when player clicks this slot
    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance?.OnSlotClicked(slotIndex);
    }

    // Called by Unity when mouse enters this slot
    public void OnPointerEnter(PointerEventData eventData) { }

    // Called by Unity when mouse leaves this slot
    public void OnPointerExit(PointerEventData eventData) { }
}