using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //public TriviaManager triviaManager;

    public List<question> responseList; //lista donde guardo la respuesta de la query hecha en la pantalla de selección de categoría

    public int currentTriviaIndex = 0;

    public int randomQuestionIndex = 0;

    public List<string> _answers = new List<string>();

    public bool queryCalled;

    private int _points;

    private int _maxAttempts = 10;

    public int _numQuestionAnswered = 0;

    public string _correctAnswer;

    public GameResult currentGameResult;

    private List<int> _usedQuestions = new List<int>();

    private TimerController _timerController;

    public static GameManager Instance { get; private set; }


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

        _timerController = GameObject.FindWithTag("Timer").GetComponent<TimerController>();
    }

    void Start()
    {

        StartTrivia();

        queryCalled = false;

    }

    void StartTrivia()
    {
        // Cargar la trivia desde la base de datos
        //triviaManager.LoadTrivia(currentTriviaIndex);

        //print(responseList.Count);
        _points = 0;
        _numQuestionAnswered = 0;
        _usedQuestions.Clear();
    }

    public void CategoryAndQuestionQuery(bool isCalled)
    {
        isCalled = UIManagment.Instance.queryCalled;

        if (!isCalled)
        {
            do
            {
                randomQuestionIndex = Random.Range(0, responseList.Count);
            }
            while (_usedQuestions.Contains(randomQuestionIndex));

            _usedQuestions.Add(randomQuestionIndex);
            // Obtén el índice original de la respuesta correcta
            int correctIndex = int.Parse(responseList[randomQuestionIndex].CorrectOption);

            // Limpia respuestas anteriores
            _answers.Clear();

            // Agrega respuestas en orden original
            _answers.Add(responseList[randomQuestionIndex].Answer1);
            _answers.Add(responseList[randomQuestionIndex].Answer2);
            _answers.Add(responseList[randomQuestionIndex].Answer3);

            // Guarda el texto de la respuesta correcta antes de mezclar
            string correctAnswerText = _answers[correctIndex - 1]; // -1 porque los índices de CorrectOption son 1-based

            // Mezcla las respuestas
            _answers.Shuffle();

            // Actualiza el texto de la respuesta correcta después de mezclar
            _correctAnswer = correctAnswerText;

            // Asigna las respuestas mezcladas a los botones
            for (int i = 0; i < UIManagment.Instance._buttons.Length; i++)
            {
                UIManagment.Instance._buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = _answers[i];

                int index = i; // Captura el índice local
                UIManagment.Instance._buttons[i].onClick.RemoveAllListeners(); // Limpia listeners anteriores
                UIManagment.Instance._buttons[i].onClick.AddListener(() => UIManagment.Instance.OnButtonClick(index));
            }
            _timerController.StartTimer();
            UIManagment.Instance.queryCalled = true;
        } 
    }

    public bool AllQuestionsAnswered()
    {
        return _numQuestionAnswered == responseList.Count;
    }
    
    public void EndGame(bool isWin)
    {
        currentGameResult = isWin ? GameResult.Win : GameResult.Lose;
        StartScene("FinishGame");
    }

    public void StartScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void DestroyInstance()
    {
        if (Instance == this)
        {
            Instance = null;
            Destroy(gameObject);
        }
    }

    //Properties
    public int Points
    {
        get => _points;
        set => _points = value;
    }

    //enum
    public enum GameResult
    {
        Win,
        Lose
    }
}

