using UnityEngine;

public class HintUI : MonoBehaviour
{
    public enum HintType { Interact, PickUp }

    [Header("Hint Type")]
    [Tooltip("Interact = Left Click, PickUp = Press F")]
    public HintType hintType = HintType.PickUp;

    [Header("Interaction")]
    [Tooltip("Maximum distance to display the hint")]
    public float pickupRange = 2.5f;

    [Header("Offset")]
    [Tooltip("Nudge the hint above the object's head")]
    public Vector3 hintOffset = new Vector3(0, 0.6f, 0);

    private Transform _player;
    private static HintUI CurrentActiveHint;

    void Update()
    {
        
        if (_player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) _player = playerGO.transform;
        }

        
        if (_player == null || HintDisplayManager.Instance == null) return;

        // calculate distance to player and check if within pickup range
        float dist = Vector3.Distance(_player.position, transform.position);
        bool inRange = dist <= pickupRange;

        
        Debug.Log($"[{gameObject.name}] playerPos={_player.position}, myPos={transform.position}, dist={dist:F2}, inRange={inRange}, currentActive={CurrentActiveHint?.name}");

        
        if (inRange && CurrentActiveHint != this)
        {
            CurrentActiveHint = this;
            HintDisplayManager.Instance.Show(this);
        }
        else if (inRange && CurrentActiveHint == this)
        {
            HintDisplayManager.Instance.UpdatePosition(transform.position + hintOffset);
        }
        else if (!inRange && CurrentActiveHint == this)
        {
            CurrentActiveHint = null;
            HintDisplayManager.Instance.Hide();
        }
    }

    void OnDisable()
    {
        if (CurrentActiveHint == this)
        {
            CurrentActiveHint = null;
            if (HintDisplayManager.Instance != null) HintDisplayManager.Instance.Hide();
        }
    }
}
