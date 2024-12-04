using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishGameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private TextMeshProUGUI _pointsText;
    [SerializeField] private TextMeshProUGUI _positionText;
    [SerializeField] private Transform _rankingItemParent;
    [SerializeField] private GameObject _rankingItemPrefab;
    [SerializeField] private TMP_Dropdown _categoryDropdown;

    private UserRankingData _rankingData;
    public static event Action<int, int> OnGameEnd;

    private void Start()
    {
        DatabaseManager.OnUserRankingDataLoaded += DisplayUserRanking;
        UIManagment.Instance?.DestroyInstance();
        TimerController.Instance?.DestroyInstance();

        var result = GameManager.Instance.currentGameResult;

        switch (result)
        {
            case GameResult.WIN:
                _resultText.text = "¡Ganaste!";
                break;

            case GameResult.LOSE_BY_ANSWER:
                _resultText.text = "¡Respuesta Incorrecta!";
                break;

            case GameResult.LOSE_BY_TIMER:
                _resultText.text = "¡Se termino el tiempo!";
                break;
        }

        _pointsText.text = $"Puntuación: {GameManager.Instance.Points}";
        PopulateDropDown();

        _categoryDropdown.onValueChanged.AddListener(DropdownValueChanged);

        OnGameEnd?.Invoke(GameManager.Instance.Points, GameManager.Instance.currentTriviaIndex);
    }

    private void PopulateDropDown()
    {
        _categoryDropdown.ClearOptions();
        _categoryDropdown.options.Add(new TMP_Dropdown.OptionData("General"));
        _categoryDropdown.options.Add(new TMP_Dropdown.OptionData($"{PlayerPrefs.GetString("SelectedTrivia")}"));
    }

    public void DropdownValueChanged(int x)
    {
        DisplayTopScores();
        DisplayPositionText();
    }

    private void DisplayUserRanking(UserRankingData data)
    {
        _rankingData = data;
        DisplayPositionText();
        DisplayTopScores();
    }

    private void DisplayPositionText()
    {
        if (_categoryDropdown.value == 0)
        {
            _positionText.text = $"Posición en el ranking: {_rankingData.GeneralPosition}°";
        }
        else
        {
            _positionText.text = $"Posición en el ranking {_rankingData.TriviaPosition}°";
        }
    }

    private void DisplayTopScores()
    {
        List<Ranking> rankingList;

        if (_categoryDropdown.value == 0)
        {
            rankingList = _rankingData.GeneralTopScores;
        }
        else
        {
            rankingList = _rankingData.TriviaTopScores;
        }

        for (int i = 2; i < _rankingItemParent.childCount; i++)
        {
            Destroy(_rankingItemParent.GetChild(i).gameObject);
        }

        foreach (var item in rankingList)
        {
            var rankingItem = Instantiate(_rankingItemPrefab, _rankingItemParent);
            rankingItem.transform.Find("PointsText").GetComponent<TextMeshProUGUI>().text = $"{item.points}";
            rankingItem.transform.Find("CategoryText").GetComponent<TextMeshProUGUI>().text = $"{item.category}";
        }
    }

    private void OnDestroy()
    {
        DatabaseManager.OnUserRankingDataLoaded -= DisplayUserRanking;
    }
}
