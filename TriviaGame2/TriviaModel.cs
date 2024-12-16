using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaGame2
{

    public class TriviaQuestion
    {
        public string Category { get; set; }
        public string Type { get; set; }
        public string Difficulty { get; set; }
        public string Question { get; set; }
        public string correct_answer { get; set; } // Change to match JSON
        public List<string> incorrect_answers { get; set; } = new List<string>(); // Change to match JSON
    }

    public class TriviaResponse
    {
        public int ResponseCode { get; set; }
        public List<TriviaQuestion> Results { get; set; }
    }
}
