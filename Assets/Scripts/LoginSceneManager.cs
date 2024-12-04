using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginSceneManager : MonoBehaviour
{
    #region Variables
    [SerializeField] private DatabaseManager _dbManager;
    [SerializeField] private TMP_InputField _emailInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private TextMeshProUGUI _loginButtonText;
    [SerializeField] private TextMeshProUGUI _errorText;

    [SerializeField] private SupabaseClientData _client;
    #endregion

    #region Methods
    #region Built in Methods
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

    private void OnDestroy()
    {
        DatabaseManager.OnAuthError -= DisplayErrorMessage;
    }
    #endregion
    #region Custom Methods
    public void OnLoginButtonPress()
    {
        if (_dropdown.value == 0)
        {
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
        if (message.Contains("validation_failed"))
        {
            _errorText.text = "Formato de email invalido.";
        }
        if (message.Contains("weak_password"))
        {
            _errorText.text = "La contraseña debe tener almenos 6 carácteres.";
        }
    }
    #endregion
    #endregion
}
