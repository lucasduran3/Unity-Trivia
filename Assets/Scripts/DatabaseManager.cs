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
    public static event Action<bool> OnTriviaDataLoaded;
    public static event Action<UserRankingData> OnUserRankingDataLoaded;

    public int index;

    private void Awake()
    {
        FinishGameManager.OnGameEnd += HandleEndGame;
    }
    async void Start()
    {
        clientSupabase = new Supabase.Client(supabaseUrl, supabaseKey);

        index = PlayerPrefs.GetInt("SelectedIndex");
        GameManager.OnExitGame += HandleExitGame;
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

        if (response != null)
        {
            try { OnTriviaDataLoaded?.Invoke(UIManagment.Instance.queryCalled); }
            catch (Exception e)
            {
                return;
            }
        }
    }

private async Task<List<Ranking>> LoadGeneralRanking()
{
    var rankingResponse = await clientSupabase
        .From<Ranking>()
        .Select("*")
        .Get();

    var triviasResponse = TriviaSelection.trivias;

    if (triviasResponse != null && rankingResponse != null)
    {
        // Combinar los datos
        var generalRanking = (from t in triviasResponse
                              join r in rankingResponse.Models
                              on t.id equals r.trivia_id
                              group r by new { t.id, t.category } into g
                              select new
                              {
                                  trivia_id = g.Key.id,
                                  category = g.Key.category,
                                  points = g.Max(r => r.points) // Mayor puntaje por trivia
                              })
                              .OrderByDescending(r => r.points)
                              .ToList();

        // Convertir a Ranking y eliminar duplicados por puntos
        var rankings = generalRanking
            .Select(gr => new Ranking
            {
                trivia_id = gr.trivia_id,
                points = gr.points,
                category = gr.category
            })
            .GroupBy(r => r.points) // Agrupar por puntos
            .Select(g => g.First()) // Seleccionar el primer elemento único
            .OrderByDescending(r => r.points) // Ordenar nuevamente por puntos
            .ToList();

        OnRankingLoaded?.Invoke(rankings);

        return rankings;
    }
    return null;
}

    private async Task<List<Ranking>> LoadCategoryRanking(int triviaId)
    {
        var response = await clientSupabase
            .From<Ranking>()
            .Where(r => r.trivia_id == triviaId)
            .Order("points", Postgrest.Constants.Ordering.Descending, Postgrest.Constants.NullPosition.First)
            .Select("*")
            .Get();

        if (response != null)
        {
            // Eliminar duplicados por puntos
            var uniqueRankings = response.Models
                .GroupBy(r => r.points)
                .Select(g => g.First())
                .OrderByDescending(r => r.points)
                .ToList();

            foreach (var item in uniqueRankings)
            {
                Debug.Log("Id de trivia en category " + item.category);
            }

            OnRankingLoaded?.Invoke(uniqueRankings);

            return uniqueRankings;
        }

        return null;
    }

    private async Task GetUserAndRankings(int score, int triviaId)
    {
        try
        {
            // Cargar rankings generales y de trivia
            var generalRanking = await LoadGeneralRanking();
            var triviaRanking = await LoadCategoryRanking(triviaId);

            // Obtener posiciones del jugador
            int generalPosition = CalculatePlayerPosition(generalRanking, score);
            int triviaPosition = CalculatePlayerPosition(triviaRanking, score);

            // Obtener los 3 mejores puntajes
            var generalTopScores = generalRanking.Take(3).ToList();
            var triviaTopScores = triviaRanking.Take(3).ToList();

            // Crear y enviar los datos del usuario
            var data = new UserRankingData
            {
                UserScrore = score,
                GeneralPosition = generalPosition,
                TriviaPosition = triviaPosition,
                GeneralTopScores = generalTopScores,
                TriviaTopScores = triviaTopScores
            };

            OnUserRankingDataLoaded?.Invoke(data);
            Debug.Log("Datos del ranking del usuario enviados correctamente.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al obtener ranking del usuario: " + ex.Message);
        }
    }

    // Método auxiliar para calcular la posición del jugador
    private int CalculatePlayerPosition(List<Ranking> rankings, int playerScore)
    {
        // Ordenar puntajes en orden descendente y manejar empates
        var orderedScores = rankings
            .Select(r => r.points)
            .Distinct() // Eliminar duplicados para manejar empates
            .OrderByDescending(p => p)
            .ToList();

        // Encontrar la posición del puntaje del jugador
        int position = orderedScores.IndexOf(playerScore) + 1;

        // Si el puntaje no está en el ranking, asignar la posición final
        if (position == 0)
        {
            position = orderedScores.Count + 1;
        }

        return position;
    }


    private async void HandleExitGame()
    {
        await SaveUsageTime();
    }

    private async Task SaveUsageTime()
    {
        try
        {
            var newUserEntry = new User
            {
                usage_time = Time.realtimeSinceStartup,
                date = DateTime.UtcNow
            };
            var response = await clientSupabase.From<User>().Insert(newUserEntry);
            if (response != null)
            {
                Debug.Log("Tiempo de uso guardado correctamente");
            }
            else
            {
                Debug.LogError("Error al guardar el tiempo de uso - response null");
            }

            Application.Quit();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al guardar el tiempo de uso: " + ex.Message);
        }
    }

    private async Task SaveRankingData(int points, int triviaId)
    {
        Debug.Log("SaveRankingData");
        try
        {
            var newRanking = new Ranking { points = points, trivia_id = triviaId };

            var response = await clientSupabase.From<Ranking>().Insert(newRanking);

            if (response != null)
            {
                Debug.Log("Puntaje guardado");
            }
            else
            {
                Debug.LogError("Error al guardar el puntaje - response null");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al guardar el puntaje: {ex.Message}");
        }
    }

    private async void HandleEndGame(int points, int triviaId)
    {
        await SaveRankingData(points, triviaId);
        await GetUserAndRankings(points, triviaId);
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
        RankingSceneManager.OnSelectCategory -= HandleSelectCategory;
        GameManager.OnExitGame -= HandleExitGame;
    }
}
