using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Winn_BOA_Cash_Pro.Models
{
    public class AppUser : IdentityUser
    {
        public DateTime Created { get; set; }
        [DataType(DataType.EmailAddress)]
        public string CreatedBy { get; set; }
        public string? EmployeeId { get; set; }
    }
}
