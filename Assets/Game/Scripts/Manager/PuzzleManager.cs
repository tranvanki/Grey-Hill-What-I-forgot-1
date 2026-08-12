using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    [Header("Game Elements")]
    [Range(2, 6)]
    [SerializeField] private int gridSize = 4;
    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform piecePrefab;

    [Header("Puzzle Image")]
    [SerializeField] private Texture2D jigsawTexture;
    [SerializeField] private Camera mainCamera; 

    [Header("Navigation")]
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private string returnSceneName = "NightmareScene";    
    [SerializeField] private GameObject backButton;
    public bool isPuzzleCompleted = false;
    [Header("Completion")]
    [SerializeField] private float completionDelay = 2f;
    private List<Transform> pieces;
    

    
    private Vector2Int dimensions;
    private float width;
    private float height;
    
    private Transform draggingPiece = null;
    private Vector3 offset;

    private int piecesCorrect;

    void Start()
    {
        if (mainCamera != null)
            mainCamera.backgroundColor = new Color(0.04f, 0.04f, 0.04f); // dark horror background

        PuzzleSaveData saved = GameState.Instance?.LoadPuzzleFromJSON();
        if (saved != null && !saved.isCompleted)
            RestoreFromSave(saved);
        else
            StartGame();
    }

    public void StartGame()
    {   
        pieces = new List<Transform>();

        // Guard: kiểm tra texture và prefab trước khi chạy
        if (jigsawTexture == null)
        {
            Debug.LogError("[PuzzleManager] jigsawTexture is NULL! Kéo texture vào Inspector của PuzzleManager.");
            return;
        }
        if (piecePrefab == null)
        {
            Debug.LogError("[PuzzleManager] piecePrefab is NULL! Kéo Piece prefab vào Inspector của PuzzleManager.");
            return;
        }

     
        piecePrefab.gameObject.SetActive(true);

        dimensions = GetDimensions(jigsawTexture, gridSize);

        CreateJigsawPieces(jigsawTexture);


        piecePrefab.gameObject.SetActive(false);

        Scatter();

        UpdateBorder();

        piecesCorrect = 0;
        isPuzzleCompleted = false;
    }
