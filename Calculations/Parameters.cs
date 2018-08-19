using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculations
{
    public class CalculationParameter
    {
        public decimal PresentValue { get; set; }
        public decimal FutureValue { get; set; }
        public decimal? RateOfIntrest { get; set; }
        public ushort? NumOfPayments { get; set; }
        public DateTime DateOfFirstPayment { get; set; }
        public DateTime DateOfPreviousPayment { get; set; }
        public Calculation.Frequancy PayFrequancy { get; set; }
        public decimal? PaymentAmount { get; set; }
        public bool EndCycleBilling { get; set; }

        //public void Set(decimal presentValue, decimal futureValue, decimal rateOfIntrest, ushort numOfYears, DateTime dateOfFirstPayment, Frequancy payFreq, decimal paymentAmount)
        //{
        //    PresentValue = presentValue;
        //    FutureValue = futureValue;
        //    NumOfYears = numOfYears;
        //    PayFrequancy = payFreq;
        //    PaymentAmount = paymentAmount;
        //    RateOfIntrest = rateOfIntrest;
        //    DateOfFirstPayment = dateOfFirstPayment;
        //}
    }

}
