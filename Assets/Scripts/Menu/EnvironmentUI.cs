using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class EnvironmentUI : MonoBehaviour
{
    public TMP_Text environmentNameText; 
    public Button backButton; 

    void Start()
    {
        if (GameManager.instance != null)
        {
            string selectedEnvironment = GameManager.instance.SelectedEnvironment;
            string environmentId = GameManager.instance.SelectedEnvironmentId; 

            if (!string.IsNullOrEmpty(selectedEnvironment))
            {
                environmentNameText.text = selectedEnvironment; 
                Debug.Log($"Geselecteerde omgeving: {selectedEnvironment}");
                Debug.Log($"Omgeving ID: {environmentId}"); 
            }
            else
            {
                environmentNameText.text = "Geen omgeving geselecteerd";
                Debug.Log("Geen omgeving ID gevonden.");
            }
        }
        else
        {
            environmentNameText.text = "GameManager niet gevonden!";
            Debug.LogError("GameManager instance is niet beschikbaar!");
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBackToSelectMenu);
        }
        else
        {
            Debug.LogError("BackButton is niet gekoppeld in de Inspector!");
        }
    }

    void GoBackToSelectMenu()
    {
        SceneManager.LoadScene("SelectMenu");
    }
}
