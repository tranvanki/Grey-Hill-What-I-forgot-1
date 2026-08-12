using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Cinemachine; 

public class PlayerManager : MonoBehaviour
{   
    public static PlayerManager Instance { get; private set; }
    [Header("Settings")]
    public bool autoSetupCinemachine = true;
     private bool isInitialized = false;
     void Awake()
    {
         if (Instance != null && Instance != this)
        {
            Debug.Log("[PlayerManager] Duplicate player found - destroying duplicate");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        isInitialized = true;
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("[PlayerManager] Player set to persist across scenes");
    }
      void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    /// <summary>
    /// Được gọi mỗi khi scene mới được load
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isInitialized) return;
        Debug.Log($"[PlayerManager] Scene loaded: {scene.name}");
        
        // ➕ DESTROY PLAYER WHEN RETURNING TO MAIN MENU
        if (scene.name == "MainMenuScene")
        {
            Debug.Log("[PlayerManager] Returned to Main Menu - destroying player");
            Destroy(gameObject);
            return;
        }
        
        // ➕ HIDE PLAYER IN CUTSCENE SCENES (chỉ nếu player này đang persist)
        if (scene.name.Contains("Cutscene") || scene.name.Contains("cutscene"))
        {
            // Chỉ ẩn nếu GameObject này đang trong DontDestroyOnLoad (player persist)
            // AdultMia trong cutscene scene sẽ KHÔNG ở DontDestroyOnLoad nên không bị ẩn
            if (gameObject.scene.name == "DontDestroyOnLoad")
            {
                Debug.Log($"[PlayerManager] Cutscene detected ({scene.name}) - hiding PERSIST player");
                gameObject.SetActive(false);
                return;
            }
            else
            {
                Debug.Log($"[PlayerManager] This is a cutscene-local player (AdultMia), NOT hiding");
                return;
            }
        }
        
        // Re-enable player if it was hidden from cutscene
        if (!gameObject.activeSelf)
        {
            Debug.Log("[PlayerManager] Re-enabling player after cutscene");
            gameObject.SetActive(true);
        }
        
        // 1. Tìm và teleport đến spawn point
        SpawnAtSpawnPoint();
        // 2. Setup Cinemachine nếu cần
        if (autoSetupCinemachine)
        {
            SetupCinemachine();
        }
    }
   
    private void SpawnAtSpawnPoint()
    {
        SceneSpawnPoint spawnPoint = FindObjectOfType<SceneSpawnPoint>();

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
            Debug.Log($"[PlayerManager] Spawned at {spawnPoint.transform.position}");
        }
        else
        {
            Debug.LogWarning($"[PlayerManager] No SceneSpawnPoint found in {SceneManager.GetActiveScene().name}");
        }
    }
      private void SetupCinemachine()
    {
        // Tìm tất cả Cinemachine Virtual Camera trong scene
        var virtualCameras = FindObjectsOfType<CinemachineCamera>();
        if (virtualCameras.Length == 0)
        {
            Debug.Log("[PlayerManager] No Cinemachine Virtual Camera found in scene");
            return;
        }
        foreach (var cam in virtualCameras)
        {
            // Set player là Follow target
            cam.Follow = transform;
            Debug.Log($"[PlayerManager] Setup Cinemachine camera: {cam.name}");
        }
    }
   
    public Vector3 GetPosition()
    {
        return transform.position;
    }
   
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        Debug.Log($"[PlayerManager] Player position set to {position}");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
