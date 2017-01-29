using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MRMV2.Models
{
    public class EditPageModel
    {
        public List<Comments> getComments { get; set; }
        public List<Picture> getImages { get; set; }
        public List<Ingredients> getIngredients { get; set; }
        public List<Appliances_Used> getAppliances { get; set; }
        public List<User_Defined_Tags> getTags { get; set; }
        public Recipe getRecipe = new Recipe();
        public string signedIn { get; set; }
        public Users Uploader { get; set; }


        public List<string> Measurement { get; set; }
        public List<string> Ingredient_Name { get; set; }
        public string applianceUsed { get; set; }


        [Required(ErrorMessage = "Recipe completion time is required!")]
        public string time { get; set; }

        public string Tags { get; set; }

        [Required]
        public string Instructions { get; set; }

        [Required]
        public string Description { get; set; }

        [Required(ErrorMessage = "A recipe name is required!")]
        public string recipeName { get; set; }

        public int recipeVisibility { get; set; }
        //public string displayPictureLocation { get; set; }
    }
}