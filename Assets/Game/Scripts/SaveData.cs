using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class PieceData
{    public string pieceName;
    public Vector3 position;
}
[System.Serializable]
public class PuzzleSaveData
{
    public bool isCompleted;
    public int piecesCorrect;
    public List <PieceData> pieces = new List<PieceData>();
}
[System.Serializable]
public class InventorySaveData
{
    public List<string> itemIDs = new List<string>();
}
[System.Serializable]
public class QuestSaveData
{
    public bool doctorQuestComplete = false;
    public bool receptionistQuestComplete = false;
    public bool receptionistWelcomeComplete = false; 
    public bool mrGravesQuestComplete = false;
}

[System.Serializable]
public class PlayerStatsSaveData
{
    public int hp = 3;
    
}
[System.Serializable]
public class SaveData
{
    public string saveDateTime = "";
    public string lastCheckpointScene = "";
    public string previousScene = "";
    public bool hasPuzzleSave = false;
    public bool electricityOut = false;
    public bool elevatorUnlocked = false;

    public InventorySaveData inventory = new InventorySaveData();
    public QuestSaveData quests = new QuestSaveData();
    public PlayerStatsSaveData playerStats = new PlayerStatsSaveData();
    public PuzzleSaveData puzzle = new PuzzleSaveData();
}