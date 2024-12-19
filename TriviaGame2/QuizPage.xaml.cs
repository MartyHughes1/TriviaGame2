using Microsoft.Maui.Controls;
using System.Data.Common;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Diagnostics;
using System.Net;

using System.Net;
using System.Text.Json;

namespace TriviaGame2
{
    public partial class QuizPage : ContentPage
    {
        private const string ApiUrl = "https://opentdb.com/api.php?amount={0}&category={1}";
        private List<TriviaQuestion> triviaQuestions = new();
        private int currentQuestionIndex = 0;
        private int currentPlayerIndex = 0;
        private List<int> playerScores = new();
        private List<string> playerNames = new(); //List to hold player names
        private int numberOfPlayers;
        private string category;
        private int questionsPerPlayer;


        public ObservableCollection<MyItem> MyItems { get; set; } = new ObservableCollection<MyItem>(); // Player name list
        public ObservableCollection<MyItem> MyItems2 { get; set; } = new ObservableCollection<MyItem>(); // Player score list
        public ObservableCollection<MyItem> MyItemsSaved { get; set; } // Save items for when program is relaunched
        public ObservableCollection<MyItem> MyItemsSaved2 { get; set; } // Save items for when program is relaunched


        // Updated constructor to accept player names
        public QuizPage(int numberOfPlayers, string category, int questionsPerPlayer, List<string> playerNames)
        {
            InitializeComponent();
            this.numberOfPlayers = numberOfPlayers;
            this.category = category;
            this.questionsPerPlayer = questionsPerPlayer;
            this.playerNames = playerNames;
            BindingContext = this;

            // Initialize player scores (set all to 0 initially)
            playerScores = Enumerable.Repeat(0, numberOfPlayers).ToList();

            // Initialize the PlayerScoresLabel to show the initial scores
            UpdatePlayerScoresLabel();



            

            // Load saved player names from preferences when the app starts
            List<MyItem> loadedNames = LoadNamesFromPreferences();
            MyItemsSaved = new ObservableCollection<MyItem>(loadedNames);

            // Load saved player scores from preferences when the app starts
            List<MyItem> loadedScores = LoadScoresFromPreferences();
            MyItemsSaved2 = new ObservableCollection<MyItem>(loadedScores);

            // Initialize the lists based on the loaded player names
            foreach (var item in MyItemsSaved)
            {
                MyItems.Add(item);
            }

            // Initialize the lists based on the loaded player scores
            foreach (var item in MyItemsSaved2)
            {
                MyItems2.Add(item);
            }
        }

        private async void OnStartClicked(object sender, EventArgs e)
        {

            // Hide these after start
            LeaderboardList.IsVisible = false;
            LeaderboardList2.IsVisible = false;
            ClearListBtn.IsVisible = false;
            StartButton.IsVisible = false;


            // Show the rest of the UI elements again
            QuestionNumberLabel.IsVisible = true;
            PlayerTurnLabel.IsVisible = true;
            QuestionLabel.IsVisible = true;
            Answer1.IsVisible = true;
            Answer2.IsVisible = true;
            Answer3.IsVisible = true;
            Answer4.IsVisible = true;
            SubmitButton.IsVisible = true;
            PlayerScoresLabel.IsVisible = true;


            // Calculate total questions needed (number of players * questions per player)
            int totalQuestions = numberOfPlayers * questionsPerPlayer;
            FetchTriviaQuestions(totalQuestions);
        }


