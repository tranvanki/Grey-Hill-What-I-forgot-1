using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "New NPC Dialogue", menuName = "NPC Dialogue")]

public class NPCDialogue : ScriptableObject
{
    public string npcName;
    [TextArea(3, 10)]
    public string[] dialogueLines;
    public float textSpeed = 0.05f; 

    public bool[] autoProgressLines;
    public float autoProgressDelay = 2f;

}
