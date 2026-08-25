using UnityEngine;

public class Wire : MonoBehaviour
{
    [Header("Color Settings")]
    public Color emptyColor = Color.white;
    public Color filledColor = new Color(1f, 0.8f, 0.2f, 1f);

    [HideInInspector] public bool isLocked = false;

    private GameObject[] orientations; // các prefab hợp lệ CHO RIÊNG ô này, do Manager cấp
    private int currentIndex = 0;
    private int targetIndex = 0;
    private GameObject currentVisual;
    private SpriteRenderer mainSprite;

    public void Setup(GameObject[] orientationPrefabs, int answerIndex)
    {
        orientations = orientationPrefabs;
        targetIndex = answerIndex;
        currentIndex = 0;
        ShowOrientation(0);
    }

    public void SetOrientation(int index)
    {
        if (orientations == null || orientations.Length == 0) return;
        currentIndex = ((index % orientations.Length) + orientations.Length) % orientations.Length;
        ShowOrientation(currentIndex);
    }

    public void RandomizeOrientation()
    {
        if (orientations == null || orientations.Length == 0) return;
        SetOrientation(Random.Range(0, orientations.Length));
    }

    public void UpdateInput()
    {
        if (isLocked || orientations == null || orientations.Length == 0) return;
        currentIndex = (currentIndex + 1) % orientations.Length;
        ShowOrientation(currentIndex);
    }

    private void ShowOrientation(int index)
    {
        if (currentVisual != null) Destroy(currentVisual);
        if (orientations == null || index < 0 || index >= orientations.Length) return;

        GameObject prefab = orientations[index];
        if (prefab == null) return;

        currentVisual = Instantiate(prefab, transform);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
        currentVisual.transform.localScale = Vector3.one; // sprite đã đúng size sẵn từ khi cắt

        mainSprite = currentVisual.GetComponent<SpriteRenderer>();
        if (mainSprite == null) mainSprite = currentVisual.GetComponentInChildren<SpriteRenderer>();
        if (mainSprite != null) mainSprite.color = emptyColor;
    }

    public void UpdateColor(bool filled)
    {
        if (mainSprite != null)
            mainSprite.color = filled ? filledColor : emptyColor;
    }
    public bool IsCorrect() => currentIndex == targetIndex;
}
