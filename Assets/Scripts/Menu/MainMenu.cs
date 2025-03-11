using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject registerButton;    
    public GameObject loginButton;     
    public GameObject emailInput;     
    public GameObject passwordInput;   
    public GameObject confirmRegisterButton; 
    public GameObject confirmLoginButton;
    public GameObject backButton;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowRegister()
    {
        registerButton.SetActive(false);
        loginButton.SetActive(false);

        emailInput.SetActive(true);
        passwordInput.SetActive(true);
        confirmRegisterButton.SetActive(true);
        backButton.SetActive(true);
    }
    public void ShowLogin()
    {
        registerButton.SetActive(false);
        loginButton.SetActive(false);

        emailInput.SetActive(true);
        passwordInput.SetActive(true);
        confirmLoginButton.SetActive(true);
        backButton.SetActive(true);
    }
    public void ShowMainMenu()
    {
        registerButton.SetActive(true);
        loginButton.SetActive(true);

        emailInput.SetActive(false);
        passwordInput.SetActive(false);
        confirmRegisterButton.SetActive(false);
        confirmLoginButton.SetActive(false);
        backButton.SetActive(false);
    }
}
