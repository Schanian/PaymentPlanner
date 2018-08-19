using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculations
{
    public class CalculationException : System.ApplicationException
    {
        //static readonly int ErrorCodePlannedPaymentAmountTooLess = 1;
        public static string ErrorPlannedPaymentAmountTooLess = string.Format("Planned payment amount is too less to pay this amount.");
        public CalculationException(string message)
            : base(message)
        { 
        
           
        }
    }
}
