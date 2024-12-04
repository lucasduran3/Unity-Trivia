using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginSceneManager : MonoBehaviour
{
    [SerializeField] private DatabaseManager _dbManager;
    [SerializeField] private TMP_InputField _emailInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private TextMeshProUGUI _loginButtonText;
    [SerializeField] private TextMeshProUGUI _errorText;

    [SerializeField] private SupabaseClientData _client;
    //[SerializeField] private TMP_InputField _usernameInput;

    private void Awake()
    {
        _client.ResetClient();
        DatabaseManager.OnAuthError += DisplayErrorMessage;
    }
    private void Start()
    {
        PopulateDropDown();
        _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        _dropdown.value = 0;
    }

    public void OnLoginButtonPress()
    {
        if (_dropdown.value == 0)
        {
            //Logearse
            _dbManager.SignInWithEmail(_emailInput.text, _passwordInput.text);
        }
        else
        {
            _dbManager.SignUpWithEmail(_usernameInput.text, _emailInput.text, _passwordInput.text);
        }
    }

    private void OnDropdownValueChanged(int i)
    {
        if (i == 0)
        {
            _loginButtonText.text = "Iniciar Sesión";
            _usernameInput.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            _loginButtonText.text = "Registrarse";
            _usernameInput.transform.parent.gameObject.SetActive(true);
        }
    }

    private void PopulateDropDown()
    {
        _dropdown.options.Clear();
        List<string> options = new List<string>() { "Iniciar Sesión", "Registrarse"};
        _dropdown.AddOptions(options);
    }

    private void DisplayErrorMessage(string message)
    {
        if (message.Contains("400"))
        {
            _errorText.text = "Email o contraseña incorrecta.";
        }
        if (message.Contains("User already registered"))
        {
            _errorText.text = "Este usuario ya está registrado. Selecciona Iniciar Sesión";
        }
    }

    private void OnDestroy()
    {
        DatabaseManager.OnAuthError -= DisplayErrorMessage;
    }
}
