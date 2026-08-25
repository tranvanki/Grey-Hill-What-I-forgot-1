using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }
    public static bool HasInstance => Instance != null;
    public string PreviousScene { get; private set; }
    public string CurrentScene => SceneManager.GetActiveScene().name;

    private SaveData _data; 
    private static bool _loggedMissingWarning;

 
    public bool DoctorQuestComplete => _data.quests.doctorQuestComplete;
    public bool ReceptionistQuestComplete => _data.quests.receptionistQuestComplete;
    public bool ReceptionistWelcomeComplete => _data.quests.receptionistWelcomeComplete;   // ➕ mới

    public bool MrGravesQuestComplete => _data.quests.mrGravesQuestComplete;
    public string LastCheckpointScene => _data.lastCheckpointScene;
    public int PlayerHP => _data.playerStats.hp;
    public bool ElectricityOut => _data.electricityOut;
    public bool ElevatorUnlocked => _data.elevatorUnlocked;

    public static bool TryGet(out GameState gameState)
    {
        gameState = Instance;

        if (gameState != null)
        {
            return true;
        }

        if (!_loggedMissingWarning)
        {
            _loggedMissingWarning = true;
            string sceneName = SceneManager.GetActiveScene().name;
            Debug.LogError($"[GameState] Instance is NULL in scene '{sceneName}'. Add a GameState bootstrap object before gameplay scene loads.");
        }

        return false;
    }

    // ────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _data = SaveManager.Load();
            PreviousScene = _data.previousScene;
            Debug.Log($"[GameState] Loaded. Checkpoint: {_data.lastCheckpointScene}");
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Temporarily disable electricity in cutscene scenes (don't save)
        if (scene.name.Contains("Cutscene") || scene.name.Contains("cutscene"))
        {
            _data.electricityOut = false;
            Debug.Log($"[GameState] Cutscene detected - temporarily disabling electricity state");
            return;
        }
                if (scene.name != "MainMenuScene" && scene.name != "LoadingScene")
        {
            SetCheckpoint(scene.name);
            Debug.Log($"[GameState] Auto-saved current scene as checkpoint: {scene.name}");
        }
    }

    // ── Save ─────────────────────────────────────────────────────
    public void SaveGame()
    {
        _data.previousScene = PreviousScene;
        SaveManager.SaveToJSON(_data);
    }

    // ── Scene ────────────────────────────────────────────────────
    public void SetPreviousScene(string sceneName)
    {
        PreviousScene = sceneName;
        _data.previousScene = sceneName;
        // Không SaveGame() — chỉ save ở checkpoint
    }

    public void SetCheckpoint(string sceneName)
    {
        _data.lastCheckpointScene = sceneName;
        SaveGame(); 
    }

    public void RespawnFromCheckpoint() =>
        SceneManager.LoadScene(_data.lastCheckpointScene);

    // ── Quest ────────────────────────────────────────────────────
    public void CompleteDoctorQuest()
    {
        _data.quests.doctorQuestComplete = true; 
        SaveGame();
    }
    public void CompleteReceptionistWelcome()
{
    _data.quests.receptionistWelcomeComplete = true;
    SaveGame();
}

    public void CompleteReceptionistQuest()
    {
        _data.quests.receptionistQuestComplete = true; 
        SaveGame();
    }
    
    public void CompleteMrGravesQuest()
{
    _data.quests.mrGravesQuestComplete = true;
    SaveGame();
}
    public void TriggerBlackout()
    {
        _data.electricityOut = true;
        _data.elevatorUnlocked = false;
        SaveGame(); 
    }

    public void RestorePowerAndUnlockElevator()
    {
        _data.electricityOut = false;
        _data.elevatorUnlocked = true;
        SaveGame(); 
    }

    public void ResetElectricPuzzleFlow()
    {
        _data.electricityOut = false;
        _data.elevatorUnlocked = false;
        SaveGame(); 
    }

    // ── Player Stats ─────────────────────────────────────────────
    public void SetHP(int value) => _data.playerStats.hp = value;

    // ── Inventory ────────────────────────────────────────────────
    public System.Collections.Generic.List<string> GetInventoryIDs() =>
        _data.inventory.itemIDs;

    public void AddItem(string itemID)
    {
        if (!_data.inventory.itemIDs.Contains(itemID))
        {
            _data.inventory.itemIDs.Add(itemID);
            SaveGame();
        }
    }

    public void RemoveItem(string itemID)
    {
        _data.inventory.itemIDs.Remove(itemID);
        SaveGame();
    }

    // ── Puzzle ───────────────────────────────────────────────────
    public void SavePuzzleToJSON(PuzzleSaveData puzzleData)
    {
        _data.puzzle = puzzleData;
        _data.hasPuzzleSave = true;
        SaveGame();
    }

    public PuzzleSaveData LoadPuzzleFromJSON() =>
        _data.hasPuzzleSave ? _data.puzzle : null;

    public void DeletePuzzleSave()
    {
        _data.puzzle = new PuzzleSaveData();
        _data.hasPuzzleSave = false;
        SaveGame();
    }

    // ── Delete All ───────────────────────────────────────────────
    public void DeleteSave()
    {
        SaveManager.Delete();
        _data = new SaveData();
        PreviousScene = "";
        Debug.Log("[GameState] Save deleted.");
    }
}