using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager instance { get; private set; }
    public string AccessToken { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    public void SetAccessToken(string token)
    {
        AccessToken = token;
        Debug.Log("Access token opgeslagen in AuthManager: " + AccessToken);
    }
}
