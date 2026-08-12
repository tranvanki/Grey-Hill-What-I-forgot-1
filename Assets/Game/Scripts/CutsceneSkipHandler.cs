using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneSkipHandler : MonoBehaviour
{
    [Header("Cutscene Type — assign ONE")]
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private ChapterTransition chapterTransition;

    [Header("Skip Button UI")]
    [SerializeField] private GameObject skipButtonRoot;
    [SerializeField] private Button skipButton;

    [Header("Behaviour")]
    [SerializeField] private float showButtonDelay = 2f;
    [SerializeField] private string overrideNextScene = "";

    private void Awake()
    {
        skipButton?.onClick.AddListener(OnSkipPressed);
        skipButtonRoot?.SetActive(false);
    }

    private void Start() => StartCoroutine(ShowButtonAfterDelay());

    private IEnumerator ShowButtonAfterDelay()
    {
        if (showButtonDelay > 0f)
            yield return new WaitForSeconds(showButtonDelay);
        skipButtonRoot?.SetActive(true);
    }

    public void OnSkipPressed()
    {
        if (skipButton != null) skipButton.interactable = false;
        StopAllCoroutines();

        if (timelineDirector != null)
        {
            timelineDirector.time = timelineDirector.duration;
            timelineDirector.Evaluate();
            timelineDirector.Stop();
        }

        string target = !string.IsNullOrWhiteSpace(overrideNextScene)
            ? overrideNextScene
            : chapterTransition != null ? chapterTransition.nextScene : "";

        if (string.IsNullOrWhiteSpace(target))
        {
            Debug.LogWarning("[CutsceneSkipHandler] Không xác định được scene tiếp theo. Gán overrideNextScene trong Inspector.");
            if (skipButton != null) skipButton.interactable = true;
            return;
        }

        StartCoroutine(LoadScene(target));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        if (GameState.HasInstance)
            GameState.Instance.SetPreviousScene(SceneManager.GetActiveScene().name);

        yield return null;
        SceneManager.LoadScene(sceneName);
    }
}