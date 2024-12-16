namespace TriviaGame2
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            PlayerPicker.SelectedIndexChanged += OnPlayerPickerChanged;
        }

        // Handle changes in the number of players selection
        private void OnPlayerPickerChanged(object sender, EventArgs e)
        {
            int numberOfPlayers = PlayerPicker.SelectedIndex + 1;  // Get the number of players selected
            CreatePlayerNameEntries(numberOfPlayers);
        }

        // Dynamically create Entry fields for player names based on the selected number of players
        private void CreatePlayerNameEntries(int numberOfPlayers)
        {
            PlayerNameStackLayout.Children.Clear(); // Clear previous entries

            for (int i = 1; i <= numberOfPlayers; i++)
            {
                var playerNameEntry = new Entry
                {
                    Placeholder = $"Enter name for Player {i}",
                    MaxLength = 20
                };
                PlayerNameStackLayout.Children.Add(playerNameEntry);
            }
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
            int numberOfPlayers = PlayerPicker.SelectedIndex + 1;

            // Get the player names from the Entry fields
            List<string> playerNames = new List<string>();
            foreach (var child in PlayerNameStackLayout.Children)
            {
                if (child is Entry entry && !string.IsNullOrEmpty(entry.Text))
                {
                    playerNames.Add(entry.Text);
                }
                else
                {
                    playerNames.Add($"Player {playerNames.Count + 1}"); // Default name if empty
                }
            }

            // Ensure the number of player names matches the selected number of players
            if (playerNames.Count != numberOfPlayers)
            {
                await DisplayAlert("Missing Player Names", "Please enter names for all players.", "OK");
                return;
            }

            // Get the category selected by the user
            string category = CategoryPicker.SelectedItem.ToString();

            // Get the number of questions per player selected by the user
            int questionsPerPlayer = int.Parse(QuestionsPicker.SelectedItem.ToString());

            // Navigate to the QuizPage and pass the player names, number of players, category, and questions per player
            await Navigation.PushAsync(new QuizPage(numberOfPlayers, category, questionsPerPlayer, playerNames));
        }
    }
}
