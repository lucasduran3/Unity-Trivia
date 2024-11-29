using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RankingSceneManager : TriviaSelection
{
    [SerializeField] private Transform _rankingListParent;
    [SerializeField] private GameObject _rankingItemPrefab;

    public static event Action<int> OnSelectCategory;

    protected override void Start()
    {
        base.Start();
        DatabaseManager.OnRankingLoaded += DisplayRanking;
    }
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

    private void DisplayRanking(List<Ranking> rankings)
    {
        foreach (Transform child in _rankingListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var ranking in rankings)
        {
            var rankingItem = Instantiate(_rankingItemPrefab, _rankingListParent);
            rankingItem.transform.Find("PointsText").GetComponent<TextMeshProUGUI>().text = $"Puntos: {ranking.points}";
            rankingItem.transform.Find("CategoryText").GetComponent<TextMeshProUGUI>().text = $"Categoría: {trivias.Find(r => r.id == ranking.trivia_id).category}";
        }
    }

    private void OnDestroy()
    {
        DatabaseManager.OnRankingLoaded -= DisplayRanking;
    }
}
