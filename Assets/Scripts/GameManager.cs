using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Variables
    public List<question> responseList; //lista donde guardo la respuesta de la query hecha en la pantalla de selección de categoría

    public List<string> _answers;

    public int currentTriviaIndex = 0;

    public int randomQuestionIndex = 0;

    public bool queryCalled;

    private int _points;

    private string _currentQuestionText;

    public int _numQuestionAnswered = 0;

    public string _correctAnswer;

    public GameResult currentGameResult;

    private List<int> _usedQuestions = new List<int>();

    private TimerController _timerController;

    #region Events
    public static event Action OnQuestionQueryCalled;
    public static event Action OnExitGame;
    #endregion

    public static GameManager Instance { get; private set; }
    #endregion

    #region Methods
    #region Built in Methods
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        DatabaseManager.OnTriviaDataLoaded += CategoryAndQuestionQuery;
    }

    void Start()
    {

        StartTrivia();

        queryCalled = false;

    }

    void OnDestroy()
    {
        DatabaseManager.OnTriviaDataLoaded -= CategoryAndQuestionQuery;
    }
    #endregion

    #region Custom Methods
    public void StartTrivia()
    {
        _points = 0;
        _numQuestionAnswered = 0;
        _usedQuestions.Clear();
        _answers.Clear();
        currentTriviaIndex = 0;
        randomQuestionIndex = 0;
        queryCalled = false;

        responseList = new List<question>();
    }

    public void CategoryAndQuestionQuery()
    {
        bool isCalled = UIManagment.Instance.queryCalled;

        if (!isCalled)
        {
            do
            {
                randomQuestionIndex = UnityEngine.Random.Range(0, responseList.Count);
            }
            while (_usedQuestions.Contains(randomQuestionIndex));

            if (_usedQuestions.Count >= responseList.Count)
            {
                Debug.LogError("Se usaron todas las preguntas");
                return;
            }

            _usedQuestions.Add(randomQuestionIndex);
            //Obtener indice de la respuesta correcta
            int correctIndex = int.Parse(responseList[randomQuestionIndex].CorrectOption);

            _currentQuestionText = responseList[randomQuestionIndex].QuestionText;

            //Actualizar UI preguntas - UIManagment
            OnQuestionQueryCalled?.Invoke();

            _answers.Clear();

            _answers.Add(responseList[randomQuestionIndex].Answer1);
            _answers.Add(responseList[randomQuestionIndex].Answer2);
            _answers.Add(responseList[randomQuestionIndex].Answer3);

            //Guarda el texto de la respuesta correcta antes de mezclar
            string correctAnswerText = _answers[correctIndex - 1]; // -1 porque los índices de CorrectOption son 1-based

            _answers.Shuffle();

            //Actualiza el texto de la respuesta correcta después de mezclar
            _correctAnswer = correctAnswerText;

            for (int i = 0; i < UIManagment.Instance._buttons.Length; i++)
            {
                UIManagment.Instance._buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = _answers[i];

                int index = i;
                UIManagment.Instance._buttons[i].onClick.RemoveAllListeners();
                UIManagment.Instance._buttons[i].onClick.AddListener(() => UIManagment.Instance.OnButtonClick(index));
            }

            InitTimer();
            UIManagment.Instance.queryCalled = true;
        } 
    }

    private void InitTimer()
    {
        //fix instancia singleton no inicializada
        if (_timerController == null)
        {
            _timerController = GameObject.FindWithTag("Timer").GetComponent<TimerController>();
        }
        _timerController.StartTimer();
    }

    public bool AllQuestionsAnswered()
    {
        return _numQuestionAnswered == responseList.Count;
    }

    public void CalculatePoints()
    {
        TimerController timerInstance = TimerController.Instance;
        _points += 10 - timerInstance.TimeToRespond;
    }

    public void EndGame(GameResult result)
    {
        currentGameResult = result;
        StartScene("FinishGame");
    }

    public void StartScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        //Guardar tiempo de uso y cerrar aplicación - DbManager
        OnExitGame?.Invoke();
    }

    public void DestroyInstance()
    {
        if (Instance == this)
        {
            Instance = null;
            Destroy(gameObject);
        }
    }
    #endregion
    #endregion

    #region Properties
    public int Points => _points;
    public string CurrentQuestionText => _currentQuestionText;
    #endregion
}

