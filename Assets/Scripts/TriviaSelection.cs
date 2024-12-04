using UnityEngine;
using Supabase;
using Supabase.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Postgrest.Models;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TriviaSelection : MonoBehaviour
{
    public static List<trivia> trivias = new List<trivia>();
    [SerializeField] protected TMP_Dropdown _dropdown;
    [SerializeField] private SupabaseClientData _supabaseClientData;

    protected virtual async void Start()
    {
        await SelectTrivias();
        PopulateDropdown();
    }

    protected virtual async Task SelectTrivias()
    {
        var response = await _supabaseClientData.Client
            .From<trivia>()
            .Select("*")
            .Get();

        if (response != null)
        {
            trivias = response.Models;
        }
    }

    protected virtual void PopulateDropdown()
    {   
        _dropdown.ClearOptions();

        List<string> categories = new List<string>();

        foreach (var trivia in trivias)
        {
            categories.Add(trivia.category);
        }

        _dropdown.AddOptions(categories);
    }

    public async void OnStartButtonClicked()
    {
        int selectedIndex = _dropdown.value;
        string selectedTrivia = _dropdown.options[selectedIndex].text;

        PlayerPrefs.SetInt("SelectedIndex", selectedIndex+1);
        PlayerPrefs.SetString("SelectedTrivia", selectedTrivia);

        SceneManager.LoadScene("Main");
    }

}
