using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public string SelectedEnvironment { get; set; } // Naam van de geselecteerde omgeving
    public string SelectedEnvironmentId { get; set; } // ID van de geselecteerde omgeving

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Zorgt ervoor dat GameManager niet vernietigd wordt bij het wisselen van scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSelectedEnvironment(string environmentName, string environmentId)
    {
        SelectedEnvironment = environmentName;
        SelectedEnvironmentId = environmentId;
    }
}
