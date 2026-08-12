using UnityEngine;

/// <summary>
/// Manages Receptionist quest in Reception scene.
/// Stages: 0=Welcome, 1=Electric Quest (after blackout), 2=Completed
/// </summary>
public class ReceptionistQuest : MonoBehaviour
{
    public enum QuestState { Welcome, ElectricQuest, Completed }
    public QuestState currentState = QuestState.Welcome;

    private NPC npcScript;
    private GameState state;

    void Start()
    {
        npcScript = GetComponent<NPC>();
        if (npcScript == null)
        {
            Debug.LogError("[ReceptionistQuest] Missing NPC component!", this);
            return;
        }

        if (GameState.TryGet(out GameState gameState))
        {
            state = gameState;
        }
        else
        {
            Debug.LogError("[ReceptionistQuest] GameState not found!", this);
            return;
        }

        // Restore quest state based on GameState flags
        if (state.ReceptionistQuestComplete)
        {
            currentState = QuestState.Completed;
            npcScript.SetStage(2);
            Debug.Log("[ReceptionistQuest] Quest already completed - stage 2");
        }
        else if (state.ElectricityOut || state.ReceptionistWelcomeComplete)
        {
            // If blackout has occurred or welcome is done, show electric quest dialogue
            currentState = QuestState.ElectricQuest;
            npcScript.SetStage(1);
            Debug.Log("[ReceptionistQuest] Blackout active - showing electric quest dialogue (stage 1)");
        }
        else
        {
            npcScript.SetStage(0);
            currentState = QuestState.Welcome;
            Debug.Log("[ReceptionistQuest] Starting at Welcome stage");
        }
    }

    void Update()
    {
        // Auto-switch to electric quest dialogue when blackout occurs
        if (state != null && currentState == QuestState.Welcome && state.ElectricityOut)
        {
            currentState = QuestState.ElectricQuest;
            npcScript.SetStage(1);
            Debug.Log("[ReceptionistQuest] Blackout detected! Switching to electric quest dialogue.");
        }
    }

    /// <summary>
    /// Called from Unity Event: NPC Inspector → onDialogueComplete
    /// </summary>
    public void OnFinishTalking()
    {
        if (state == null)
        {
            Debug.LogError("[ReceptionistQuest] GameState is null in OnFinishTalking!", this);
            return;
        }

        Debug.Log($"[ReceptionistQuest] OnFinishTalking called. Current state: {currentState}");

        switch (currentState)
        {
            case QuestState.Welcome:
                // Player finished welcome dialogue, mark it complete
                state.CompleteReceptionistWelcome();
                Debug.Log("[ReceptionistQuest] Welcome complete! Player can now click elevator.");
                break;

            case QuestState.ElectricQuest:
                // Player talked after solving the electric puzzle
                if (!state.ElectricityOut)
                {
                    currentState = QuestState.Completed;
                    npcScript.SetStage(2);
                    state.CompleteReceptionistQuest();
                    Debug.Log("[ReceptionistQuest] Electric quest complete! Power restored.");
                }
                else
                {
                    Debug.Log("[ReceptionistQuest] Still need to fix the electricity first.");
                }
                break;

            case QuestState.Completed:
                Debug.Log("[ReceptionistQuest] Quest already completed, casual conversation.");
                break;
        }
    }
}
