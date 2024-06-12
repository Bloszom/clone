using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcea.Models
{
    [Table("SsoInvoice", Schema = "dbo")]
    public partial class SsoInvoice
    {
        [Key]
        public long RecId { get; set; }
        [Column("orderId")]
        [StringLength(50)]
        public string OrderId { get; set; }
        [Column("companyName")]
        public string CompanyName { get; set; }
        [Column("appUserId")]
        [StringLength(250)]
        public string AppUserId { get; set; }
        [Column("paymentUrl")]
        public string PaymentUrl { get; set; }
        [Column("paymentBankName")]
        [StringLength(250)]
        public string PaymentBankName { get; set; }
        [Column("paymentBankBranch")]
        [StringLength(250)]
        public string PaymentBankBranch { get; set; }
        [Column("paymentBankCountry")]
        [StringLength(250)]
        public string PaymentBankCountry { get; set; }
        [Column("applicationOrderId")]
        [StringLength(100)]
        public string ApplicationOrderId { get; set; }
        [Column("tellerNo")]
        [StringLength(100)]
        public string TellerNo { get; set; }
        [Column("description")]
        [StringLength(250)]
        public string Description { get; set; }
        [Column("receiptNo")]
        [StringLength(100)]
        public string ReceiptNo { get; set; }
        [Column("invoiceNo")]
        [StringLength(100)]
        public string InvoiceNo { get; set; }
        [Column("calllbackUrl")]
        public string CalllbackUrl { get; set; }
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; }
        [Column("rrr")]
        [StringLength(100)]
        public string Rrr { get; set; }
        [Column("receiptPath")]
        [StringLength(250)]
        public string ReceiptPath { get; set; }
        [Column("receiptUploadedBy")]
        [StringLength(250)]
        public string ReceiptUploadedBy { get; set; }
        [Column("transactionId")]
        [StringLength(250)]
        public string TransactionId { get; set; }
        [Column("dateInitialized", TypeName = "date")]
        public DateTime? DateInitialized { get; set; }
        [Column("datePaid", TypeName = "date")]
        public DateTime? DatePaid { get; set; }
        [Column("responseCode")]
        [StringLength(50)]
        public string ResponseCode { get; set; }
        [Column("amount", TypeName = "decimal(18, 3)")]
        public decimal? Amount { get; set; }
        [Column("paymentType")]
        [StringLength(50)]
        public string PaymentType { get; set; }
        [Column("createdBy")]
        [StringLength(100)]
        public string CreatedBy { get; set; }
        [Column("currencyId")]
        [StringLength(100)]
        public string CurrencyId { get; set; }
        [Column("currencyCode")]
        [StringLength(100)]
        public string CurrencyCode { get; set; }
        [Column("currencyName")]
        [StringLength(100)]
        public string CurrencyName { get; set; }
    }
}
