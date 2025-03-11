using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class EnvironmentUI : MonoBehaviour
{
    public TMP_Text environmentNameText; // UI-tekst voor de naam van de omgeving
    public Button backButton; // UI-knop om terug te gaan naar het SelectMenu

    void Start()
    {
        if (GameManager.instance != null)
        {
            string selectedEnvironment = GameManager.instance.SelectedEnvironment;
            string environmentId = GameManager.instance.SelectedEnvironmentId; // Haal het ID op

            if (!string.IsNullOrEmpty(selectedEnvironment))
            {
                environmentNameText.text = selectedEnvironment; // Zet de naam in de UI
                Debug.Log($"Geselecteerde omgeving: {selectedEnvironment}");
                Debug.Log($"Omgeving ID: {environmentId}"); // Log het ID
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

        // Zorg ervoor dat de knop correct is ingesteld
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
