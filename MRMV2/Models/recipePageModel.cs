using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRMV2.Models
{
    public class RecipePageModel
    {
        public List<Comments> getComments { get; set; }
        public List<Picture> getImages { get; set; }
        public List<Ingredients> getIngredients { get; set; }
        public List<Appliances_Used> getAppliances { get; set; }
        public List<User_Defined_Tags> getTags { get; set; }
        public Recipe getRecipe = new Recipe();
        public string signedIn { get; set; }
        public Users Uploader { get; set; }
    }
}