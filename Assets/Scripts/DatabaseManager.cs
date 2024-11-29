using UnityEngine;
using Supabase;
using Supabase.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Postgrest.Models;
using System.Linq;
using Unity.VisualScripting;
using Postgrest.Responses;
using System;
using Newtonsoft.Json;
using Postgrest.Exceptions;

public class DatabaseManager : MonoBehaviour
{
    string supabaseUrl = "https://iwdrxfxeosvebxwayxfh.supabase.co"; //COMPLETAR
    string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Iml3ZHJ4Znhlb3N2ZWJ4d2F5eGZoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzIyMjE1MDEsImV4cCI6MjA0Nzc5NzUwMX0.l_3eGWD_kVqEd8E64f9kArIEsedViSiw5Q1hc6NpKZ4"; //COMPLETAR

    Supabase.Client clientSupabase;

    public static event Action<List<Ranking>> OnRankingLoaded;

    public int index;

    async void Start()
    {
        clientSupabase = new Supabase.Client(supabaseUrl, supabaseKey);
        
        index = PlayerPrefs.GetInt("SelectedIndex");

        GameManager.OnGameEnd += HandleGameEnd;
        RankingSceneManager.OnSelectCategory += HandleSelectCategory;

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

    private async Task LoadGeneralRanking()
    {
        var rankingResponse = await clientSupabase
            .From<Ranking>()
            .Select("*")
            .Get();

        var triviasResponse = TriviaSelection.trivias;

        if (triviasResponse != null && rankingResponse != null)
        {
            //Combinar los datos
            var generalRanking = (from t in triviasResponse
                                  join r in rankingResponse.Models
                                  on t.id equals r.trivia_id
                                  group r by new { t.id, t.category } into g
                                  select new
                                  {
                                      trivia_id = g.Key.id,
                                      category = g.Key.category,
                                      points = g.Max(r => r.points) //Mayor puntaje por trivia
                                  })
                                  .OrderByDescending(r => r.points)
                                  .ToList();

            //  Convertir a Ranking
            var rankings = generalRanking.Select(gr => new Ranking
            {
                trivia_id = gr.trivia_id,
                points = gr.points,
                category = gr.category
            }).ToList();

            OnRankingLoaded?.Invoke(rankings);
        }
    }

    private async Task LoadCategoryRanking(int triviaId)
    {
        var response = await clientSupabase
            .From<Ranking>()
            .Where(r => r.trivia_id == triviaId)
            .Order("points", Postgrest.Constants.Ordering.Descending, Postgrest.Constants.NullPosition.First)
            .Select("*")
            .Get();

        if (response != null)
        {
            foreach (var item in response.Models)
            {
                Debug.Log("Id de trivia en category " + item.category);
            }

            OnRankingLoaded?.Invoke(response.Models);
        }
    }

    private async void HandleGameEnd(int points, int triviaId)
    {       
        var newRanking = new Ranking { points = points, trivia_id = triviaId };

        var response = await clientSupabase.From<Ranking>().Insert(newRanking);

        if (response != null)
        {
            Debug.Log("Puntaje guardado");
        }
        else
        {
            Debug.LogError("Error al guardar el puntaje");
        }
    }

    private async void HandleSelectCategory(int trivia_id)
    {
        if (trivia_id == 0)
        {
           await LoadGeneralRanking();
        }
        else
        {
            var selectedTrivia = TriviaSelection.trivias[trivia_id - 1];
            await LoadCategoryRanking(selectedTrivia.id);
        }
    }

    private void OnDestroy()
    {
        GameManager.OnGameEnd -= HandleGameEnd;
        RankingSceneManager.OnSelectCategory -= HandleSelectCategory;
    }
}
