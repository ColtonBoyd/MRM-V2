using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MRMV2.Models
{
    public class AdminPageModel
    {

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "The email Field is not a valid email address.")]
        public string inviteEmail { get; set; }

        public List<Report> comReports { get; set; }
        public List<Report> recReports { get; set; }

    }
}