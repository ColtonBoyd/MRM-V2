using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRMV2.Models
{
    public class Users
    {
        public string userID { get; set; }
        public string user_Description { get; set; }
        public string userName { get; set; }
        public string userProfilePicture { get; set; }
        public List<Recipe> LikedRecipes = new List<Recipe>();
        public List<Recipe> UploadedRecipes = new List<Recipe>();
        public int LikedRecipesCount { get; set; }
        public int UploadedRecipesCount { get; set; }
    }
}