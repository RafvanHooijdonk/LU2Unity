using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public GameObject panel; // Het menu TEST
    public Button openButton; // "+" knop
    public Button closeButton; // "X" knop
    public List<GameObject> prefabs;
    public List<GameObject> clones = new List<GameObject>();

    private void Start()
    {
        panel.SetActive(false);
        openButton.onClick.AddListener(() => HideMenu(true));
        closeButton.onClick.AddListener(() => HideMenu(false));
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
