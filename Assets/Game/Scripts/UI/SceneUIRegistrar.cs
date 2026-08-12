using UnityEngine;
using UnityEngine.UI;


public class SceneUIRegistrar : MonoBehaviour
{
    [Header("Panels ")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject inventoryPanel;

    [Header("Buttons ")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button inventoryButton;

    void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RegisterPanels(settingsPanel, inventoryPanel,
                                              settingsButton, inventoryButton);
        }
    }
}
