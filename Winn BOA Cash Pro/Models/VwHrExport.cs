using System;
using System.Collections.Generic;

namespace Winn_BOA_Cash_Pro.Models
{
    public partial class VwHrExport
    {
        public string? EmployeeId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? Ssologin { get; set; }
        public string? BusinessEmail { get; set; }
        public string? BusinessPhone { get; set; }
        public string? Status { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string? Office { get; set; }
        public string? EmployeeGroup { get; set; }
        public string? ManagerId { get; set; }
    }
}
