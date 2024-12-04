using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Supabase.Gotrue.Exceptions;
using Unity.VisualScripting;

public class DatabaseManager : MonoBehaviour
{
    #region Variables
    string supabaseUrl = "https://iwdrxfxeosvebxwayxfh.supabase.co";
    string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Iml3ZHJ4Znhlb3N2ZWJ4d2F5eGZoIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzIyMjE1MDEsImV4cCI6MjA0Nzc5NzUwMX0.l_3eGWD_kVqEd8E64f9kArIEsedViSiw5Q1hc6NpKZ4";
    public int index;

    [SerializeField] SupabaseClientData supabaseClientData; //Scriptable object para tener acceso al cliente en todas las escenas

    #region Events
    public static event Action<List<Ranking>> OnRankingLoaded;
    public static event Action<List<Ranking>> OnUserRankingLoaded;
    public static event Action<UserRankingData> OnUserRankingDataLoaded;
    public static event Action<string> OnAuthError;
    public static event Action OnTriviaDataLoaded;
    #endregion

    #endregion

    #region Methods
    #region Built in Methods
    private void Awake()
    {
        FinishGameManager.OnGameEnd += HandleEndGame;

        if (supabaseClientData.Client == null)
        {
            supabaseClientData.Client = new Supabase.Client(supabaseUrl, supabaseKey);
        }
    }
    async void Start()
    {    
        GameManager.OnExitGame += HandleExitGame;
        RankingSceneManager.OnSelectCategory += HandleSelectCategory;
        RankingSceneManager.OnRankingSceneStart += HandleStartRanking;

        index = PlayerPrefs.GetInt("SelectedIndex");
       
        await LoadTriviaData(index); //Cargar info de la trivia al iniciar la escena Main
    }
    private void OnDestroy()
    {
        GameManager.OnExitGame -= HandleExitGame;
        RankingSceneManager.OnSelectCategory -= HandleSelectCategory;
        RankingSceneManager.OnRankingSceneStart -= HandleStartRanking;
        FinishGameManager.OnGameEnd -= HandleEndGame;
    }
    #endregion

    #region Custom Methods
    #region Async Loading Methods
   public async Task LoadTriviaData(int index)
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
            try { OnTriviaDataLoaded?.Invoke(); }
            catch (Exception ex)
            {
                Debug.LogError($"error al cargar trivia data: {ex.Message}");
                return;
            }
        }
    }

    public async Task<List<Ranking>> LoadGeneralRanking()
    {
        try
        {
            var rankingResponse = await supabaseClientData.Client
                .From<Ranking>()
                .Select("*")
                .Get();

            var triviasLoaded = TriviaSelection.trivias;

            if (triviasLoaded != null && rankingResponse != null)
            {
                //Relacionar tablas mediante trivia id
                var generalRanking = triviasLoaded
                    .Join(
                        rankingResponse.Models,
                        trivia => trivia.id,
                        ranking => ranking.trivia_id,
                        (trivia, ranking) => new { trivia.id, trivia.category, ranking.points }
                    )
                    .GroupBy(item => new { item.id, item.category })
                    .Select(group => new Ranking
                    {
                        trivia_id = group.Key.id,
                        category = group.Key.category,
                        points = group.Max(item => item.points)
                    })
                    .OrderByDescending(result => result.points)
                    .ToList();

                OnRankingLoaded?.Invoke(generalRanking);

                return generalRanking;
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cargar ranking general {ex.Message}");
            return null;
        }
    }

    public async Task<List<Ranking>> LoadCategoryRanking(int triviaId)
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

            OnRankingLoaded?.Invoke(uniqueRankings);

            return uniqueRankings;
        }

        return null;
    }

    public async Task<List<Ranking>> LoadUserRanking()
    {
        try
        {
            var currentUserId = supabaseClientData.Client.Auth.CurrentUser.Id;

            var rankingResponse = await supabaseClientData.Client
                .From<Ranking>()
                .Select("*")
                .Filter(r => r.user_id, Postgrest.Constants.Operator.Equals, currentUserId)
                .Get();

            if (rankingResponse.Models != null)
            {
                OnUserRankingLoaded?.Invoke(rankingResponse.Models);
                return rankingResponse.Models;
            }
            else
            {
                Debug.LogWarning("No se encontro ranking para el usuario actual.");
                return new List<Ranking>();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cargar el ranking del usuario: {ex.Message}");
            return new List<Ranking>();
        }
    }

    public async Task GetUserAndRankings(int score, int triviaId)
    {
        try
        {
            var generalRanking = await LoadGeneralRanking();
            var triviaRanking = await LoadCategoryRanking(triviaId);

            // Obtener posiciones del jugador
            int generalPosition = CalculatePlayerPosition(generalRanking, score);
            int triviaPosition = CalculatePlayerPosition(triviaRanking, score);

            // Obtener los 3 mejores puntajes
            var generalTopScores = generalRanking.ToList();
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
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al obtener ranking del usuario: " + ex.Message);
        }
    }

    public int CalculatePlayerPosition(List<Ranking> rankings, int playerScore)
    {
        var orderedScores = rankings
            .Select(r => r.points)
            .Distinct() //Eliminar duplicados
            .OrderByDescending(p => p)
            .ToList();

        //Encontrar la posición del puntaje del jugador
        int position = orderedScores.IndexOf(playerScore) + 1;

        //Si el puntaje no está en el ranking, asignar la posición final
        if (position == 0)
        {
            position = orderedScores.Count + 1;
        }

        return position;
    }
    #endregion

    #region Saving Methods
    public async Task SaveUsageTime()
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

    public async Task SaveRankingData(int points, int triviaId)
    {
        if (supabaseClientData?.Client?.Auth?.CurrentUser == null)
        {
            Debug.LogError("El usuario no está autenticado o supabaseClientData no está configurado.");
            Debug.Log(supabaseClientData);
            Debug.Log(supabaseClientData.Client);
            Debug.Log(supabaseClientData.Client.Auth.CurrentUser);
            return;
        }

        try
        {
            var userId = supabaseClientData.Client.Auth.CurrentUser.Id;

            //Buscar si existe un registro con el mismo user_id y trivia_id
            var existingRankingResponse = await supabaseClientData.Client
                .From<Ranking>()
                .Where(r => r.user_id == userId && r.trivia_id == triviaId)
                .Get();

            if (existingRankingResponse.Models.Any())
            {
                var existingRanking = existingRankingResponse.Models.First();
                existingRanking.points = points; //Actualizar solo los puntos del registro existente

                var updateResponse = await supabaseClientData.Client
                    .From<Ranking>()
                    .Update(existingRanking); //Actualizar el registro en la base de datos

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
                if (TriviaSelection.trivias == null || GameManager.Instance.currentTriviaIndex <= 0 || GameManager.Instance.currentTriviaIndex > TriviaSelection.trivias.Count)
                {
                    Debug.LogError("El índice de la trivia no es válido o TriviaSelection.trivias es null.");
                    return;
                }

                //Si no se encontro un registro insertamos uno nuevo
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
    #endregion

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
                await supabaseClientData.Client.Auth.SignIn(email, password);
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

    #region Event Handlers
    public async void HandleExitGame()
    {
        await SaveUsageTime();
    }
    public async void HandleEndGame(int points, int triviaId)
    {
        await SaveRankingData(points, triviaId);
        await GetUserAndRankings(points, triviaId);
    }

    public async void HandleSelectCategory(int trivia_id)
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

    public async void HandleStartRanking()
    {
        await LoadUserRanking();
    }

    #endregion
    #endregion
    #endregion
}
