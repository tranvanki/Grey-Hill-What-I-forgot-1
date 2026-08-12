using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject inventoryPanel;

    [Header("HUD Buttons")]
    public Button settingButton;
    public Button inventoryButton;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;// 1. only one instance of UIManager  exists
            GameObject root = transform.root.gameObject;
            DontDestroyOnLoad(root);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        if (settingButton != null) settingButton.onClick.AddListener(OpenSettings);
        if (inventoryButton != null) inventoryButton.onClick.AddListener(OpenInventory);
    }


    public void RegisterPanels(GameObject settings, GameObject inventory,
                                Button settingsBtn = null, Button inventoryBtn = null)
    {
        settingsPanel = settings;
        inventoryPanel = inventory;

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        if (settingsBtn != null)
        {
            settingButton = settingsBtn;
            settingButton.onClick.RemoveAllListeners();
            settingButton.onClick.AddListener(OpenSettings);
        }

        if (inventoryBtn != null)
        {
            inventoryButton = inventoryBtn;
            inventoryButton.onClick.RemoveAllListeners();
            inventoryButton.onClick.AddListener(OpenInventory);
        }
    }


    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (settingsPanel == null || inventoryPanel == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel.activeSelf) CloseSettings();
            else OpenSettings();
        }

        if (kb[Key.I].wasPressedThisFrame || kb.tabKey.wasPressedThisFrame)
        {    if (InventoryManager.Instance != null && !InventoryManager.Instance.IsInventoryEnabled())
            return; //
            if (inventoryPanel.activeSelf) CloseInventory();
            else OpenInventory();
        }
    }

    // ── Settings ─────────────────────────────────────────────────────────────
    public void OpenSettings()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.OpenPanel);
        settingsPanel.SetActive(true);
        inventoryPanel.SetActive(false);
        Time.timeScale = 0f; // 3. Stop when settings is open
    }

    public void CloseSettings()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.ClickX);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game when settings is closed
        
    }
    
    public void OnQuitButton()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.ClickButton);
        Time.timeScale = 1f;
        Destroy(transform.root.gameObject); 
        SceneManager.LoadScene("MainMenuScene");
    }

    // ── Inventory ─────────────────────────────────────────────────────────────
    public void OpenInventory()
    {   
        if (InventoryManager.Instance != null && !InventoryManager.Instance.IsInventoryEnabled())
        return;
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.OpenPanel);
        inventoryPanel.SetActive(true);
        settingsPanel.SetActive(false);
        Time.timeScale = 0f; // Stop game when inventory is open
       
    }

    public void CloseInventory()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.ClickX);
        inventoryPanel.SetActive(false);
        Time.timeScale = 1f; // Continue when close inventory
        
    }
    
}
