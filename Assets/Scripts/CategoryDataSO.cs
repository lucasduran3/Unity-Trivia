using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CategoryData", menuName = "ScriptableObjects/Data/CategoryData")]
public class CategoryDataSO : ScriptableObject
{
    private List<string> _categories = new List<string>();

    public List<string> Categories
    {
        get => _categories;
        set => _categories = value;
    }

    public void ResetData()
    {
        _categories.Clear();
    }
}
