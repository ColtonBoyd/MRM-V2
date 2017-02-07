using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MRMV2.Models
{
    public class NewsModel
    {

        public int Id { get; set; }
        public string News1 { get; set; }
        public System.DateTime UploadDate { get; set; }
        public string UserID { get; set; }
        public string UserPicturePath { get; set; }
        public string UserName { get; set; }

    }
}