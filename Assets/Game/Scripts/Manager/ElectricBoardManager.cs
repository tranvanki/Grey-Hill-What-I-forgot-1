using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CellSetup
{
    [Header("grid(0-4)")]
    public int row;
    public int col;

    [Header("")]
    public GameObject[] orientationPrefabs;

    [Header("")]
    public int answerIndex;

    [Header("AAA")]
    public bool locked;
}

public class ElectricBoardManager : MonoBehaviour
{
    [Header("Cell Prefab ()")]
    [SerializeField] private Wire cellPrefab;

    [Header("grid setup(5x5)")]
    [SerializeField] private CellSetup[] cellSetups;

    private Wire[,] gridWires = new Wire[5, 5];
    private bool isSolved = false;
    private bool _hasUserInteracted = false;

    private GameObject _hiddenPlayer;
    private GameObject _hiddenUIRoot;

    void Start()
    {
        HidePersistentObjects();
        GeneratePuzzle();
    }

    private void HidePersistentObjects()
    {
        if (PlayerManager.Instance != null)
        {
            _hiddenPlayer = PlayerManager.Instance.gameObject;
            _hiddenPlayer.SetActive(false);
        }
        if (UIManager.Instance != null)
        {
            _hiddenUIRoot = UIManager.Instance.transform.root.gameObject;
            _hiddenUIRoot.SetActive(false);
        }
    }

    private void ShowPersistentObjects()
    {
        if (_hiddenPlayer != null) _hiddenPlayer.SetActive(true);
        if (_hiddenUIRoot != null) _hiddenUIRoot.SetActive(true);
    }

    private void GeneratePuzzle()
    {
        foreach (var cell in cellSetups)
        {
            if (cell.orientationPrefabs == null || cell.orientationPrefabs.Length == 0) continue;

            Vector2 spawnPos = new Vector2(cell.col, -cell.row);
            Wire newWire = Instantiate(cellPrefab, spawnPos, Quaternion.identity, this.transform);

            newWire.Setup(cell.orientationPrefabs, cell.answerIndex);
            newWire.isLocked = cell.locked;

            if (cell.locked)
                newWire.SetOrientation(cell.answerIndex); 
            else
                newWire.RandomizeOrientation(); 

            gridWires[cell.row, cell.col] = newWire;
        }

        Camera.main.transform.position = new Vector3(2f, -2f, -10f);
        Camera.main.orthographicSize = 4f;

        UpdateAllColors();
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
                if (clickedWire != null && !clickedWire.isLocked)
                {
                    if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.PuzzleClick);
                    _hasUserInteracted = true;
                    clickedWire.UpdateInput();
                    CheckCircuit();
                }
            }
        }
    }

    private void UpdateAllColors()
    {
        foreach (var wire in gridWires)
        {
            if (wire != null) wire.UpdateColor(wire.IsCorrect());
        }
    }

    private void CheckCircuit()
    {
        UpdateAllColors();
        if (!_hasUserInteracted) return;

        foreach (var wire in gridWires)
        {
            if (wire != null && !wire.IsCorrect()) return;
        }

        isSolved = true;
        Debug.Log("[ElectricBoardManager] Victory! Puzzle Matched!");

        if (GameState.TryGet(out GameState state))
        {
            state.RestorePowerAndUnlockElevator();
            ShowPersistentObjects();
            string targetScene = !string.IsNullOrEmpty(state.PreviousScene) ? state.PreviousScene : "Reception";
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            ShowPersistentObjects();
            SceneManager.LoadScene("Reception");
        }
    }

    void OnDestroy()
    {
        ShowPersistentObjects();
    }
    
}
