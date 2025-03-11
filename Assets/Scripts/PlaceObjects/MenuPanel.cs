using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public GameObject panel; // Het menu
    public Button openButton; // "+" knop
    public Button closeButton; // "X" knop
    public List<GameObject> prefabs;
    public List<GameObject> clones = new List<GameObject>();

    private static readonly HttpClient client = new HttpClient();
    private string apiUrl = "https://avansict2230382.azurewebsites.net/api/Environment2D/CreateObject";

    private void Start()
    {
        panel.SetActive(false);
        openButton.onClick.AddListener(() => HideMenu(true));
        closeButton.onClick.AddListener(() => HideMenu(false));
    }

    public async void CreateGameObjectFromClick(int prefabIndex)
    {
        string environmentId = GameManager.instance != null ? GameManager.instance.SelectedEnvironmentId : "UNKNOWN";

        if (string.IsNullOrEmpty(environmentId) || environmentId == "UNKNOWN")
        {
            Debug.LogError("Geen geldig EnvironmentId gevonden!");
            return;
        }

        Debug.Log($"Gekozen EnvironmentId: {environmentId}");

        var well = Instantiate(prefabs[prefabIndex], Vector3.zero, Quaternion.identity);
        var dadWell = well.GetComponent<DragAndDrop>();

        clones.Add(well);
        dadWell.isDragging = true;
        dadWell.menuPanel = this;

        // Maak het object
        var objectData = new ObjectData
        {
            PrefabId = prefabIndex,
            PositionX = well.transform.position.x,
            PositionY = well.transform.position.y,
            ScaleX = well.transform.localScale.x,
            ScaleY = well.transform.localScale.y,
            RotationZ = well.transform.rotation.eulerAngles.z,
            SortingLayer = 0, 
            EnvironmentId = environmentId //
        };

        // Log de waarden om te zien wat er wordt verzonden
        Debug.Log($"ObjectData - PrefabId: {objectData.PrefabId}, EnvironmentId: {objectData.EnvironmentId}");
        Debug.Log($"ObjectData - PositionX: {objectData.PositionX}, PositionY: {objectData.PositionY}, ScaleX: {objectData.ScaleX}, ScaleY: {objectData.ScaleY}, RotationZ: {objectData.RotationZ}");

        // Verstuur het object naar de API
        await SendObjectToApi(objectData);

        // Verberg het menu en de "+" knop bij slepen
        HideMenu(false);
        HideOpenButton(true);
    }

    // Verzend de objectgegevens naar de API
    private async Task SendObjectToApi(ObjectData objectData)
    {
        // Haal het token op uit AuthManager
        string token = AuthManager.instance?.AccessToken;

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Geen token gevonden. Gebruiker moet ingelogd zijn.");
            return;
        }

        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
            {
                // Stel de content in als JSON
                request.Headers.Add("Authorization", $"Bearer {token}");
                var json = ObjectJsonHelper.ToJson(objectData);
                Debug.Log("JSON: " + json);
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

    // Verberg of toon het menu
    public void HideMenu(bool show)
    {
        panel.SetActive(show);
        openButton.gameObject.SetActive(!show); // Toon de "+" knop als het menu verborgen is
    }

    // Verberg of toon de "+" knop
    public void HideOpenButton(bool hide)
    {
        openButton.gameObject.SetActive(!hide); // De knop verbergen als 'hide' true is, anders tonen
    }

    // Verwijder alle geplaatste objecten
    public void clear()
    {
        Debug.Log("Clearing");
        foreach (var i in clones)
        {
            Destroy(i);
        }
    }
}

[Serializable]
public class ObjectData
{
    public int PrefabId;
    public float PositionX;
    public float PositionY;
    public float ScaleX;
    public float ScaleY;
    public float RotationZ;
    public int SortingLayer;
    public string EnvironmentId;
}

public static class ObjectJsonHelper
{
    public static string ToJson<T>(T obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static T FromJson<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }
}
