using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRMV2.Models
{
    public class Comments
    {
        public string commentID { get; set; }
        public string comment { get; set; }
        public string commentDate { get; set; }

        public Users getUser { get; set; }
        public Recipe Recipe { get; set; }
        //public int recipeID { get; set; }
    }

}