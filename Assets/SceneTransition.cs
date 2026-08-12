using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
     [Header("Scene Name")]
    public string nextScene = "";

    [Header("Timing")]
    public float transitionDelay = 1f;

    private bool isLoading = false;
    private bool playerNearby = false;

    //==========================================================
    // Transition to ANY scene
    //==========================================================
    public void TransitionToScene(string sceneName)
    {
        if (isLoading || string.IsNullOrWhiteSpace(sceneName))
            return;

        // ★ CORRECT: Save the current scene BEFORE transitioning away
        if (GameState.Instance != null)
        {
            string current = SceneManager.GetActiveScene().name;
            GameState.Instance.SetPreviousScene(current);
            Debug.Log($"[SceneTransition] Set PreviousScene = '{current}' → moving to '{sceneName}'");
        }

        StartCoroutine(LoadAfterDelay(sceneName, transitionDelay));
    }

    //==========================================================
    // Used for doors / interactions
    //==========================================================
    public void SceneChange()
    {
        TransitionToScene(nextScene);
    }

    public void SceneChangeAfterDelay(float customDelay)
    {
        if (isLoading || string.IsNullOrWhiteSpace(nextScene))
            return;

        if (GameState.Instance != null)
        {
            string current = SceneManager.GetActiveScene().name;
            GameState.Instance.SetPreviousScene(current);
            Debug.Log($"[SceneTransition] Set PreviousScene = '{current}' → moving to '{nextScene}'");
        }

        StartCoroutine(LoadAfterDelay(nextScene, customDelay));
    }

    //==========================================================
    // Return to the previous scene
    //==========================================================
    public void GoBack()
    {
        Debug.Log("[SceneTransition] GoBack() was called!");

        if (isLoading)
        {
            Debug.LogWarning("[SceneTransition] Currently isLoading = true → returning");
            return;
        }

        if (GameState.Instance == null)
        {
            Debug.LogWarning("[SceneTransition] GameState.Instance == null → cannot GoBack");
            return;   // ★ FIX: Just return, DO NOT call Instance while it is null
        }

        string target = GameState.Instance.PreviousScene;
        Debug.Log($"[SceneTransition] PreviousScene = '{target}'");

        if (string.IsNullOrWhiteSpace(target))
        {
            Debug.LogWarning("[SceneTransition] PreviousScene is empty → skipping scene transition");
            return;
        }

        // ★ IMPORTANT: Do not use SetPreviousScene here!
        // GoBack only READS the value to know where to return.
        StartCoroutine(LoadAfterDelay(target, transitionDelay));
    }

    //==========================================================
    // Scene loading Coroutine
    //==========================================================
    private IEnumerator LoadAfterDelay(string sceneName, float delay)
    {
        isLoading = true;

        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(sceneName);
    }

    //==========================================================
    // Door trigger
    //==========================================================
    void Update()
    {
        if (playerNearby && Input.GetMouseButtonDown(0))
        {
            SceneChange();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("[Door] Player entered trigger.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("[Door] Player exited trigger.");
        }
    }
}