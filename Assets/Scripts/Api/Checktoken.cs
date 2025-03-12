using UnityEngine;
using UnityEngine.SceneManagement;

public class TokenButtonHandler : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnCheckTokenButtonClicked();
    }

    private void OnCheckTokenButtonClicked()
    {
        if (AuthManager.instance != null && !string.IsNullOrEmpty(AuthManager.instance.AccessToken))
        {
            Debug.Log("Access Token in deze Scene: " + AuthManager.instance.AccessToken);
        }
        else
        {
            Debug.Log("GEEN toegangstoken beschikbaar in deze scene.");
        }
    }
}
