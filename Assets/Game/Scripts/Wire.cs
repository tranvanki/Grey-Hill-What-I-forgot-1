using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    [Header("Wire Type Prefabs")]
    [SerializeField] public GameObject[] wirePrefabs;

    [HideInInspector] public int wireType; 
    [HideInInspector] public bool isFilled; 
    [HideInInspector] public bool isLocked = false; 

    private List<Transform> connectBoxes = new List<Transform>();

    public void Init(int type)
    {
        wireType = type;
        
        // Destroy existing children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Validate type
        if (type == 0 || type >= wirePrefabs.Length || wirePrefabs[type] == null)
        {
            return;
        }

        // Instantiate visual
        GameObject wireVisual = Instantiate(wirePrefabs[type], transform);
        wireVisual.transform.localPosition = Vector3.zero;
        wireVisual.transform.localRotation = Quaternion.identity;
        wireVisual.transform.localScale = new Vector3(48f / 99f, 48f / 100f, 1f);
        // Populate connect boxes (Assuming child 0 is the sprite, and children 1+ are connection points)
        connectBoxes.Clear();
        for (int i = 1; i < wireVisual.transform.childCount; i++)
        {
            connectBoxes.Add(wireVisual.transform.GetChild(i));
        }
    }

    public void UpdateInput()
    {
        if (isLocked) return;
        transform.Rotate(0, 0, -90f);
    }

    public List<Wire> GetConnectedWires()
    {
        List<Wire> result = new List<Wire>();
        HashSet<Wire> alreadyAdded = new HashSet<Wire>(); 

        foreach (Transform box in connectBoxes)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(box.position, 0.1f);
        foreach (Collider2D col in hits)
        {
            Wire otherWire = col.GetComponentInParent<Wire>();

            if (otherWire != null && otherWire != this && !alreadyAdded.Contains(otherWire))
            {
                result.Add(otherWire);
                alreadyAdded.Add(otherWire);
            }
        }
        }

        return result;
    }
}