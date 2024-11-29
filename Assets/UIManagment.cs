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
    [SerializeField] TextMeshProUGUI _timerText;
    [SerializeField] TextMeshProUGUI _pointsText;
    
    string _correctAnswer;

    public Button[] _buttons = new Button[3];

    [SerializeField] Button _backButton;

    private List<string> _answers = new List<string>();

    public bool queryCalled;

    private Color _originalButtonColor;

    private bool _isCorrectSelection;

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
        UpdateUI();
    }

    void Update()
    {
        _categoryText.text = PlayerPrefs.GetString("SelectedTrivia");
        _questionText.text = GameManager.Instance.responseList[GameManager.Instance.randomQuestionIndex].QuestionText;
        GameManager.Instance.CategoryAndQuestionQuery(queryCalled);


    }

    public void UpdateUI()
    {
        /*_categoryText.text = PlayerPrefs.GetString("SelectedTrivia");
        _questionText.text = GameManager.Instance.responseList[GameManager.Instance.randomQuestionIndex].QuestionText;*/
        //GameManager.Instance.CategoryAndQuestionQuery(queryCalled);
    }
    public void OnButtonClick(int buttonIndex)
    {
        TimerController.Instance.StopTimer();

        string selectedAnswer = _buttons[buttonIndex].GetComponentInChildren<TextMeshProUGUI>().text;

        _correctAnswer = GameManager.Instance.responseList[GameManager.Instance.randomQuestionIndex].CorrectOption;

        if (selectedAnswer == GameManager.Instance._correctAnswer)
        {
            _isCorrectSelection = true;
            DisableButtons();
            GameManager.Instance.CalculatePoints();
            UpdatePointsText();
            //setear UI puntos

            ChangeButtonColor(buttonIndex, Color.green);
            Invoke("RestoreButtonColor", 2f);
            GameManager.Instance._answers.Clear();
            Invoke("NextAnswer", 2f);
            Invoke("ExecuteEndGame", 1f);
        }
        else
        {
            _isCorrectSelection = false;
            _buttons[buttonIndex].interactable = false;
            Debug.Log("Respuesta incorrecta. Inténtalo de nuevo.");
            ChangeButtonColor(buttonIndex, Color.red);
            Invoke("ExecuteEndGame", 1f);
        }

        GameManager.Instance._numQuestionAnswered++;
        Debug.Log(GameManager.Instance.Points);
    }

    public void UpdateTimerText(int second)
    {
        _timerText.text = $"Timer: {second}";
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

    private void EnableButtons()
    {
        foreach (var button in _buttons)
        {
            button.interactable = true;
        }
    }

    private void DisableButtons()
    {
        foreach (var button in _buttons)
        {
            button.interactable = false;
        }
    }

    private void NextAnswer()
    {
        queryCalled = false;
        UpdateUI();
        EnableButtons();
    }

    public void PreviousScene()
    {
        Destroy(GameManager.Instance);
        Destroy(UIManagment.Instance);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void DestroyInstance()
    {
        if (Instance == this)
        {
            Instance = null; // Limpia la referencia estática
            Destroy(gameObject); // Destruye el objeto del juego
        }
    }

    private void UpdatePointsText()
    {
        _pointsText.text = $"Puntos: {GameManager.Instance.Points}";
    }

    private void ExecuteEndGame()
    {
        if (_isCorrectSelection && GameManager.Instance.AllQuestionsAnswered())
        {
            GameManager.Instance.EndGame(GameResult.WIN);
        } 
        else if (!_isCorrectSelection)
        {
            GameManager.Instance.EndGame(GameResult.LOSE_BY_ANSWER);
        }

        return;
    }
}
