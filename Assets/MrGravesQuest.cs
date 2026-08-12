using UnityEngine;

public class MrGravesQuest : MonoBehaviour
{
    public enum QuestState { Intro, NeedMedicine, MedicineCollected, Completed }
    public QuestState currentState = QuestState.Intro;
    [SerializeField] private string itemID = "medicine01";
    private NPC npcScript; private GameState state;
    void Start()
    {
        npcScript = GetComponent<NPC>();
        if (npcScript == null)
        {
            Debug.LogError("[MrGravesQuest] Missing NPC component!", this);
            return;
        }

        if (GameState.TryGet(out GameState gameState))
        {
            state = gameState;
        }
        else
        {
            Debug.LogError("[MrGravesQuest] GameState not found!", this);
        }

        // Check if quest already completed
        if (state != null && state.MrGravesQuestComplete)
        {
            currentState = QuestState.Completed;
            npcScript.SetStage(3);
            Debug.Log("[MrGravesQuest] Quest already completed - stage 3");
            return;
        }

        // Check if player already has medicine
        if (state != null && InventoryManager.Instance.HasItem(itemID))
        {
            currentState = QuestState.MedicineCollected;
            npcScript.SetStage(2);
            Debug.Log("[MrGravesQuest] Player has medicine - starting at stage 2");
        }
        else
        {
            npcScript.SetStage(0);
            Debug.Log("[MrGravesQuest] Starting at Intro stage 0");
        }
    }

    void Update()
    {
        if (state == null || npcScript == null) return;

        // Auto-advance to stage 2 when player gets medicine
        if (currentState == QuestState.NeedMedicine && InventoryManager.Instance.HasItem(itemID))
        {
            currentState = QuestState.MedicineCollected;
            npcScript.SetStage(2);
            Debug.Log("[MrGravesQuest] Medicine found! Advancing to stage 2");
        }
    }


    public void OnFinishTalking()
    {
        if (state == null)
        {
            Debug.LogError("[MrGravesQuest] GameState is null in OnFinishTalking!", this);
            return;
        }

        Debug.Log($"[MrGravesQuest] OnFinishTalking called. Current state: {currentState}, Current NPC stage: {npcScript.currentStage}");

        switch (currentState)
        {
            case QuestState.Intro:
                // First dialogue completed, ask player to find medicine
                currentState = QuestState.NeedMedicine;
                npcScript.SetStage(1);
                Debug.Log("[MrGravesQuest] Stage 0→1: Now asking for medicine");
                break;

            case QuestState.NeedMedicine:
                // Player talked again but doesn't have medicine yet
                Debug.Log("[MrGravesQuest] Still waiting for medicine. Stage stays at 1");
                break;

            case QuestState.MedicineCollected:
                // Player has medicine, complete quest and unlock room 63
                currentState = QuestState.Completed;
                InventoryManager.Instance.ConsumeItem(itemID);
                state.RemoveItem(itemID);
                state.CompleteMrGravesQuest();
                npcScript.SetStage(3);
                Debug.Log("[MrGravesQuest] Quest completed! Medicine consumed, Room 63 should unlock.");
                break;

            case QuestState.Completed:
                Debug.Log("[MrGravesQuest] Quest already completed, casual conversation.");
                break;
        }
    }
}