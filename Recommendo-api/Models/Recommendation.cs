﻿using System;
using System.Collections.Generic;

namespace Recommendo_api.Models
{
    //todo: some model validation
    public partial class Recommendation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } 
        public string Recommender { get; set; }
    }
}
