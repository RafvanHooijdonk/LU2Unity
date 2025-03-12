using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public GameObject panel; // Het menu TEST
    public Button openButton; // "+" knop
    public Button closeButton; // "X" knop
    public List<GameObject> prefabs;
    public List<GameObject> clones = new List<GameObject>();
    private HttpClient client = new HttpClient(); // HttpClient voor API-verzoeken

    private GameObject selectedPrefab; // Dit zal het geselecteerde prefab zijn voor plaatsing

    private void Start()
    {
        panel.SetActive(false);
        openButton.onClick.AddListener(() => HideMenu(true));
        closeButton.onClick.AddListener(() => HideMenu(false));

        // Haal de objecten op bij het starten van de scene (onafhankelijk van het menu)
        FetchObjectsFromApi();
    }

    // Methode om objecten van de API op te halen
    private async void FetchObjectsFromApi()
    {
        // Haal de geselecteerde EnvironmentId op uit GameManager
        string environmentId = GameManager.instance != null ? GameManager.instance.SelectedEnvironmentId : "UNKNOWN";
        if (string.IsNullOrEmpty(environmentId) || environmentId == "UNKNOWN")
        {
            Debug.LogError("Geen geldig EnvironmentId gevonden!");
            return;
        }

        // Haal de Bearer token op uit AuthManager
        string token = AuthManager.instance?.AccessToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Geen token gevonden. Gebruiker moet ingelogd zijn.");
            return;
        }

        // Bouw de URL met de queryparameter voor environmentId
        string apiUrl = $"https://avansict2230382.azurewebsites.net/api/Environment2D/GetObjects?environmentId={environmentId}";

        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, apiUrl))
            {
                // Voeg de Authorization header toe
                request.Headers.Add("Authorization", $"Bearer {token}");

                // Stuur de GET-aanroep naar de API
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Lees de JSON-respons
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Debug.Log("Objecten succesvol opgehaald van de API.");

                    // Converteer de JSON naar een lijst van ObjectData met behulp van de ObjectJsonHelper
                    var objects = ObjectJsonHelper.FromJson<List<ObjectData>>(responseBody);

                    // Maak de objecten aan in Unity
                    foreach (var objData in objects)
                    {
                        CreateObjectFromData(objData);
                    }
                }
                else
                {
                    Debug.LogError($"Fout bij ophalen van objecten: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error bij ophalen van objecten van de API: {ex.Message}");
        }
    }

    // Methode om een object te maken op basis van ObjectData
    public void CreateObjectFromData(ObjectData objData)
    {
        // Controleer of er een prefab is voor dit objectId
        if (objData.PrefabId < 0 || objData.PrefabId >= prefabs.Count)
        {
            Debug.LogError("Ongeldige PrefabId: " + objData.PrefabId);
            return;
        }

        // Instantieer het prefab op basis van de gegeven positie en rotatie
        var position = new Vector3(objData.PositionX, objData.PositionY, 0);
        var rotation = Quaternion.Euler(0, 0, objData.RotationZ);
        var scale = new Vector3(objData.ScaleX, objData.ScaleY, 1);

        var well = Instantiate(prefabs[objData.PrefabId], position, rotation);
        well.transform.localScale = scale;

        // Voeg de objecten toe aan de clones lijst
        clones.Add(well);

        // Voeg andere logica toe zoals DragAndDrop
        var dadWell = well.GetComponent<DragAndDrop>();
        if (dadWell != null)
        {
            dadWell.isDragging = false; // Zorg ervoor dat het object niet aan de muis blijft hangen
            dadWell.menuPanel = this;
            dadWell.Initialize(objData.PrefabId, objData.EnvironmentId);
        }
    }

    public void CreateGameObjectFromClick(int prefabIndex)
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

        // Geef de environmentId mee aan DragAndDrop zodat het later gebruikt kan worden TEST
        dadWell.Initialize(prefabIndex, environmentId);

        // Verberg het menu en de "+" knop bij slepen
        HideMenu(false);
        HideOpenButton(true);
    }
    // Verberg of toon het menu
    public void HideMenu(bool show)
    {
        panel.SetActive(show);
        openButton.gameObject.SetActive(!show);
    }

    // Verberg of toon de "+" knop
    public void HideOpenButton(bool hide)
    {
        openButton.gameObject.SetActive(!hide);
    }

    // Selecteer een prefab voor plaatsing via het menu
    public void SelectPrefab(int prefabIndex)
    {
        if (prefabIndex >= 0 && prefabIndex < prefabs.Count)
        {
            selectedPrefab = prefabs[prefabIndex];
            Debug.Log($"Prefab {prefabIndex} geselecteerd voor plaatsing.");
        }
        else
        {
            Debug.LogError("Ongeldige prefab index.");
        }
    }

    // Plaats het geselecteerde object in de scene op de muispositie
    public void PlaceSelectedObject()
    {
        if (selectedPrefab == null)
        {
            Debug.LogError("Geen prefab geselecteerd.");
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // Z-positie moet 0 zijn voor 2D-wereld

        var placedObject = Instantiate(selectedPrefab, mousePos, Quaternion.identity);
        clones.Add(placedObject);

        // Voeg andere logica toe zoals DragAndDrop
        var dadWell = placedObject.GetComponent<DragAndDrop>();
        if (dadWell != null)
        {
            dadWell.isDragging = false;
            dadWell.menuPanel = this;
            dadWell.Initialize(0, ""); // Stel de juiste waarden in afhankelijk van je implementatie
        }

        Debug.Log("Object geplaatst!");
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
    // Methode om een object naar JSON om te zetten
    public static string ToJson<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    // Methode om JSON om te zetten naar een object
    public static T FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}