        private async void FetchTriviaQuestions(int totalQuestions)
        {
            try
            {
                // Construct the API URL with the selected category and total number of questions
                string apiUrl = string.Format(ApiUrl, totalQuestions, GetCategoryId(category));

                using var client = new HttpClient();
                var response = await client.GetStringAsync(apiUrl);

                var triviaData = JsonSerializer.Deserialize<TriviaResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (triviaData?.Results?.Count > 0)
                {
                    triviaQuestions = triviaData.Results;
                    DisplayQuestion();
                }
                else
                {
                    await DisplayAlert("No Questions", "Unable to fetch trivia questions.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load trivia questions: {ex.Message}", "OK");
            }
        }

        private int GetCategoryId(string category)
        {
            // Mapping categories to their corresponding IDs in the trivia API
            switch (category)
            {
                case "General Knowledge":
                    return 9;
                case "Science and Nature":
                    return 17;
                case "Entertainment: Video Games":
                    return 15;
                case "Entertainment: Film":
                    return 11;
                case "Music":
                    return 12;
                case "Books":
                    return 10;
                case "Art":
                    return 25;
                default:
                    return 9; // Default to General Knowledge if the category is not found
            }
        }

        public static string DecodeHtml(string html)
        {
            return WebUtility.HtmlDecode(html);
        }

        // When quiz is finished, save both player names and scores
        private async void DisplayQuestion()
        {
            if (currentQuestionIndex < triviaQuestions.Count)
            {
                var currentQuestion = triviaQuestions[currentQuestionIndex];

                // Update the Question Number Label
                QuestionNumberLabel.Text = $"Question {currentQuestionIndex + 1}";

                // Update the Player Turn Label
                PlayerTurnLabel.Text = $"{playerNames[currentPlayerIndex]}'s turn";

                QuestionLabel.Text = DecodeHtml(currentQuestion.Question);

                var incorrectAnswers = currentQuestion.incorrect_answers ?? new List<string>();

                // Adjust the options based on the question type (True/False or multiple choice)
                if (currentQuestion.Type == "boolean")
                {
                    Answer1.Content = DecodeHtml("True");
                    Answer2.Content = DecodeHtml("False");
                    Answer3.IsVisible = false;
                    Answer4.IsVisible = false;
                }
                else
                {
                    var answers = new List<string>(incorrectAnswers) { currentQuestion.correct_answer };
                    answers = answers.OrderBy(_ => Guid.NewGuid()).ToList();

                    Answer1.Content = DecodeHtml(answers.ElementAtOrDefault(0) ?? "N/A");
                    Answer2.Content = DecodeHtml(answers.ElementAtOrDefault(1) ?? "N/A");
                    Answer3.Content = DecodeHtml(answers.ElementAtOrDefault(2) ?? "N/A");
                    Answer4.Content = DecodeHtml(answers.ElementAtOrDefault(3) ?? "N/A");

                    Answer3.IsVisible = true;
                    Answer4.IsVisible = true;
                }
            }
            else
            {
                // Quiz Complete! Show final scores and the Play Again button
                QuestionLabel.Text = "Quiz Complete!";
                await DisplayAlert("Quiz Finished", $"Final Scores:\n{GetPlayerScoresString()}", "OK");

                // Hide all other elements
                QuestionNumberLabel.IsVisible = false;
                PlayerTurnLabel.IsVisible = false;
                QuestionLabel.IsVisible = false;
                Answer1.IsVisible = false;
                Answer2.IsVisible = false;
                Answer3.IsVisible = false;
                Answer4.IsVisible = false;
                SubmitButton.IsVisible = false;
                PlayerScoresLabel.IsVisible = false;

                // Show these after quiz
                Trivia_image.IsVisible = true;
                PlayAgainButton.IsVisible = true;

                // Combine player names and scores into a list of PlayerScore objects
                List<PlayerScore> playerScoresList = playerNames
                    .Select((name, index) => new PlayerScore
                    {
                        PlayerName = name,
                        Score = playerScores[index]
                    })
                    .OrderByDescending(ps => ps.Score) // Sort by score in descending order
                    .ToList();

                // Update MyItems and MyItems2 with sorted player names and scores
                foreach (var playerScore in playerScoresList)
                {
                    MyItems.Add(new MyItem { Items = playerScore.PlayerName });
                    MyItems2.Add(new MyItem { Items = playerScore.Score.ToString() });
                }

                // Save tasks after adding a new one (include both MyItems and MyItems2)
                SaveNamesToPreferences(MyItems.ToList());
                SaveScoresToPreferences(MyItems2.ToList());
            }
        }


        private string GetPlayerScoresString()
        {
            var scoresString = "";
            for (int i = 0; i < playerScores.Count; i++)
            {
                scoresString += $"{playerNames[i]}: {playerScores[i]}\n";
            }
            return scoresString;
        }

        private void UpdatePlayerScoresLabel()
        {
            // Create a string with the scores for all players
            var scoresString = "";
            for (int i = 0; i < playerScores.Count; i++)
            {
                scoresString += $"{playerNames[i]}: {playerScores[i]}, ";

            }

            // Remove the last comma and space
            if (scoresString.EndsWith(", "))
                scoresString = scoresString.Substring(0, scoresString.Length - 2);

            // Update the PlayerScoresLabel
            PlayerScoresLabel.Text = scoresString;
        }

        private async void OnSubmitAnswerClicked(object sender, EventArgs e)
        {
            string selectedAnswer = null;

            if (Answer1.IsChecked)
                selectedAnswer = Answer1.Content.ToString();
            else if (Answer2.IsChecked)
                selectedAnswer = Answer2.Content.ToString();
            else if (Answer3.IsChecked)
                selectedAnswer = Answer3.Content.ToString();
            else if (Answer4.IsChecked)
                selectedAnswer = Answer4.Content.ToString();

            if (selectedAnswer == null)
            {
                await DisplayAlert("No Selection", "Please select an answer before submitting.", "OK");
                return;
            }

            var currentQuestion = triviaQuestions[currentQuestionIndex];
            if (selectedAnswer == currentQuestion.correct_answer)
            {
                playerScores[currentPlayerIndex]++;
            }

            // Update the scores label after the answer is submitted
            UpdatePlayerScoresLabel();

            // Move to the next question and switch to the next player
            currentQuestionIndex++;
            currentPlayerIndex = (currentPlayerIndex + 1) % numberOfPlayers;

            DisplayQuestion();
        }

        // Handle the "Play Again" button click event
        private async void OnPlayAgainClicked(object sender, EventArgs e)
        {
            // Navigate back to the mainpage to restart quiz
            await Navigation.PopToRootAsync();

            // Hide these after quiz
            Trivia_image.IsVisible = false;
            PlayAgainButton.IsVisible = false;
            LeaderboardList.IsVisible = false;
            LeaderboardList2.IsVisible = false;
            ClearListBtn.IsVisible = false;


            // Show the rest of the UI elements again
            QuestionNumberLabel.IsVisible = true;
            PlayerTurnLabel.IsVisible = true;
            QuestionLabel.IsVisible = true;
            Answer1.IsVisible = true;
            Answer2.IsVisible = true;
            Answer3.IsVisible = true;
            Answer4.IsVisible = true;
            SubmitButton.IsVisible = true;
            PlayerScoresLabel.IsVisible = true;

            // Update the scores label to show all players' scores as 0
            UpdatePlayerScoresLabel();

            // Restart the quiz
            DisplayQuestion();
        }

        //For list of leaderboard
        public class MyItem
        {
            public string Items { get; set; }
        }


        // Method to save player names to preferences
        private void SaveNamesToPreferences(List<MyItem> NamesPlayer)
        {
            string json = JsonSerializer.Serialize(NamesPlayer.OrderBy(x => x.Items).ToList());
            Preferences.Set("PlayerNames", json); // Save player names in order if necessary
        }

        // Method to save player scores to preferences
        private void SaveScoresToPreferences(List<MyItem> Playerscores)
        {
            string json = JsonSerializer.Serialize(Playerscores.OrderByDescending(x => int.TryParse(x.Items, out var score) ? score : 0).ToList());
            Preferences.Set("PlayerScores", json); // Save player scores in descending order
        }


        // Method to load player names from preferences
        private List<MyItem> LoadNamesFromPreferences()
        {
            string json = Preferences.Get("PlayerNames", "");
            if (!string.IsNullOrEmpty(json))
            {
                var names = JsonSerializer.Deserialize<List<MyItem>>(json);
                return names.OrderBy(x => x.Items).ToList(); // Order the names list if necessary
            }
            return new List<MyItem>(); // Return empty list if no names exist
        }



        // Method to load player scores from preferences
        private List<MyItem> LoadScoresFromPreferences()
        {
            string json = Preferences.Get("PlayerScores", "");
            if (!string.IsNullOrEmpty(json))
            {
                var scores = JsonSerializer.Deserialize<List<MyItem>>(json);
                // Order the scores in descending order based on the scores
                return scores.OrderByDescending(x => int.TryParse(x.Items, out var score) ? score : 0).ToList();
            }
            return new List<MyItem>(); // Return empty list if no scores exist
        }


        private void ClearLeaderboard(object sender, EventArgs e)
        {
            // Clear the leaderboard
            MyItems.Clear();
            MyItems2.Clear();

            // Save the updated lists (MyItems and MyItems2) to preferences
            SaveNamesToPreferences(MyItems.ToList());
            SaveScoresToPreferences(MyItems2.ToList());

        }

        public class PlayerScore
        {
            public string PlayerName { get; set; }
            public int Score { get; set; }
        }







    }
}