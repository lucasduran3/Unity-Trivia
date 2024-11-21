using UnityEngine;
using Supabase;
using Supabase.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Postgrest.Models;

public class DatabaseManager : MonoBehaviour
{
    string supabaseUrl = "url"; //COMPLETAR
    string supabaseKey = "key"; //COMPLETAR

    Supabase.Client clientSupabase;

    public int index;

    //UI
    [SerializeField] private 


    async void Start()
    {
        clientSupabase = new Supabase.Client(supabaseUrl, supabaseKey);
        
        index = PlayerPrefs.GetInt("SelectedIndex");

        //print(_selectedTrivia);

        await LoadTriviaData(index);
    }

    async Task LoadTriviaData(int index)
    {
        var response = await clientSupabase
            .From<question>()
            .Where(question => question.trivia_id == index)
            .Select("id, question, answer1, answer2, answer3, correct_answer, trivia_id, trivia(id, category)")
            .Get();

        GameManager.Instance.currentTriviaIndex = index;

        GameManager.Instance.responseList = response.Models;

        print("Response from query: "+ response.Models.Count);
        print("ResponseList from GM: "+ GameManager.Instance.responseList.Count);

    


    }

}
