using UnityEngine;

[CreateAssetMenu(fileName = "WireData", menuName = "Scriptable Objects/WireData")]
public class WireData : ScriptableObject
{
    public string wireName;
    public Sprite icon;
    public Sprite burntIcon;          // sprite burnt
    public WireConnection connections; 
}
