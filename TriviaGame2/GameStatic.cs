using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaGame2
{
    // A static class to store global game state (like player scores)
    public static class GameStatic
    {
        // List to store scores for 4 players
        public static List<int> PlayerScores = new List<int> { 0, 0, 0, 0 }; // Initialize scores for 4 players
    }
}

