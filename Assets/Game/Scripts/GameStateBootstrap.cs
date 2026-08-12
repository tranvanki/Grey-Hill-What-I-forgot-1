using UnityEngine;

/// <summary>
/// Automatically creates GameState if it doesn't exist when entering any scene.
/// This ensures GameState is never null during gameplay.
/// </summary>
public class GameStateBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureGameState()
    {
        if (GameState.HasInstance)
        {
            return;
        }

        GameObject go = new GameObject("[GameState_AutoBootstrap]");
        go.AddComponent<GameState>();
        Debug.Log("[GameStateBootstrap] Auto-created GameState because it was missing.");
    }
}
