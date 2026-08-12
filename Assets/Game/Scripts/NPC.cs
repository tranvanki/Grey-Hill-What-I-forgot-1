using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NPC : MonoBehaviour
{
    [Header("Dialogue Stages")]
    public NPCDialogue[] dialogueStages;
    public int currentStage = 0;

    [Header("UI References ")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nametext;

    [Header("Interaction ")]
    public float interactRange = 2f;

    [Header("Events ")]
    public UnityEvent onDialogueComplete;

    [Header("Blip Sound ")]
    public AudioSource audioSource;
    public AudioClip defaultBlipSound;

    
    [HideInInspector] public NPCDialogue dialogueData;
    [HideInInspector] public NPCDialogue postQuestDialogue;

    private int dialogueindex = 0;
    private bool isTyping = false, isDialogueActive = false;
    private NPCDialogue activeDialogue;

    void Awake()
    {
        if (dialoguePanel == null)
            dialoguePanel = GameObject.Find("Dialogue/DialogueBox");

        if (dialoguePanel == null)
        {
            Debug.LogError("[NPC] DialogueBox not found! Make sure 'Dialogue/DialogueBox' exists and is ACTIVE at scene start.", this);
            return;
        }

        dialoguePanel.SetActive(false);
    }

    
    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    void Interact()
    {
        if (dialogueStages == null || dialogueStages.Length == 0) return;

        if (!isDialogueActive)
        {
            startDialogue();
        }
        else
        {
            NextLine();
        }
    }

    public void startDialogue()
    {
        if (dialoguePanel == null) return;

        currentStage = Mathf.Clamp(currentStage, 0, dialogueStages.Length - 1);
        activeDialogue = dialogueStages[currentStage];
        
        dialogueData = activeDialogue; 

        isDialogueActive = true;
        DialogueController.Instance?.RegisterNPC(this);
        dialoguePanel.SetActive(true);
        nametext.text = activeDialogue.npcName;
        dialogueindex = 0;
        
        StartCoroutine(TypeLine());
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);

        bool clickedMe = false;
        foreach (var h in hits)
            if (h.gameObject == gameObject) { clickedMe = true; break; }

        if (!clickedMe) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist > interactRange) return;

        Interact();
    }

    public void NextLine()
    {
        // Safety check
    if (activeDialogue == null || activeDialogue.dialogueLines == null)
    {
        Debug.LogError("[NPC] activeDialogue or dialogueLines is null!", this);
        EndDialogue();
        return;
    }

    if (dialogueindex >= activeDialogue.dialogueLines.Length)
    {
        Debug.LogError($"[NPC] dialogueindex {dialogueindex} out of bounds (Length: {activeDialogue.dialogueLines.Length})", this);
        EndDialogue();
        return;
    }

    if (isTyping)
    {
        StopAllCoroutines();
        dialogueText.text = activeDialogue.dialogueLines[dialogueindex];
        isTyping = false;
    }
    else if (++dialogueindex < activeDialogue.dialogueLines.Length)
    {
        StartCoroutine(TypeLine());
    }
    else
    {
        EndDialogue();
    }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char letter in activeDialogue.dialogueLines[dialogueindex])
        {
            dialogueText.text += letter;

            if (defaultBlipSound != null && audioSource != null && letter != ' ')
            {
                audioSource.PlayOneShot(defaultBlipSound);
            }

            yield return new WaitForSeconds(activeDialogue.textSpeed);
        }

        isTyping = false;

        if (activeDialogue.autoProgressLines != null && activeDialogue.autoProgressLines.Length > dialogueindex && activeDialogue.autoProgressLines[dialogueindex])
        {
            yield return new WaitForSeconds(activeDialogue.autoProgressDelay);
            NextLine();
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        DialogueController.Instance?.ClearNPC(this);
        dialoguePanel.SetActive(false);

        StartCoroutine(InvokeCompleteNextFrame());
    }

    IEnumerator InvokeCompleteNextFrame()
    {
        yield return null; 
        onDialogueComplete?.Invoke();
    }

    public void SetStage(int stageIndex)
    {
        currentStage = Mathf.Clamp(stageIndex, 0, dialogueStages.Length - 1);
    }
}