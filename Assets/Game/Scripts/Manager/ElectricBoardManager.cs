using UnityEngine;

using System.Collections.Generic;

using UnityEngine.SceneManagement;

using UnityEngine.EventSystems;



public class ElectricBoardManager : MonoBehaviour

{

    [Header("Cell Prefab")]

    [SerializeField] private Wire cellPrefab;



    private Wire[,] gridWires = new Wire[5, 5];

    private Wire sourceWire;

    private Wire targetWire;

    private bool isSolved = false;

    private bool _hasUserInteracted = false;

    private GameObject _hiddenPlayer;

    private GameObject _hiddenUIRoot;



    private readonly int[,] puzzleMap = new int[5, 5] {

        { 0, 3, 2, 3, 0 },

        { 0, 2, 0, 2, 0 },

        { 1, 4, 0, 4, 1 },

        { 0, 2, 0, 2, 0 },

        { 0, 3, 2, 3, 0 }

    };



    void Start()

    {

        HidePersistentObjects();

        CleanupDuplicateEventSystems();

        GeneratePuzzle();

    }



 

    private void HidePersistentObjects()

    {

       

        if (PlayerManager.Instance != null)

        {

            _hiddenPlayer = PlayerManager.Instance.gameObject;

            _hiddenPlayer.SetActive(false);

            Debug.Log("[ElectricBoardManager] Hidden Player for puzzle scene.");

        }



       

        if (UIManager.Instance != null)

        {

            _hiddenUIRoot = UIManager.Instance.transform.root.gameObject;

            _hiddenUIRoot.SetActive(false);

            Debug.Log("[ElectricBoardManager] Hidden UI root for puzzle scene.");

        }

    }



   

    private void ShowPersistentObjects()

    {

        if (_hiddenPlayer != null)

        {

            _hiddenPlayer.SetActive(true);

            Debug.Log("[ElectricBoardManager] Restored Player visibility.");

        }

        if (_hiddenUIRoot != null)

        {

            _hiddenUIRoot.SetActive(true);

            Debug.Log("[ElectricBoardManager] Restored UI root visibility.");

        }

    }



 

    private void CleanupDuplicateEventSystems()

    {

        EventSystem[] systems = FindObjectsOfType<EventSystem>();

        if (systems.Length > 1)

        {

            for (int i = 1; i < systems.Length; i++)

            {

                Debug.Log($"[ElectricBoardManager] Destroying duplicate EventSystem: {systems[i].name}");

                Destroy(systems[i].gameObject);

            }

        }

    }



private void GeneratePuzzle()
{
    for (int row = 0; row < 5; row++)
    {
        for (int col = 0; col < 5; col++)
        {
            int type = puzzleMap[row, col];
            if (type == 0) continue;

            Vector2 spawnPos = new Vector2(col, -row);
            Wire newWire = Instantiate(cellPrefab, spawnPos, Quaternion.identity, this.transform);
            newWire.Init(type);

            if (row == 2 && col == 0)
            {
                sourceWire = newWire;
                newWire.isLocked = true;
                newWire.transform.eulerAngles = Vector3.zero;
            }
            else if (row == 2 && col == 4)
            {
                targetWire = newWire;
                newWire.isLocked = true;
                newWire.transform.eulerAngles = Vector3.zero;
            }
            else if (row == 0 && col == 1)
            {
                newWire.transform.eulerAngles = new Vector3(0, 0, -90f);
            }
            else if (col == 3)
            {
                int randomRot = Random.Range(0, 4);
                newWire.transform.eulerAngles = new Vector3(0, 0, randomRot * -90f);
            }
            else
            {
                newWire.transform.eulerAngles = Vector3.zero;
            }

            gridWires[row, col] = newWire;
        }
    }

    Camera.main.transform.position = new Vector3(2f, -2f, -10f);
    Camera.main.orthographicSize = 4f;

    StartCoroutine(UpdateCircuitDelay());
}



    void Update()

    {

        if (isSolved) return;



        if (Input.GetMouseButtonDown(0))

        {

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

           

            if (hit.collider != null)

            {

                Wire clickedWire = hit.collider.GetComponentInParent<Wire>();

                if (clickedWire != null)

                {

                    if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.PuzzleClick);

                    _hasUserInteracted = true;

                    clickedWire.UpdateInput();

                    StartCoroutine(UpdateCircuitDelay());

                }

            }

        }

    }



    private void CheckCircuit()

    {
        Physics2D.SyncTransforms();
        foreach (var wire in gridWires)

        {

            if (wire != null) wire.isFilled = false;

        }
        if (sourceWire == null) return;

        Queue<Wire> queue = new Queue<Wire>();

        HashSet<Wire> visited = new HashSet<Wire>();

        queue.Enqueue(sourceWire);

        visited.Add(sourceWire);

        while (queue.Count > 0)

        {

            Wire current = queue.Dequeue();

            current.isFilled = true;

            foreach (var neighbor in current.GetConnectedWires())

            {

                if (!visited.Contains(neighbor))

                {

                    visited.Add(neighbor);

                    queue.Enqueue(neighbor);

                }

            }

        }

        foreach (var wire in gridWires)

        {

            if (wire != null) 
            Debug.Log($"[ElectricBoardManager] Wire at {wire.transform.position} isFilled = {wire.isFilled}");

        }



        if (targetWire != null && targetWire.isFilled && _hasUserInteracted)

        {

            isSolved = true;

            Debug.Log("[ElectricBoardManager] Victory! The circuit is complete!");

           

            if (GameState.TryGet(out GameState state))

            {

                Debug.Log($"[ElectricBoardManager] GameState found. PreviousScene = '{state.PreviousScene}'");

                state.RestorePowerAndUnlockElevator();

               

                ShowPersistentObjects();

                string targetScene = !string.IsNullOrEmpty(state.PreviousScene) ? state.PreviousScene : "Reception";

                SceneManager.LoadScene(targetScene);

            }

            else

            {

                Debug.LogWarning("[ElectricBoardManager] GameState is NULL! Loading Reception as fallback.");

                ShowPersistentObjects();

                SceneManager.LoadScene("Reception");

            }

        }

    }
    
    private System.Collections.IEnumerator UpdateCircuitDelay()
    {

        yield return null;
        CheckCircuit();
    }

    void OnDestroy()

    {
        ShowPersistentObjects();
    }

}