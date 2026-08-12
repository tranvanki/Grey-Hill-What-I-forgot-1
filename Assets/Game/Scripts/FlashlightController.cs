using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class FlashlightController : MonoBehaviour
{
    public static FlashlightController Instance { get; private set; }

    // ── URP Light2D ──────────────────────────────────────────────
    [Header("URP Light2D")]
    [SerializeField] private Light2D spotLight2D;

    // ── Cone Mesh ────────────────────────────────────────────────
    [Header("Cone Settings")]
    [SerializeField] private float coneLength = 12f;
    [SerializeField] private float coneAngle = 50f;
    [SerializeField] private int rayCount = 20;
    [SerializeField] private LayerMask wallMask;

    // ── Toggle ───────────────────────────────────────────────────
    [Header("Toggle")]
    public KeyCode toggleKey = KeyCode.Q;
    [Header("Scene Gating")]
    [SerializeField] private string[] hideLightScenes =
    {
        "CutsceneChap3",
    };

    [Header("Debug/Testing")]
    public bool startUnlocked = false;  
        private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private PolygonCollider2D _collider;

    private bool _hasLight = false;           
    private bool _isOn = false;                
    private bool _forceDisabledByScene = false; 
    private Vector2 _facingDir = Vector2.down;  

    void Awake()
    {    
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[Flashlight] Duplicate instance detected. Current: {gameObject.name}, Existing: {Instance.gameObject.name}");
            Destroy(this);
            return;
        }
        Instance = this;

        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<PolygonCollider2D>();
        _collider.isTrigger = true;

        _mesh = new Mesh { name = "FlashlightCone" };
        _meshFilter.mesh = _mesh;

        if (spotLight2D == null)
            Debug.LogWarning("[Flashlight] Light2D reference is missing! Assign 'Spot Light 2D' in Inspector.", this);
        Debug.Log($"[DEBUG] FlashlightController Awake: GameObject.tag='{gameObject.tag}', name='{gameObject.name}'", this);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        bool unlockedByGameState = GameState.TryGet(out GameState state)
            && state.GetInventoryIDs().Contains("Flashlight");

        _hasLight = startUnlocked || unlockedByGameState;

        ApplySceneGate(SceneManager.GetActiveScene().name);
        if(!_hasLight && !_forceDisabledByScene)
        {
            SetFlashlight(false);
        }

    }

    void Update()
    {
        if (!_hasLight || _forceDisabledByScene) return;

        if (Input.GetKeyDown(toggleKey))
            SetFlashlight(!_isOn);

        UpdateRotation();

        if (_isOn) BuildCone();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneGate(scene.name);
    }

    // Turns the light fully off and locks it out while in a gated scene;
    // restores normal toggle behavior otherwise.
    private void ApplySceneGate(string sceneName)
    {
        bool shouldHide = System.Array.IndexOf(hideLightScenes, sceneName) >= 0;
        _forceDisabledByScene = shouldHide;

        if (shouldHide)
        {
            SetFlashlight(false);
        }
        else if (_hasLight && startUnlocked)
        {
            // Only auto-restore if the light was meant to be on by default;
            
            SetFlashlight(true);
        }
    }

    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(_facingDir.y, _facingDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // Called from PlayerMovement to update the flashlight's facing direction.
    public void SetFacingDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        _facingDir = dir.normalized;
    }

    public void UnlockFlashlight()
    {
        _hasLight = true;
        SetFlashlight(!_forceDisabledByScene);

    }

    private void SetFlashlight(bool on)
    {
        _isOn = on;

        if (_meshRenderer != null) _meshRenderer.enabled = on;
        if (_collider != null) _collider.enabled = on;
        if (spotLight2D != null) spotLight2D.enabled = on;


        if (!on) _mesh.Clear();
    }

    public void BuildCone()
    {
        Vector3[] verts = new Vector3[rayCount + 2];
        int[] tris = new int[rayCount * 3];

        verts[0] = Vector3.zero;

        float halfAngle = coneAngle * 0.5f;
        float step = coneAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float rad = (-halfAngle + step * i) * Mathf.Deg2Rad;
            Vector2 localDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 worldDir = transform.TransformDirection(localDir);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, worldDir, coneLength, wallMask);
            float dist = hit.collider != null ? hit.distance : coneLength;
            verts[i + 1] = localDir * dist;
        }

        for (int i = 0; i < rayCount; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }

        _mesh.Clear();
        _mesh.vertices = verts;
        _mesh.triangles = tris;
        _mesh.RecalculateNormals();

        SyncCollider(verts);
    }

    private void SyncCollider(Vector3[] localVerts)
    {
        Vector2[] points = new Vector2[localVerts.Length];

        for (int i = 0; i < localVerts.Length; i++)
            points[i] = localVerts[i];

        _collider.SetPath(0, points);
        
        if (Time.frameCount % 60 == 0) 
        {
            string samplePoints = points.Length > 2 ? $"p[0]={points[0]}, p[1]={points[1]}, p[last]={points[points.Length-1]}" : "too few points";
            Debug.Log($"[DEBUG] Flashlight collider updated: {points.Length} points. {samplePoints}", this);
        }
    }
}