private void RestoreFromSave(PuzzleSaveData saved)
    {
       StartGame();
       piecesCorrect = saved.piecesCorrect;
       foreach(PieceData pData in saved.pieces)
       {
            Transform piece = pieces.Find(p => p.name == pData.pieceName);
            if(piece != null)
            {
                piece.position = pData.position;
                if(Vector2.Distance(piece.localPosition, new Vector2(
                    (-width * dimensions.x / 2) + (width *
                     (pieces.IndexOf(piece) % dimensions.x)) + (width / 2),

                    (-height * dimensions.y / 2) + (height * 
                    (pieces.IndexOf(piece) / dimensions.x)) + (height / 2))) < (width / 2))
                {
                    piece.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
       }
    }
    Vector2Int GetDimensions(Texture2D texture, int diff)
    {
        Vector2Int dims = Vector2Int.zero;
        if (texture.width < texture.height)
        {
            dims.x = diff;
            dims.y = (diff * texture.height) / texture.width;
        }
        else
        {
            dims.x = (diff * texture.width) / texture.height;
            dims.y = diff;
        }
        return dims;
    }

    void CreateJigsawPieces(Texture2D texture)
    {
        height = 1f / dimensions.y;
        float aspect = (float)texture.width / texture.height;
        width = aspect / dimensions.x;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameHolder);
                piece.localPosition = new Vector3(
                    (-width * dimensions.x / 2) + (width * col) + (width / 2),
                    (-height * dimensions.y / 2) + (height * row) + (height / 2),
                    -1);
                piece.localScale = new Vector3(width, height, 1f);
                piece.name = $"Piece {(row * dimensions.x) + col}";
                pieces.Add(piece);

                float width1 = 1f / dimensions.x;
                float height1 = 1f / dimensions.y;

                Mesh mesh = new Mesh();
                // Vertices: bottom-left, bottom-right, top-left, top-right
                mesh.vertices = new Vector3[]
                {
                    new Vector3(-0.5f, -0.5f, 0),
                    new Vector3( 0.5f, -0.5f, 0),
                    new Vector3(-0.5f,  0.5f, 0),
                    new Vector3( 0.5f,  0.5f, 0)
                };
                mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
                // UV order: bottom-left, bottom-right, top-left, top-right
                mesh.uv = new Vector2[]
                {
                    new Vector2(width1 * col,       height1 * row),
                    new Vector2(width1 * (col + 1), height1 * row),
                    new Vector2(width1 * col,       height1 * (row + 1)),
                    new Vector2(width1 * (col + 1), height1 * (row + 1))
                };

                piece.GetComponent<MeshFilter>().mesh = mesh;
                
                // Clone material từ prefab (đảm bảo dùng đúng shader URP đã setup)
                // KHÔNG dùng Shader.Find vì tên shader có thể khác nhau tùy URP version
                MeshRenderer prefabRenderer = piecePrefab.GetComponent<MeshRenderer>();
                Material sourceMat = prefabRenderer != null ? prefabRenderer.sharedMaterial : null;
                
                Material mat;
                if (sourceMat != null)
                {
                    mat = new Material(sourceMat); // clone từ material gốc của prefab
                }
                else
                {
                    // Fallback nếu prefab không có MeshRenderer
                    Shader fallback = Shader.Find("Universal Render Pipeline/Unlit") 
                                   ?? Shader.Find("Sprites/Default")
                                   ?? Shader.Find("Standard");
                    mat = new Material(fallback);
                    Debug.LogWarning("[PuzzleManager] piecePrefab không có MeshRenderer, dùng fallback shader.");
                }
                
                // Set texture cho TẤT CẢ property name phổ biến (belt & suspenders)
                mat.mainTexture = texture;          // Legacy / Standard
                if (mat.HasProperty("_MainTex"))   mat.SetTexture("_MainTex", texture);  // Legacy
                if (mat.HasProperty("_BaseMap"))   mat.SetTexture("_BaseMap", texture);  // URP Unlit/Lit
                if (mat.HasProperty("_BaseColor"))  mat.SetColor("_BaseColor", Color.white); // đảm bảo không bị tint
                
                piece.GetComponent<MeshRenderer>().material = mat;
                
                Debug.Log($"[PuzzleManager] Piece {(row * dimensions.x) + col}: scale=({width:F3},{height:F3}), shader={mat.shader.name}");

                // Ensure BoxCollider2D exists for drag detection (remove 3D collider if present)
                BoxCollider col3D = piece.GetComponent<BoxCollider>();
                if (col3D != null) DestroyImmediate(col3D);
                BoxCollider2D boxCol = piece.GetComponent<BoxCollider2D>();
                if (boxCol == null) boxCol = piece.gameObject.AddComponent<BoxCollider2D>();
                boxCol.size = Vector2.one;
            }
        }
    }

    private void Scatter()
    {
        float orthoHeight = mainCamera.orthographicSize;

        float screenAspect = (float)Screen.width / Screen.height;
        float orthoWidth = screenAspect * orthoHeight;

        float pieceWidth = width * gameHolder.localScale.x;
        float pieceHeight = height * gameHolder.localScale.y;

        orthoHeight -= pieceHeight;
        orthoWidth -= pieceWidth;

        foreach (Transform piece in pieces)
        {
            float x = Random.Range(-orthoWidth, orthoWidth);
            float y = Random.Range(-orthoHeight, orthoHeight);
            piece.position = new Vector3(x, y, -1);
        }
    }

    private void UpdateBorder()
    {
        LineRenderer lineRenderer = gameHolder.GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameHolder.gameObject.AddComponent<LineRenderer>();

        float halfWidth = (width * dimensions.x) / 2f;
        float halfHeight = (height * dimensions.y) / 2f;

        lineRenderer.positionCount = 4;
        lineRenderer.loop = true;
        lineRenderer.SetPosition(0, new Vector3(-halfWidth,  halfHeight, 0));
        lineRenderer.SetPosition(1, new Vector3( halfWidth,  halfHeight, 0));
        lineRenderer.SetPosition(2, new Vector3( halfWidth, -halfHeight, 0));
        lineRenderer.SetPosition(3, new Vector3(-halfWidth, -halfHeight, 0));

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        
        // Sửa lỗi viền bị màu hồng (Magenta) do thiếu Material
        if (lineRenderer.material == null || lineRenderer.material.name == "Default-Material")
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
        }

        lineRenderer.enabled = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            if (hit)
            {
                if (SFXManager.Instance != null) SFXManager.Instance.PlaySFX(SFXManager.SFXType.PuzzleClick);
                draggingPiece = hit.transform;
                offset = draggingPiece.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
                offset += Vector3.back;
            }
        }

        if (draggingPiece && Input.GetMouseButtonUp(0))
        {
            SnapAndDisableIfCorrect();
            draggingPiece.position += Vector3.forward;
            draggingPiece = null;
        }

        if (draggingPiece)
        {
            Vector3 newPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            newPosition += offset;
            draggingPiece.position = newPosition;
        }
    }

    private void SnapAndDisableIfCorrect()
    {
        int pieceIndex = pieces.IndexOf(draggingPiece);
        int col = pieceIndex % dimensions.x;
        int row = pieceIndex / dimensions.x;

        Vector2 targetPosition = new(
            (-width * dimensions.x / 2) + (width * col) + (width / 2),
            (-height * dimensions.y / 2) + (height * row) + (height / 2));

        if (Vector2.Distance(draggingPiece.localPosition, targetPosition) < (width / 2))
        {
            draggingPiece.localPosition = targetPosition;
            draggingPiece.GetComponent<BoxCollider2D>().enabled = false;

            piecesCorrect++;
            if (piecesCorrect == pieces.Count)
            {
                isPuzzleCompleted = true;
                Debug.Log("Puzzle Completed!");
                OnPuzzleCompleted(); 
            }
        }
    }

    public void RestartGame()
    {
        foreach (Transform piece in pieces)
        {
            Destroy(piece.gameObject);
        }
        pieces.Clear();

        gameHolder.GetComponent<LineRenderer>().enabled = false;

        StartGame();
    }

    private void OnPuzzleCompleted()
    {
        

        if (GameState.Instance != null)
            GameState.Instance.DeletePuzzleSave();

        StartCoroutine(AutoTransitionAfterComplete());
    }

    private IEnumerator AutoTransitionAfterComplete()
    {
        yield return new WaitForSeconds(completionDelay);

        if (sceneTransition != null)
            sceneTransition.SceneChange();
        else
            Debug.LogWarning("[PuzzleManager] sceneTransition did not assigned!");
    }
   
    public void GoBack()
{
    if (sceneTransition == null)
    {
        Debug.LogWarning("[PuzzleManager] sceneTransition is not assigned.");
        return;
    }

    
    if (isPuzzleCompleted)
    {
        if (GameState.Instance != null)
            GameState.Instance.DeletePuzzleSave();
    }
    else
    {
        SavePuzzleState();
        Debug.Log("[PuzzleManager] Saved  Back");
    }

  
    if (string.IsNullOrWhiteSpace(returnSceneName))
    {
        Debug.LogError("[PuzzleManager] returnSceneName is empty!");
        return;
    }

    Debug.Log($"[PuzzleManager] Back to  scene: {returnSceneName}");
    sceneTransition.TransitionToScene(returnSceneName);
}
    private void SavePuzzleState()
    {
        if(GameState.Instance == null)
        {
            return;
        }
        PuzzleSaveData saveData = new PuzzleSaveData();
        saveData.isCompleted = isPuzzleCompleted;
        saveData.piecesCorrect = piecesCorrect;
        foreach(Transform piece in pieces)
        {
            PieceData pData = new PieceData();
            pData.pieceName = piece.name;
            pData.position =  piece.position;

            saveData.pieces.Add(pData);
        }
        GameState.Instance.SavePuzzleToJSON(saveData);
    }
}
