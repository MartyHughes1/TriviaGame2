using Microsoft.Maui.Controls;
using System.Data.Common;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Diagnostics;
using System.Net; // Add this using directive

namespace TriviaGame2
{
    public partial class MainPage : ContentPage
    {
        


        public MainPage()
        {
            InitializeComponent();
            


        }



        // This method will be called when the "Go to Settings" button is clicked
        private async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            // Navigate to the SettingsPage
            await Navigation.PushAsync(new SettingsPage());
        }
    }




}
