using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManagment : MonoBehaviour
{

    
    [SerializeField] TextMeshProUGUI _categoryText;
    [SerializeField] TextMeshProUGUI _questionText;
    
    string _correctAnswer;

    public Button[] _buttons = new Button[3];

    [SerializeField] Button _backButton;

    private List<string> _answers = new List<string>();

    public bool queryCalled;

    private Color _originalButtonColor;

    public static UIManagment Instance { get; private set; }


    void Awake()
    {
        // Configura la instancia
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Para mantener el objeto entre escenas
        }
        else
        {
            Destroy(gameObject);
        }

    }


    private void Start()
    {
        queryCalled = false;

        _originalButtonColor = _buttons[0].GetComponent<Image>().color;

    }

    void Update()
    {
        _categoryText.text = PlayerPrefs.GetString("SelectedTrivia");
        _questionText.text = GameManager.Instance.responseList[GameManager.Instance.randomQuestionIndex].QuestionText;

        GameManager.Instance.CategoryAndQuestionQuery(queryCalled);

    }
    public void OnButtonClick(int buttonIndex)
    {
        
        string selectedAnswer = _buttons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>().text;

        _correctAnswer = GameManager.Instance.responseList[GameManager.Instance.randomQuestionIndex].CorrectOption;

        if (selectedAnswer == _correctAnswer)
        {
            Debug.Log("¡Respuesta correcta!");
            ChangeButtonColor(buttonIndex, Color.green);
            Invoke("RestoreButtonColor", 2f);
            GameManager.Instance._answers.Clear();
            Invoke("NextAnswer", 2f);
            
        }
        else
        {
            Debug.Log("Respuesta incorrecta. Inténtalo de nuevo.");
            
            ChangeButtonColor(buttonIndex, Color.red);
            Invoke("RestoreButtonColor", 2f);
        }


    }

    private void ChangeButtonColor(int buttonIndex, Color color)
    {
        Image buttonImage = _buttons[buttonIndex].GetComponent<Image>();
        buttonImage.color = color;
    }

    private void RestoreButtonColor()
    {
        foreach (Button button in _buttons)
        {
            Image buttonImage = button.GetComponent<Image>();
            buttonImage.color = _originalButtonColor;
        }
    }

    private void NextAnswer()
    {
        queryCalled = false;
    }

    public void PreviousScene()
    {
        Destroy(GameManager.Instance);
        Destroy(UIManagment.Instance);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }


}
