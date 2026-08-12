using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{   
    public string QuestName;
    public string Description;
    public List<QuestObjective> Objectives;

    [System.Serializable]
    public class QuestObjective
    {
        public string ObjectiveID;//match with item ID
        public ObjectiveType Type;
        public string Description;
        public int currentAmount;
        public int requiredAmount;
        public bool IsCompleted => currentAmount >= requiredAmount;


    }
    public enum ObjectiveType{Collectitem, DefeatEnemy,ReachLocation,TalkNPC,Custom}

public class QuestProgress
    {
        public Quest quest;
        public List<QuestObjective> objectives;
           public QuestProgress(Quest quest)
        {
            this.quest = quest;
            objectives = new List<QuestObjective>();
            foreach(var obj in quest.Objectives)
            {
                objectives.Add(new QuestObjective
                {
                    ObjectiveID = obj.ObjectiveID,
                    Type = obj.Type,
                    Description = obj.Description,
                    currentAmount = 0,
                    requiredAmount = obj.requiredAmount
                });
            }
        }
        
    }

}
