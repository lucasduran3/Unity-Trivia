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
using Supabase.Gotrue.Exceptions;
using System.ComponentModel;

public class DatabaseManager : MonoBehaviour
{
    string supabaseUrl = "https://iwdrxfxeosvebxwayxfh.supabase.co"; //COMPLETAR
    string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Iml3ZHJ4Znhlb3N2ZWJ4d2F5eGZoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzIyMjE1MDEsImV4cCI6MjA0Nzc5NzUwMX0.l_3eGWD_kVqEd8E64f9kArIEsedViSiw5Q1hc6NpKZ4"; //COMPLETAR

    [SerializeField] SupabaseClientData supabaseClientData;

    //Supabase.Client clientSupabase;

    public static event Action<List<Ranking>> OnRankingLoaded;
    public static event Action<bool> OnTriviaDataLoaded;
    public static event Action<UserRankingData> OnUserRankingDataLoaded;
    public static event Action<string> OnAuthError;

    public int index;

    private void Awake()
    {
        FinishGameManager.OnGameEnd += HandleEndGame;
    }
    async void Start()
    {
        if (supabaseClientData.Client == null)
        {
            supabaseClientData.Client = new Supabase.Client(supabaseUrl, supabaseKey);
        }

        index = PlayerPrefs.GetInt("SelectedIndex");
        GameManager.OnExitGame += HandleExitGame;
        RankingSceneManager.OnSelectCategory += HandleSelectCategory;

        await LoadTriviaData(index);
    }

    async Task LoadTriviaData(int index)
    {
        var response = await supabaseClientData.Client
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
    var rankingResponse = await supabaseClientData.Client
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

        var rankings = generalRanking
            .Select(gr => new Ranking
            {
                trivia_id = gr.trivia_id,
                points = gr.points,
                category = gr.category
            })
            .OrderByDescending(r => r.points) // Ordenar nuevamente por puntos
            .ToList();

        OnRankingLoaded?.Invoke(rankings);

        return rankings;
    }
    return null;
}

    private async Task<List<Ranking>> LoadCategoryRanking(int triviaId)
    {
        var response = await supabaseClientData.Client
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

            var newTimeEntry = new TimeModel
            {
                usage_time = Time.realtimeSinceStartup,
                date = DateTime.UtcNow, 
                user_id = supabaseClientData.Client.Auth.CurrentUser.Id,
            };
            var response = await supabaseClientData.Client.From<TimeModel>().Insert(newTimeEntry);
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
            var userId = supabaseClientData.Client.Auth.CurrentUser.Id;

            // Buscar si existe una fila con el mismo user_id y trivia_id
            var existingRankingResponse = await supabaseClientData.Client
                .From<Ranking>()
                .Where(r => r.user_id == userId && r.trivia_id == triviaId)
                .Get();

            if (existingRankingResponse.Models.Any())
            {
                // Actualizar la fila existente
                var existingRanking = existingRankingResponse.Models.First();
                existingRanking.points = points; // Actualizar el valor de points

                var updateResponse = await supabaseClientData.Client
                    .From<Ranking>()
                    .Update(existingRanking);

                if (updateResponse != null)
                {
                    Debug.Log("Puntaje actualizado correctamente.");
                }
                else
                {
                    Debug.LogError("Error al actualizar el puntaje - response null.");
                }
            }
            else
            {
                // Insertar una nueva fila
                var newRanking = new Ranking
                {
                    points = points,
                    trivia_id = triviaId,
                    category = TriviaSelection.trivias[GameManager.Instance.currentTriviaIndex - 1].category,
                    user_id = userId
                };

                var insertResponse = await supabaseClientData.Client
                    .From<Ranking>()
                    .Insert(newRanking);

                if (insertResponse != null)
                {
                    Debug.Log("Puntaje guardado correctamente.");
                }
                else
                {
                    Debug.LogError("Error al guardar el puntaje - response null.");
                }
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


    #region Auth
    public async void SignInWithEmail(string email, string password)
    {
        try
        {
            var session = await supabaseClientData.Client.Auth.SignIn(email, password);
            GameManager.Instance.StartScene("MainMenu");
        }
        catch (GotrueException ex)
        {       
            Debug.LogError("Error en sign in " + ex.Message);
            OnAuthError?.Invoke(ex.Message);
        }
    }

    public async void SignUpWithEmail(string username, string email, string password)
    {
        try
        {
            var session = await supabaseClientData.Client.Auth.SignUp(email, password);
            if (session != null)
            {
                session.User.UserMetadata.Add("username", username);
                GameManager.Instance.StartScene("MainMenu");
            }
        }
        catch (GotrueException ex)
        {
            Debug.LogError("Error en sign up " + ex.Message);
            OnAuthError?.Invoke(ex.Message);
        }
    }
    #endregion

    private void OnDestroy()
    {
        RankingSceneManager.OnSelectCategory -= HandleSelectCategory;
        GameManager.OnExitGame -= HandleExitGame;
    }
}
