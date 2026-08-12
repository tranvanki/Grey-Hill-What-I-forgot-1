using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{   
    public AudioSource buttonClickSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
 public void Play()
    {
        StartCoroutine(PlayAndLoad());
    }

    IEnumerator PlayAndLoad()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.ClickButton);
        buttonClickSound.Play();

        yield return new WaitForSeconds(buttonClickSound.clip.length);

        SceneManager.LoadScene("HospitalScene");
    }
    public void Continue()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.ClickButton);
        GameState.Instance.RespawnFromCheckpoint();
        buttonClickSound.Play();
    }  
   public void QuitGame() 
{
    if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.ClickButton);
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        buttonClickSound.Play();
    #else
        Application.Quit();
    #endif
}
    // Update is called once per frame
    void Update()
    {
        
    }
}
