using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Light2D))]
public class LightingManager : MonoBehaviour
{   
    [SerializeField] private Light2D globalLight;
    [SerializeField] private float normalIntensity = 1f;
    [SerializeField] private float blackoutIntensity = 0.02f;
    
    [Header("Cutscene Override")]
    [SerializeField] private float cutsceneIntensity = 1f;
    [SerializeField] private bool debugDarkImages = true;
    
    private bool _isCutscene = false;
    
    void Start()
    {
        if (globalLight == null)
        {
            globalLight = GetComponent<Light2D>();
        }

        string sceneName = SceneManager.GetActiveScene().name;
        _isCutscene = sceneName.Contains("Cutscene") || sceneName.Contains("cutscene");
        
        if (_isCutscene)
        {
            // FORCE bright lighting in cutscene, ignore electricity state
            globalLight.intensity = cutsceneIntensity;
            globalLight.enabled = true;
            Debug.Log($"[LightingManager] Cutscene detected - FORCING light intensity to {cutsceneIntensity}");
            
            if (debugDarkImages)
            {
                Invoke(nameof(FindDarkImages), 0.5f); // Delay để UI load xong
            }
            return;
        }
        
        // Check the global state as soon as the scene loads (e.g., after player dies)
        if (GameState.TryGet(out GameState state))
        {
            if (state.ElectricityOut)
            {
                globalLight.intensity = blackoutIntensity;
                Debug.Log("[LightingManager] Electricity is OUT. Setting light to Blackout mode.");
            }
            else
            {
                globalLight.intensity = normalIntensity;
                Debug.Log("[LightingManager] Electricity is ON. Setting light to Normal mode.");
            }
        }
    }
    
    void LateUpdate()
    {
        // Force lighting mỗi frame trong cutscene (chạy sau Timeline để override)
        if (_isCutscene && globalLight != null)
        {
            // Force bật GameObject (Timeline có thể tắt qua Activation Track)
            if (!globalLight.gameObject.activeSelf)
            {
                globalLight.gameObject.SetActive(true);
                Debug.LogWarning("[LightingManager] Timeline tried to disable Global Light - FORCING it back ON");
            }
            
            // Force component enabled và intensity
            if (!globalLight.enabled || globalLight.intensity < cutsceneIntensity)
            {
                globalLight.enabled = true;
                globalLight.intensity = cutsceneIntensity;
            }
        }
    }
    
    private void FindDarkImages()
    {
        Debug.Log("<color=yellow>===== Scanning for Dark UI Images che màn hình =====</color>");
        
        Image[] allImages = FindObjectsOfType<Image>(true);
        int darkCount = 0;

        foreach (Image img in allImages)
        {
            if (!img.gameObject.activeInHierarchy) continue;

            Color c = img.color;
            bool isDark = (c.r < 0.1f && c.g < 0.1f && c.b < 0.1f && c.a > 0.5f);
            
            if (isDark)
            {
                darkCount++;
                string path = GetPath(img.gameObject);
                RectTransform rt = img.rectTransform;
                Debug.LogWarning($"<color=red>DARK IMAGE: {path}</color>\nColor={c}, Size={rt.rect.size}", img);
            }
        }

        CanvasGroup[] allCG = FindObjectsOfType<CanvasGroup>(true);
        foreach (CanvasGroup cg in allCG)
        {
            if (cg.gameObject.activeInHierarchy && cg.alpha > 0.8f)
            {
                Debug.LogWarning($"<color=orange>CanvasGroup: {GetPath(cg.gameObject)}, alpha={cg.alpha}</color>", cg);
            }
        }

        if (darkCount == 0)
            Debug.Log("<color=green>Không tìm thấy Image đen che màn hình</color>");
        else
            Debug.LogError($"<color=red>Tìm thấy {darkCount} Image đen có thể đang che cutscene!</color>");
    }
    
    private string GetPath(GameObject obj)
    {
        string path = obj.name;
        Transform t = obj.transform.parent;
        while (t != null) { path = t.name + "/" + path; t = t.parent; }
        return path;
    }
}
