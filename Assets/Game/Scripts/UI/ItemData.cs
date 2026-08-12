using UnityEngine;

/// <summary>
/// ScriptableObject that defines a single item type.
/// Create: right-click in Project → Create → Inventory / Item Data
/// </summary>
[CreateAssetMenu(menuName = "Inventory/Item Data", fileName = "New ItemData")]
public class ItemData : ScriptableObject
{
    public enum ItemType { Key, Document, Tool, Consumable, QuestItem }

    [Header("Informations")]
    public string itemID;           // Used to match against QuestObjective.ObjectiveID
    public string itemName;
    [TextArea(2, 4)]
    public string description;

    [Header("Visuals")]
    public Sprite icon;

    [Header("Type")]
    public ItemType itemType;

    [Header("Key Settings")]        // Only used when itemType == Key
    public string unlocksID;        // ID of the door this key unlocks
}
