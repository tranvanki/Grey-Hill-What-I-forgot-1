using UnityEngine;

/// <summary>
/// Place this on an empty GameObject in each scene to mark the player's spawn position.
/// The Player prefab placed in the scene will be teleported here on Start.
/// </summary>
public class SceneSpawnPoint : MonoBehaviour
{
    [Tooltip("Tag of the player GameObject to reposition on scene load.")]
    public string playerTag = "Player";

    void Start()
    {
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning("[SpawnPoint] No GameObject with tag '" + playerTag + "' found.");
            return;
        }

        player.transform.position = transform.position;
        Debug.Log("[SpawnPoint] Player spawned at " + transform.position);
    }

    // Draw a visible gizmo in Scene view so you can see spawn points easily
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "sv_icon_dot3_pix16_gizmo", true);
    }
}
