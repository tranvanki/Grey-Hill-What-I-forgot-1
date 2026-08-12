using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Attach to a GameObject in the transition scene.
/// Uses ONE TMP_Text object — changes text content for each card.
/// No Timeline needed.
/// 
/// Setup:
///   1. Scene background: solid black Camera (Clear Flags = Solid Color, black)
///   2. Canvas > Text (TMP) — assign to textLabel below
///   3. Fill in cards[] in Inspector
///   4. Set nextScene to your Chapter 2 scene name
/// </summary>
public class ChapterTransition : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text textLabel;

    [Header("Cards — fill in order")]
    [TextArea(2, 4)]
    public string[] cards = new string[]
    {
        "You woke up in a hospital bed.",
        "The next morning you was discharged from the hospital.",
        
        "You got a press badge.\nYou started asking questions.",
        "After 10 years, you are now become a journalist for the local Press.",
        "You're going back to Woodvine and find out why."
    };

    [Header("Timing")]
    public float fadeInDuration  = 1f;
    public float holdDuration    = 2.5f;
    public float fadeOutDuration = 1f;

    [Header("Scene")]
    public string nextScene = "Entrance";

    void Start()
    {
        if (textLabel == null)
        {
            Debug.LogError("[ChapterTransition] textLabel is not assigned!");
            return;
        }
        textLabel.alpha = 0f;
        StartCoroutine(PlayCards());
    }

    private IEnumerator PlayCards()
    {
        foreach (string card in cards)
        {
            textLabel.text = card;

            // Fade in
            yield return StartCoroutine(FadeTo(1f, fadeInDuration));

            // Hold
            yield return new WaitForSeconds(holdDuration);

            // Fade out
            yield return StartCoroutine(FadeTo(0f, fadeOutDuration));
        }

        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = textLabel.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textLabel.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        textLabel.alpha = targetAlpha;
    }
}
