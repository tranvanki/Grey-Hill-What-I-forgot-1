using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }
    [Header("Options UI")]
    public GameObject optionsPanel;
    public Button yesButton;
    public Button noButton;
    public Button nextButton;

    [Header("Audio")]
    public AudioSource audioSource;

    private NPC activeNPC;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(Next);
            nextButton.onClick.AddListener(Next);
        }
    }

    public void RegisterNPC(NPC npc)
    {
        activeNPC = npc;
    }

    public void ClearNPC(NPC npc)
    {
        if (activeNPC == npc)
        {
            activeNPC = null;
        }
    }

    public void Next()
    {
        if (activeNPC != null)
        {
            if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.DialogueBlip);
            activeNPC.NextLine();
        }
    }
}
