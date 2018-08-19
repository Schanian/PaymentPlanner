using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculations
{
     //NextPayDate = dateOfNextPay.ToShortDateString(), PresentValue = remainingBalance.ToString("C2"), InterestPaid = interstAmount.ToString("C2"), EffectivePayment = effectivePayment.ToString("C2"), RemainingAmount = (remainingBalance += effectivePayment).ToString("C2"), TotalInterest = totalInterest.ToString("C2")
    public class Payment
    {
        //public DateTime NextPayDate { get; set; }
        //public decimal PresentValue { get; set; }
        //public decimal InterestPaid { get; set; }
        //public decimal EffectivePayment { get; set; }
        //public decimal RemainingAmount { get; set; }
        //public decimal TotalInterest { get; set; } 

        public string Seq { get; set; } 
        public String PaymentDate { get; set; }
        public String PaymentAmount { get; set; }
 
        //public String PresentValue { get; set; }

        public String EffectivePayment { get; set; }
        public String RemainingAmount { get; set; }  
        public String InterestPaid { get; set; }
        public String TotalInterest { get; set; }
        public String TotalSaving { get; set; }


        public Payment()
        { }
        public Payment(int seq,decimal  paymentAmount, decimal remainingAmount, DateTime nextPayDate, decimal presentValue, decimal interestPaid,decimal effectivePayment, decimal totalInterest,decimal totalSaving )
        {
            this.Seq = seq.ToString();
            this.PaymentDate = nextPayDate.ToShortDateString();
            this.PaymentAmount = paymentAmount.ToString("C2");
            //this.PresentValue = presentValue.ToString("C2");
            this.InterestPaid = interestPaid.ToString("C2");
            this.EffectivePayment = effectivePayment.ToString("C2");
            this.RemainingAmount = remainingAmount.ToString("C2");
            this.TotalInterest = totalInterest.ToString("C2");
            this.TotalSaving = totalSaving.ToString("C2");

        }
    }
}
