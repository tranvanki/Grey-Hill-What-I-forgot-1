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
    private Transform player;
    public GameObject lockedHint;
    public float hintDuration = 2f;
    public float interactRange = 2f;
    private bool _inRange = false;
    [SerializeField] private bool showInteractHint = true;
    [SerializeField] private string hintText = "Left click to interact";
    private GUIStyle _hintStyle;
    private float _hintTimer;

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
    void Start()
    {
        if (lockedHint != null) lockedHint.SetActive(false);

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
        // 1. Find the player if we haven't already
        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) player = playerGO.transform;
        }

        // 2. Update the _inRange variable every frame
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            _inRange = (dist <= interactRange);
        }

        // 3. Handle the Locked Hint timer to hide the hint after a few seconds
        if (_hintTimer > 0)
        {
            _hintTimer -= Time.deltaTime;
            if (_hintTimer <= 0 && lockedHint != null)
            {
                lockedHint.SetActive(false);
            }
        }

        // 4. Handle Mouse Clicks
        if (UnityEngine.InputSystem.Mouse.current == null) return;
        if (!UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(
            UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorld);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
            {
                if (_inRange)
                {
                    Interact();
                }
                else
                {
                    Debug.Log($"[NPC] Player clicked on NPC '{gameObject.name}' but is out of range.");
                }
                break;
            }
        }
    }
//GUI function
private void OnGUI()
    {
        if (!showInteractHint || !_inRange) return;
        if (Camera.main == null) return;

        if (_hintStyle == null)
        {
            _hintStyle = new GUIStyle(GUI.skin.box);
            _hintStyle.fontSize = 18;
            _hintStyle.fontStyle = FontStyle.Bold;
            _hintStyle.normal.textColor = Color.white;
            _hintStyle.alignment = TextAnchor.MiddleCenter;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.6f);
        if (screenPos.z < 0f) return;

        float w = 200f, h = 36f;
        Rect rect = new Rect(screenPos.x - w / 2f, Screen.height - screenPos.y - h, w, h);
        GUI.Box(rect, hintText, _hintStyle);
    }
    public void NextLine()
    {   
         Debug.Log($"[NPC] NextLine called. isTyping = {isTyping}");
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