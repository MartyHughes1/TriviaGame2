namespace TriviaGame2
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        // Handle the "Start Quiz" button click event
        private async void OnStartQuizButtonClicked(object sender, EventArgs e)
        {
            // Validate that the user has selected valid options
            if (PlayerPicker.SelectedIndex == -1 || CategoryPicker.SelectedIndex == -1 || QuestionsPicker.SelectedIndex == -1)
            {
                // Show an alert if any picker has not been selected
                await DisplayAlert("Missing Selection", "Please select a number of players, a category, and the number of questions per player.", "OK");
                return;
            }

            // Get the number of players selected by the user
            int numberOfPlayers = PlayerPicker.SelectedIndex + 1;  // Picker starts from index 0

            // Get the category selected by the user
            string category = CategoryPicker.SelectedItem.ToString(); // Get selected category

            // Get the number of questions per player selected by the user
            int questionsPerPlayer = int.Parse(QuestionsPicker.SelectedItem.ToString());

            // Navigate to the QuizPage and pass the number of players, category, and questions per player
            await Navigation.PushAsync(new QuizPage(numberOfPlayers, category, questionsPerPlayer));
        }
    }
}
