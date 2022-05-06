using System;
using System.Collections.Generic;

namespace TimesheetBackend.Models
{
    public class Employee
    {
        public int IdEmployee { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool Active { get; set; }
        public string ImageLink { get; set; }

    }
}
