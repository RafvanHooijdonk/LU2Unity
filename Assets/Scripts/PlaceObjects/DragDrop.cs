using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    public bool isDragging = false;
    public MenuPanel menuPanel;
    public float gridSize = 1f;
    private static GameObject previewIndicator; // Gedeelde preview voor alle objecten

    private int prefabId;
    private string environmentId;
    private static readonly HttpClient client = new HttpClient();
    private string apiUrl = "https://avansict2230382.azurewebsites.net/api/Environment2D/CreateObject";

    void Start()
    {
        // Zoek of maak een preview-object
        if (previewIndicator == null)
        {
            previewIndicator = GameObject.Find("PlacementPreview");

            if (previewIndicator == null)
            {
                // Als het niet bestaat, maak het aan
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

        // Verstuur de API-call
        SendObjectToApi();
    }

    Vector3 SnapToGrid(Vector3 originalPosition)
    {
        float x = Mathf.Round(originalPosition.x / gridSize) * gridSize;
        float y = Mathf.Round(originalPosition.y / gridSize) * gridSize;
        float centerX = x + gridSize / 2f;
        float centerY = y + gridSize / 2f;
        return new Vector3(centerX, centerY, originalPosition.z);
    }

    private async void SendObjectToApi()
    {
        string token = AuthManager.instance?.AccessToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Geen token gevonden. Gebruiker moet ingelogd zijn.");
            return;
        }

        var objectData = new ObjectData
        {
            PrefabId = prefabId,
            PositionX = transform.position.x,
            PositionY = transform.position.y,
            ScaleX = transform.localScale.x,
            ScaleY = transform.localScale.y,
            RotationZ = transform.rotation.eulerAngles.z,
            SortingLayer = 8,
            EnvironmentId = environmentId.ToUpper()
        };

        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
                var json = ObjectJsonHelper.ToJson(objectData);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Object succesvol verzonden naar de API.");
                }
                else
                {
                    Debug.LogError($"Fout bij verzenden naar API: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error bij verzenden naar API: {ex.Message}");
        }
    }

    public void Initialize(int prefabId, string environmentId)
    {
        this.prefabId = prefabId;
        this.environmentId = environmentId;
    }
}