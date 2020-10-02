using System;
using System.Collections.Generic;

namespace Recommendo_api.Models
{
    public partial class Recipe
    {
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
