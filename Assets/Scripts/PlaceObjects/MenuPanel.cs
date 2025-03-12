using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public GameObject panel; 
    public Button openButton; 
    public Button closeButton; 
    public List<GameObject> prefabs;
    public List<GameObject> clones = new List<GameObject>();
    private HttpClient client = new HttpClient(); 

    private GameObject selectedPrefab; 

    private void Start()
    {
        panel.SetActive(false);
        openButton.onClick.AddListener(() => HideMenu(true));
        closeButton.onClick.AddListener(() => HideMenu(false));

        GetObjectsFromEnvironment();
    }

    private async void GetObjectsFromEnvironment()
    {
        string environmentId = GameManager.instance != null ? GameManager.instance.SelectedEnvironmentId : "UNKNOWN";
        if (string.IsNullOrEmpty(environmentId) || environmentId == "UNKNOWN")
        {
            Debug.LogError("Geen geldig EnvironmentId gevonden!");
            return;
        }

        string token = AuthManager.instance?.AccessToken;
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("Geen token gevonden. Gebruiker moet ingelogd zijn.");
            return;
        }

        string apiUrl = $"https://avansict2230382.azurewebsites.net/api/Environment2D/GetObjects?environmentId={environmentId}";

        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, apiUrl))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Debug.Log("Objecten succesvol opgehaald van de API.");

                    var objects = ObjectJsonHelper.FromJson<List<ObjectData>>(responseBody);

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
    public void CreateObjectFromData(ObjectData objData)
    {
        if (objData.PrefabId < 0 || objData.PrefabId >= prefabs.Count)
        {
            Debug.LogError("Ongeldige PrefabId: " + objData.PrefabId);
            return;
        }

        var position = new Vector3(objData.PositionX, objData.PositionY, 0);
        var rotation = Quaternion.Euler(0, 0, objData.RotationZ);
        var scale = new Vector3(objData.ScaleX, objData.ScaleY, 1);

        var well = Instantiate(prefabs[objData.PrefabId], position, rotation);
        well.transform.localScale = scale;

        clones.Add(well);

        var dadWell = well.GetComponent<DragAndDrop>();
        if (dadWell != null)
        {
            dadWell.isDragging = false; 
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

        dadWell.Initialize(prefabIndex, environmentId);

        HideMenu(false);
        HideOpenButton(true);
    }

    public void HideMenu(bool show)
    {
        panel.SetActive(show);
        openButton.gameObject.SetActive(!show);
    }

    public void HideOpenButton(bool hide)
    {
        openButton.gameObject.SetActive(!hide);
    }

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

    public void PlaceSelectedObject()
    {
        if (selectedPrefab == null)
        {
            Debug.LogError("Geen prefab geselecteerd.");
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; 

        var placedObject = Instantiate(selectedPrefab, mousePos, Quaternion.identity);
        clones.Add(placedObject);

        var dadWell = placedObject.GetComponent<DragAndDrop>();
        if (dadWell != null)
        {
            dadWell.isDragging = false;
            dadWell.menuPanel = this;
            dadWell.Initialize(0, ""); 
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
    public static string ToJson<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj);
    }
    public static T FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}
