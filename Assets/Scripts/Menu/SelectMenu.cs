using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class IslandButtonManager : MonoBehaviour
{
    public Button buttonIsland1, buttonIsland2, buttonIsland3, buttonIsland4, buttonIsland5;
    public Button deleteButtonIsland1, deleteButtonIsland2, deleteButtonIsland3, deleteButtonIsland4, deleteButtonIsland5;
    public Button logoutButton; 

    public GameObject popupPanel;
    public TMP_Text popupText;
    public TMP_InputField nameInputField;
    public Button confirmButton;

    public GameObject outputPanel; 
    public TMP_Text outputText;     


    private string selectedIsland = "";
    private Button selectedButton;

    private Dictionary<string, string> worldButtonMap = new Dictionary<string, string>();
    private List<string> existingWorldNames = new List<string>();
    private List<Button> islandButtons;
    private List<Button> deleteButtons;

    void Start()
    {
        islandButtons = new List<Button> { buttonIsland1, buttonIsland2, buttonIsland3, buttonIsland4, buttonIsland5 };
        deleteButtons = new List<Button> { deleteButtonIsland1, deleteButtonIsland2, deleteButtonIsland3, deleteButtonIsland4, deleteButtonIsland5 };

        foreach (Button button in islandButtons)
        {
            button.onClick.AddListener(() => OnIslandButtonClick(button));
        }
        for (int i = 0; i < deleteButtons.Count; i++)
        {
            int index = i; 
            deleteButtons[i].onClick.AddListener(() => OnDeleteButtonClick(index));
        }

        confirmButton.onClick.AddListener(OnConfirmButtonClick);
        popupPanel.SetActive(false);
        StartCoroutine(LoadExistingEnvironments());

        logoutButton.onClick.AddListener(OnLogoutButtonClick);
    }

    private void OnIslandButtonClick(Button islandButton)
    {
        string buttonName = islandButton.GetComponentInChildren<TMP_Text>().text;

        if (existingWorldNames.Contains(buttonName))
        {
            SceneManager.LoadScene("Environment");
            return;
        }

        selectedButton = islandButton;
        selectedIsland = islandButton.name;

        popupText.text = $"Je hebt {selectedIsland} geselecteerd. Voer de naam in voor de nieuwe 2D-wereld.";
        popupText.color = Color.white;
        popupPanel.SetActive(true);
    }

    private void OnConfirmButtonClick()
    {
        string worldName = nameInputField.text;

        if (string.IsNullOrEmpty(worldName) || worldName.Length < 1 || worldName.Length > 25)
        {
            popupText.color = Color.red;
            popupText.text = $"Fout! De naam moet tussen de 1 en 25 tekens lang zijn.\nJe hebt {selectedIsland} geselecteerd. Voer de naam in voor de nieuwe 2D-wereld.";
            return;
        }

        if (existingWorldNames.Contains(worldName))
        {
            popupText.color = Color.red;
            popupText.text = $"Fout! Er bestaat al een wereld met de naam: {worldName}. Kies een andere naam.";
            return;
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.SetSelectedEnvironment(worldName, "TEMP_ID");
        }

        StartCoroutine(CreateEnvironment(worldName));
        popupPanel.SetActive(false);
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    private IEnumerator CreateEnvironment(string worldName)
    {
        var requestData = new PostCreateWorldRequestDto { Name = worldName, Maxheight = 120, MaxLength = 120 };
        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest("https://avansict2230382.azurewebsites.net/api/Environment2D/CreateEnvironment", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + AuthManager.instance.AccessToken);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Fout bij het verzenden van de request: " + request.error);
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                Environment2D createdEnvironment = JsonUtility.FromJson<Environment2D>(responseJson);
                GameManager.instance.SetSelectedEnvironment(worldName, createdEnvironment.id);
                existingWorldNames.Add(worldName);
                worldButtonMap[selectedButton.name] = worldName;
                selectedButton.GetComponentInChildren<TMP_Text>().text = worldName;
                selectedButton.onClick.RemoveAllListeners();
                selectedButton.onClick.AddListener(() => SceneManager.LoadScene("Environment"));

                UpdateDeleteButtons();
            }
        }
    }

    private IEnumerator LoadExistingEnvironments()
    {
        using (UnityWebRequest request = UnityWebRequest.Get("https://avansict2230382.azurewebsites.net/api/Environment2D/GetEnvironments"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + AuthManager.instance.AccessToken);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Fout bij het ophalen van bestaande werelden: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Environment2D[] environments = JsonHelper.FromJson<Environment2D>(jsonResponse);

                for (int i = 0; i < environments.Length && i < islandButtons.Count; i++)
                {
                    string environmentName = environments[i].name;
                    string environmentId = environments[i].id;
                    existingWorldNames.Add(environmentName);

                    TMP_Text buttonText = islandButtons[i].GetComponentInChildren<TMP_Text>();
                    buttonText.text = environmentName;

                    Button islandButton = islandButtons[i];
                    islandButton.onClick.RemoveAllListeners();
                    islandButton.onClick.AddListener(() =>
                    {
                        GameManager.instance.SetSelectedEnvironment(environmentName, environmentId);
                        SceneManager.LoadScene("Environment");
                    });
                }
                UpdateDeleteButtons();
            }
        }
    }

    private void OnDeleteButtonClick(int index)
    {
        string worldName = islandButtons[index].GetComponentInChildren<TMP_Text>().text;
        StartCoroutine(DeleteEnvironment(worldName, index));
    }

    private IEnumerator DeleteEnvironment(string worldName, int index)
    {
        var requestData = new { name = worldName };

        string jsonData = JsonConvert.SerializeObject(requestData);

        using (UnityWebRequest request = new UnityWebRequest("https://avansict2230382.azurewebsites.net/api/Environment2D/DeleteEnvironment", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + AuthManager.instance.AccessToken);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Fout bij het verzenden van de request: " + request.error);
                Debug.LogError("Response text: " + request.downloadHandler.text); 
            }
            else
            {
                existingWorldNames.Remove(worldName);
                islandButtons[index].GetComponentInChildren<TMP_Text>().text = "NIEUWE WERELD";
                islandButtons[index].onClick.RemoveAllListeners();
                islandButtons[index].onClick.AddListener(() => OnIslandButtonClick(islandButtons[index]));
                worldButtonMap.Remove(islandButtons[index].name);

                UpdateDeleteButtons();

                outputPanel.SetActive(true);

                outputText.color = Color.green;
                outputText.text = $"\"Wereld '{worldName}' en de bijbehorende objecten zijn succesvol verwijderd.";

                Debug.Log($"Wereld '{worldName}' en objecten verwijderd uit database.");

                yield return new WaitForSeconds(5f);

                outputPanel.SetActive(false);
            }
        }
    }

    private void UpdateDeleteButtons()
    {
        for (int i = 0; i < islandButtons.Count; i++)
        {
            string buttonText = islandButtons[i].GetComponentInChildren<TMP_Text>().text;
            deleteButtons[i].gameObject.SetActive(existingWorldNames.Contains(buttonText));
        }
    }
    private void OnLogoutButtonClick()
    {
        SceneManager.LoadScene("MainMenu");
    }
}


public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = $"{{\"items\": {json} }}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}

[System.Serializable]
public class PostCreateWorldRequestDto
{
    public string Name;
    public int Maxheight;
    public int MaxLength;
}

[System.Serializable]
public class Environment2D
{
    public string id;
    public string name;
    public string ownerUserId;
    public int minLength;
    public int maxLength;
}

[System.Serializable]
public class Island
{
    public string Name;
}

[System.Serializable]
public class IslandList
{
    public List<Island> islands;
}
