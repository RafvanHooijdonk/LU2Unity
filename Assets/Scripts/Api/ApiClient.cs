using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class ApiClient : MonoBehaviour
{
    public TMP_InputField emailInputField;  
    public TMP_InputField passwordInputField;
    public TMP_Text Output; 

    void Start()
    {
        HideFeedbackMessage();
        DontDestroyOnLoad(gameObject);
    }

    public async void Register()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        var request = new PostRegisterRequestDto
        {
            email = email,
            password = password
        };

        var jsonData = JsonUtility.ToJson(request);
        Debug.Log("Register JSON: " + jsonData);
        string response = await PerformApiCall("https://avansict2230382.azurewebsites.net/account/register", "POST", jsonData);
    }

    public async void Login()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        var request = new PostLoginRequestDto
        {
            email = email,
            password = password
        };

        var jsonData = JsonUtility.ToJson(request);
        Debug.Log("Login JSON: " + jsonData);

        string response = await PerformApiCall("https://avansict2230382.azurewebsites.net/account/login", "POST", jsonData);

        if (!string.IsNullOrEmpty(response))
        {
            var loginResponse = JsonUtility.FromJson<PostLoginResponseDto>(response);
            AuthManager.instance.SetAccessToken(loginResponse.accessToken); 
            ShowFeedbackMessage("Login succesvol!", Color.green);
            StartCoroutine(WaitAndLoadScene("SelectMenu"));
        }
        else
        {
            ShowFeedbackMessage("Login mislukt. Controleer je gegevens.", Color.red); 
        }
    }

    private async Task<string> PerformApiCall(string url, string method, string jsonData = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, method))
        {
            if (!string.IsNullOrEmpty(jsonData))
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            string accessToken = AuthManager.instance.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            }
            
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("API-aanroep is succesvol: " + request.downloadHandler.text);
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError("Fout bij API-aanroep: " + request.error);
                Debug.LogError("Response body: " + request.downloadHandler.text);
                return null;
            }
        }
    }
    private void ShowFeedbackMessage(string message, Color messageColor)
    {
        if (Output != null)
        {
            Output.text = message;
            Output.color = messageColor;
            Output.gameObject.SetActive(true);

            StartCoroutine(HideFeedbackMessageAfterDelay(5f));
        }
    }
    private IEnumerator HideFeedbackMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Output != null)
        {
            HideFeedbackMessage();
        }
    }
    private void HideFeedbackMessage()
    {
        if (Output != null)
        {
            Output.gameObject.SetActive(false);
        }
    }
    private IEnumerator WaitAndLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(1f); 
        SceneManager.LoadScene(sceneName);   
    }
}
