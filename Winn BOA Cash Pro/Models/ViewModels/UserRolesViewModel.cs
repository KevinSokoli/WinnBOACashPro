using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Winn_BOA_Cash_Pro.Models.ViewModels
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
