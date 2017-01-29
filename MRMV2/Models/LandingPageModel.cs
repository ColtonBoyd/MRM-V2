using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRMV2.Models
{
    public class LandingPageModel
    {
        public List<Recipe> getRecipes { get; set; }
        public Users getUser { get; set; }
    }
}