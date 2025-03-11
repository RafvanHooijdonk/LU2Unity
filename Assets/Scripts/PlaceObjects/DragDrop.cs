using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    public bool isDragging = false;
    public MenuPanel menuPanel;
    public float gridSize = 1f;
    private static GameObject previewIndicator; // Zorgt ervoor dat de preview gedeeld wordt door alle objecten

    void Start()
    {
        // Zoek of maak een preview-object
        if (previewIndicator == null)
        {
            previewIndicator = GameObject.Find("PlacementPreview");

            if (previewIndicator == null)
            {
                // Als het niet bestaat, maak het dan aan
                previewIndicator = new GameObject("PlacementPreview");
                SpriteRenderer sr = previewIndicator.AddComponent<SpriteRenderer>();
                sr.color = new Color(1, 1, 1, 0.5f); // Doorzichtig maken
            }
        }

        previewIndicator.SetActive(false);
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x, mousePos.y, 0);

            // Update de preview-positie
            if (previewIndicator != null)
            {
                previewIndicator.SetActive(true);
                previewIndicator.transform.position = SnapToGrid(transform.position);
            }
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        menuPanel?.HideOpenButton(true);
        if (previewIndicator != null) previewIndicator.SetActive(true);
    }

    void OnMouseUp()
    {
        isDragging = false;
        menuPanel?.HideOpenButton(false);
        transform.position = SnapToGrid(transform.position);

        if (previewIndicator != null)
            previewIndicator.SetActive(false);

        menuPanel?.HideMenu(true);
    }

    Vector3 SnapToGrid(Vector3 originalPosition)
    {
        float x = Mathf.Round(originalPosition.x / gridSize) * gridSize;
        float y = Mathf.Round(originalPosition.y / gridSize) * gridSize;
        float centerX = x + gridSize / 2f;
        float centerY = y + gridSize / 2f;
        return new Vector3(centerX, centerY, originalPosition.z);
    }
}
