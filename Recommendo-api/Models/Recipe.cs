using System;
using System.Collections.Generic;

namespace Recommendo_api.Models
{
    public partial class Recipe
    {
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ingredients { get; set; }
        public string Method { get; set; }

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
