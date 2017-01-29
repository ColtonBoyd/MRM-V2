using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MRMV2.Models
{
    public class AddRecipeViewModel
    {


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

        public string displayPictureLocation { get; set; }

    }
}