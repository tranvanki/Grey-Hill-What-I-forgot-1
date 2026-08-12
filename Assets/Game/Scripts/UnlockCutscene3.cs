using UnityEngine;
using UnityEngine.SceneManagement; 
public class UnlockCutscene3 : MonoBehaviour
{
    [SerializeField] private string itemID = "image01"; 
    [SerializeField] private GameObject hintUI;

    void Start()
    {
       
        // CheckAndLoadCutscene(); 
    }

    public void CheckAndLoadCutscene()
    {
        if (GameState.TryGet(out GameState state))
        {
            
            if (state.GetInventoryIDs().Contains(itemID))
            {
                Debug.Log("Picked " + itemID + ",  load Cutscene 3...");
                SceneManager.LoadScene("CutsceneChap3");
            }
            else
            {
                Debug.Log("Not picked " + itemID + ", cannot open cutscene!");
            }
        }
    }
}