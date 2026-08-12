using UnityEngine;

public class HintDisplayManager : MonoBehaviour
{
    public static HintDisplayManager Instance;

    [Header("Refs")]
    public GameObject hintRoot;
    public SpriteRenderer iconRenderer;
    public Sprite interactIcon; // Left click icon
    public Sprite pickupIcon;   // 'F' key icon

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (hintRoot != null) hintRoot.SetActive(false);
    }

    public void Show(HintUI target)
    {
        if (hintRoot == null) return;
        hintRoot.SetActive(true);
        iconRenderer.sprite = target.hintType == HintUI.HintType.Interact ? interactIcon : pickupIcon;
        hintRoot.transform.position = target.transform.position + target.hintOffset;
    }

    public void UpdatePosition(Vector3 pos)
    {
        if (hintRoot != null) hintRoot.transform.position = pos;
    }

    public void Hide()
    {
        if (hintRoot != null) hintRoot.SetActive(false);
    }
}
