using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRMV2.Models
{
    public class LandingPageModel
    {
        public List<Recipe> getHotRecipes { get; set; }
        public List<Recipe> getNewRecipes { get; set; }

        //Comment object has all fields required for a news item
        //public List<NewsModel> getNews { get; set; }
    }
}