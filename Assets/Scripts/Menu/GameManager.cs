using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public string SelectedEnvironment { get; set; }
    public string SelectedEnvironmentId { get; set; } 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
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
