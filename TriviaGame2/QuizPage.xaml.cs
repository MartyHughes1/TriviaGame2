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
        private int numberOfPlayers;
        private string category;
        private int questionsPerPlayer;

        public QuizPage(int numberOfPlayers, string category, int questionsPerPlayer)
        {
            InitializeComponent();
            this.numberOfPlayers = numberOfPlayers;
            this.category = category;
            this.questionsPerPlayer = questionsPerPlayer;

            // Initialize player scores (set all to 0 initially)
            playerScores = Enumerable.Repeat(0, numberOfPlayers).ToList();

            // Initialize the PlayerScoresLabel to show the initial scores
            UpdatePlayerScoresLabel();

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

        private async void DisplayQuestion()
        {
            if (currentQuestionIndex < triviaQuestions.Count)
            {
                var currentQuestion = triviaQuestions[currentQuestionIndex];

                // Update the Question Number Label
                QuestionNumberLabel.Text = $"Question {currentQuestionIndex + 1}";

                // Update the Player Turn Label
                PlayerTurnLabel.Text = $"Player {currentPlayerIndex + 1}'s turn";

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

                // Show the Play Again button and image
                Trivia_image.IsVisible = true;
                PlayAgainButton.IsVisible = true;
            }
        }

        private string GetPlayerScoresString()
        {
            var scoresString = "";
            for (int i = 0; i < playerScores.Count; i++)
            {
                scoresString += $"Player {i + 1}: {playerScores[i]}\n";
            }
            return scoresString;
        }

        private void UpdatePlayerScoresLabel()
        {
            // Create a string with the scores for all players
            var scoresString = "";
            for (int i = 0; i < playerScores.Count; i++)
            {
                scoresString += $"Player {i + 1}: {playerScores[i]}, ";
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



            // Hide the "Play Again" button and image
            Trivia_image.IsVisible = false;
            PlayAgainButton.IsVisible = false;

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
    }
}
