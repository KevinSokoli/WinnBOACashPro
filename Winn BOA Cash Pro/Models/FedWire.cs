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
        [Display(Name = "To Bank City")]
        public string ToBankCity { get; set; } = null!;
        public string TransactionStatus { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = null!;
        [Display(Name = "Description Line 1")]
        public string Description { get; set; } = null!;
        [Display(Name = "Description Line 2")]
        public string? Description1 { get; set; }
        [Display(Name = "Description Line 3")]
        public string? Description2 { get; set; }
        [Display(Name = "Description Line 4")]
        public string? Description3 { get; set; }
        [Display(Name = "Description Line 5")]
        public string? Description4 { get; set; }
        [Display(Name = "From Bank Zip code")]
        public string FromBankZip { get; set; } = null!;
        [Display(Name = "From Bank State")]
        public string FromBankState { get; set; } = null!;
        [Display(Name = "From Bank City")]
        public string FromBankCity { get; set; } = null!;
        [Display(Name = "To Bank Zip code")]
        public string ToBankZip { get; set; } = null!;
        [Display(Name = "To Bank State")]
        public string ToBankState { get; set; } = null!;
    }
}
