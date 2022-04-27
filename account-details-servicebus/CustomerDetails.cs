using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace account_details_servicebus
{
    public class CustomerDetails
    {
        [Key]
        public Guid UserId { get; set; }
        public int AccountNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailId { get; set; }
        public string UniversityName { get; set; }
        public string UserType { get; set; }
        public bool IsEnabled { get; set; }
    }
}
