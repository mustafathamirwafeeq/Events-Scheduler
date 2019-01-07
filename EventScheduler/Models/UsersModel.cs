using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EventScheduler.Models
{
    public class UsersModel
    {
        public int userID { get; set; }
        [StringLength(10, ErrorMessage = "First name should not be greater than 10 characters")]
        [RegularExpression(@"[A-Za-z ]*$", ErrorMessage = "Please Provide Valid First Name!")]
        public string FirstName { get; set; }
        [StringLength(10, ErrorMessage = "First name should not be greater than 10 characters")]
        [RegularExpression(@"[A-Za-z ]*$", ErrorMessage = "Please Provide Valid First Name!")]
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        ////////////////////////

        public string ToEmail { get; set; }

        public string EMailBody { get; set; }
        public string EmailSubject { get; set; }
        public string EmailCC { get; set; }
        public string EmailBCC { get; set; }
        
        //////////////////////
    }
}