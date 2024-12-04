using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RankingSceneManager : TriviaSelection
{
    #region Variables
    [SerializeField] private TextMeshProUGUI m_userRankingTxt;
    [SerializeField] private Transform _rankingListParent;
    [SerializeField] private GameObject _rankingItemPrefab;

    #region Events
    public static event Action<int> OnSelectCategory;
    public static event Action OnRankingSceneStart;
    #endregion
    #endregion

    #region Methods
    #region Build in Methods
    protected override void Start()
    {
        base.Start();
        DatabaseManager.OnRankingLoaded += DisplayRanking;
        DatabaseManager.OnUserRankingLoaded += DisplayUserRanking;

        OnRankingSceneStart?.Invoke();
    }

    private void OnDestroy()
    {
        DatabaseManager.OnRankingLoaded -= DisplayRanking;
        DatabaseManager.OnUserRankingLoaded -= DisplayUserRanking;
    }
    #endregion

    #region Custom Methods
    protected override void PopulateDropdown()
    {
        _dropdown.ClearOptions();

        List<string> categories = new List<string>() { "General" };

        foreach (var trivia in trivias)
        {
            categories.Add(trivia.category);
        }

        _dropdown.AddOptions(categories);
        _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        _dropdown.value = 0;
        OnDropdownValueChanged(0);
    }

    private void OnDropdownValueChanged(int selectedIndex)
    {
        OnSelectCategory?.Invoke(selectedIndex);
    }

    public void DisplayRanking(List<Ranking> rankings)
    {
        for (int i = 1; i < _rankingListParent.childCount; i++)
        {
            Destroy(_rankingListParent.GetChild(i).gameObject);
        }

        foreach (var ranking in rankings)
        {
            var rankingItem = Instantiate(_rankingItemPrefab, _rankingListParent);
            rankingItem.transform.Find("PointsText").GetComponent<TextMeshProUGUI>().text = $"{ranking.points}";
            //rankingItem.transform.Find("CategoryText").GetComponent<TextMeshProUGUI>().text = $"{trivias.Find(r => r.id == ranking.trivia_id).category}";
            rankingItem.transform.Find("CategoryText").GetComponent<TextMeshProUGUI>().text = $"{ranking.category}";
        }
    }

    private void DisplayUserRanking(List<Ranking> userRanking)
    {
        m_userRankingTxt.text = "";
        foreach (var ranking in userRanking)
        {
            m_userRankingTxt.text += $"\n{ranking.category}: {ranking.points}";
        }
    }
    #endregion
    #endregion
}
