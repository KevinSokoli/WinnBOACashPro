using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Winn_BOA_Cash_Pro.Models
{
    public partial class FedWire
    {
        public int Id { get; set; }
        //[Display(Name = "From Account")]
        public string FromAccountName { get; set; } = null!;
        public string FromBankName { get; set; } = null!;
        public string FromAccountNumber { get; set; } = null!;
        public string FromAbanumber { get; set; } = null!;
        public string ToAccountName { get; set; } = null!;
        public string ToBankName { get; set; } = null!;
        public string ToAccountNumber { get; set; } = null!;
        public string ToAbanumber { get; set; } = null!;
        public decimal TransferAmount { get; set; }
        //[Display(Name = "To Bank City")]
        public string ToBankCity { get; set; } = null!;
        public string TransactionStatus { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
