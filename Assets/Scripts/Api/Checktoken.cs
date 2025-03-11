using UnityEngine;
using UnityEngine.SceneManagement;

public class TokenButtonHandler : MonoBehaviour
{
    void Awake()
    {
        // Zorg ervoor dat dit object niet wordt vernietigd bij het laden van een nieuwe scène
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        // Zorg ervoor dat de methode wordt aangeroepen zodra de scène is geladen
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Verwijder de listener als het object wordt uitgeschakeld
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Deze methode wordt aangeroepen zodra een nieuwe scène is geladen
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnCheckTokenButtonClicked();
    }

    // Methode die wordt aangeroepen wanneer de scène is geladen
    private void OnCheckTokenButtonClicked()
    {
        if (AuthManager.instance != null && !string.IsNullOrEmpty(AuthManager.instance.AccessToken))
        {
            Debug.Log("Access Token in deze Scene: " + AuthManager.instance.AccessToken); // Log het token als het aanwezig is
        }
        else
        {
            Debug.Log("GEEN toegangstoken beschikbaar in deze scene.");
        }
    }
}
