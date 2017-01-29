using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRMV2.Models
{
    public class Ingredients
    {
        public string ingredientName { get; set; }
        public string measurement { get; set; }
        public string measuringVessel { get; set; }
        public string ingredientID { get; set; }
        public Recipe Recipe { get; set; }
    }
}