using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class MemoryShard : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemData memoryShardData;
    
    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    private Transform player;

    [Header("UI Hint")]
    [SerializeField] private GameObject hintUI;

    [Header("Cutscene")]
    
[SerializeField] private string cutsceneSceneName = "CutsceneChap2";

    private void Start()
    {
        // Find the player
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Initialize visibility based on game state
        if (GameState.TryGet(out GameState state))
        {
            // Hide the shard initially if puzzle not complete or player already has it
            bool alreadyHasIt = InventoryManager.Instance != null && memoryShardData != null && 
                                InventoryManager.Instance.HasItem(memoryShardData.itemID);
            
            bool shouldBeVisible = state.ElevatorUnlocked && !alreadyHasIt;
            gameObject.SetActive(shouldBeVisible);
            
            Debug.Log($"[MemoryShard] Start - Visible: {shouldBeVisible} (ElevatorUnlocked: {state.ElevatorUnlocked}, AlreadyHas: {alreadyHasIt})", this);
        }
        else
        {
            // If no GameState, hide by default
            gameObject.SetActive(false);
            Debug.LogWarning("[MemoryShard] GameState not found, hiding by default.", this);
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Show memory shard ONLY after electric puzzle is solved (elevator unlocked)
        if (GameState.TryGet(out GameState state))
        {
            // Memory shard should be visible if:
            // 1. Electric puzzle is solved (ElevatorUnlocked = true)
            // 2. Player doesn't already have it
            bool alreadyHasIt = InventoryManager.Instance != null && memoryShardData != null && 
                                InventoryManager.Instance.HasItem(memoryShardData.itemID);
            
            bool shouldBeVisible = state.ElevatorUnlocked && !alreadyHasIt;
            
            if (shouldBeVisible && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Debug.Log("[MemoryShard] Shown: Electric Puzzle Complete! Memory shard appears.", this);
            }
            else if (!shouldBeVisible && gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                Debug.Log($"[MemoryShard] Hidden: ElevatorUnlocked={state.ElevatorUnlocked}, alreadyHas={alreadyHasIt}", this);
                return;
            }
        }

        // Check distance
        float distance = Vector2.Distance(transform.position, player.position);
        bool inRange = distance <= interactRange;
        // Show/hide hint UI based on distance
        if (hintUI != null)
        {
            if (inRange && !hintUI.activeSelf) hintUI.SetActive(true);
            else if (!inRange && hintUI.activeSelf) hintUI.SetActive(false);
        }
        // Pick up if in range and pressed F
        if (inRange && Input.GetKeyDown(KeyCode.F))
        {
            PickupShard();
        }
    }

    private void PickupShard()
    {
        Debug.Log("[MemoryShard] Picking up memory shard...", this);
        
        // Add to inventory
        if (InventoryManager.Instance != null && memoryShardData != null)
        {
            InventoryManager.Instance.AddItem(memoryShardData);
            
            // Also add to GameState to persist it
            if (GameState.HasInstance)
            {
                GameState.Instance.AddItem(memoryShardData.itemID);
            }
            
            Debug.Log("[MemoryShard] Added to inventory and saved to GameState.", this);
        }

        // Hide hint
        if (hintUI != null) hintUI.SetActive(false);

         if (!string.IsNullOrEmpty(cutsceneSceneName))
    {
        Debug.Log($"[MemoryShard] Loading Chapter 2 cutscene scene: {cutsceneSceneName}");
        SceneManager.LoadScene(cutsceneSceneName);
    }
    else
    {
        Debug.LogWarning("[MemoryShard] Cutscene scene name is not set!");
    }

    gameObject.SetActive(false);
    }
}
