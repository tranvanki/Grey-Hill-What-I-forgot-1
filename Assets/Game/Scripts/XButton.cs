using UnityEngine;

public class XButton : MonoBehaviour
{
    public enum PanelTarget { Inventory, Setting }
    [SerializeField] private PanelTarget panelType;

    // Assign the panel this button belongs to; if left empty, closes parent GameObject
    [SerializeField] private GameObject panelOverride;

    public void ClosePanel()
    {
        if(UIManager.Instance == null) return;
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.ClickX);
        if(panelType == PanelTarget.Inventory)
            UIManager.Instance.CloseInventory();
        else
            UIManager.Instance.CloseSettings();
    }
}
