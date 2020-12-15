using System;
using System.Collections.Generic;

namespace Recommendo_api.Models
{
    //todo: some model validation
    public partial class Recommendation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public enum RecommdationType
        {
            Recipe = 0,
            Movie = 10,
            TvShow = 20,
            Book = 30,
            Game = 40,
            Music = 60,
            DaysOut = 70,

        }
    }
}
