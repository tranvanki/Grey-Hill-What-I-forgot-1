using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class Chap2CutsceneController : MonoBehaviour
{
     [SerializeField] private PlayableDirector director;
    [SerializeField] private string nextSceneName = "Corrider_Left"; // scene chứa Corridor_left

    private void Awake()
    {
        if (director == null) director = GetComponent<PlayableDirector>();
    }

    private void OnEnable()  => director.stopped += OnCutsceneFinished;
    private void OnDisable() => director.stopped -= OnCutsceneFinished;

    private void OnCutsceneFinished(PlayableDirector pd)
    {
        Debug.Log("[Chap2Cutscene] Cutscene finished, loading next scene...");
       
        SceneManager.LoadScene(nextSceneName);
    }
}
