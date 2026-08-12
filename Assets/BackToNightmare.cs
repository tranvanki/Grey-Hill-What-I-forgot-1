using UnityEngine;
using UnityEngine.SceneManagement;
public class BackToNightmare : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private SceneTransition sceneTransition;

    [SerializeField] private string targetSceneName = "NightmareScene";

    public void BackToNightmareScene()
    {
        if (sceneTransition != null)
        {
            sceneTransition.TransitionToScene(targetSceneName);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
