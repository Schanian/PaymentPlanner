using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculations
{
    public class Calculation
    {
        public CalculationParameter Parameter { get; set; }
        public delegate void ReadInput(Calculation calculation);
        public event ReadInput ReadInputParameters;

        public enum CalculationMethod
        {
            None,
            PaymentAmountRateOfIntrest,
            NumOfPayRateOfIntrest,
            NumOfPayPaymentAmount
        }

        public enum CalculationProcessor
        {
            Future,
            Year,
            Both
        }

        public Calculation()
        {
            Parameter = new CalculationParameter();
        }
        public void ReadParameters()
        {
            ReadInputParameters(this);
        }
        public enum Frequancy
        {
            Daily = 1, Weekly = 7, BiWeekly = 14, HalfMonthly = 15, Monthly = 30, BiMonthly = 61, Quaterly = 91, SemiAnnual = 182, Annual = 365
        };

        public List<object> FindLoanRePaymentPlan(CalculationMethod method)
        {
            //return BuildPlan(true, method);
                      return BuildPlanForSavingRepaying_ByNumberOfPayment_RateOfIntrest(true, method);
        }

        public List<object> FindSavingPaymentPlan(CalculationMethod method)//'(decimal presentValue, decimal futureValue, decimal rateOfIntrest, ushort numOfYears, DateTime dateOfFirstPayment, Frequancy payFreq, decimal paymentAmount)
        {

            //if (Parameter.PresentValue < 0)
            //    Parameter.PresentValue = -Parameter.PresentValue;

           // return BuildPlanForSavingRepaying_ByPaymentAmount_RateOfIntrest(false, method);
            return BuildPlanForSavingRepaying_ByNumberOfPayment_RateOfIntrest(false, method);
        }

        /// <summary>
        /// BuildPlan is the Core Plan Generation Logic for Loan Repayment and Saving
        /// </summary>
        /// <param name="isLoanRepayment"></param>
        /// <returns></returns>
        List<object> BuildPlan(bool isLoanRepayment, CalculationMethod method) 
        {
            DateTime dateOfNextPay = Parameter.DateOfFirstPayment;
            DateTime dateOfStart = Parameter.DateOfFirstPayment.AddYears(Parameter.NumOfPayments.Value);
            List<object> listOfData = new List<object>();
            decimal totalInterest = 0;

            if (Parameter.NumOfPayments.HasValue && Parameter.NumOfPayments>0)
            {
                dateOfStart = Parameter.DateOfFirstPayment.AddDays((Parameter.NumOfPayments.Value -1) * (int)Parameter.PayFrequancy);
            }
            int seq = 1;
            if (!Parameter.PaymentAmount.HasValue || Parameter.PaymentAmount == 0)
                Parameter.PaymentAmount = (Parameter.PresentValue / Parameter.NumOfPayments);

            decimal roiInterest = Parameter.RateOfIntrest.Value / (decimal)100.0;
            decimal remainingBalance = Parameter.PresentValue;

            decimal intrestAmount = InterestCalculator_RateOfIntrest(roiInterest, remainingBalance, (int)Parameter.PayFrequancy);
            if (intrestAmount > Parameter.PaymentAmount.Value)
            {
                throw new CalculationException(CalculationException.ErrorPlannedPaymentAmountTooLess);
            }

            decimal totalAmount = 0;
            for (; remainingBalance > Parameter.FutureValue 
                    || (method == CalculationMethod.NumOfPayRateOfIntrest &&  dateOfNextPay <= dateOfStart) 
                    || (Parameter.NumOfPayments != 0 && Parameter.NumOfPayments >= seq); seq++)
            {
                intrestAmount = InterestCalculator_RateOfIntrest(roiInterest, remainingBalance,dateOfStart,dateOfNextPay);

                decimal effectivePayment = Math.Round(Parameter.PaymentAmount.Value - intrestAmount, 2);

                if (method == CalculationMethod.NumOfPayRateOfIntrest && effectivePayment > remainingBalance)//To find EMI we need to add payment amount and interst on balance.
                {
                    effectivePayment = Parameter.PaymentAmount.Value + intrestAmount;
                }
                //To avoid too much amount on last payment day we need to find the maximum payment amount.
                else if (effectivePayment > remainingBalance)
                {
                    effectivePayment = remainingBalance + intrestAmount;
                    remainingBalance = 0;
                }
                else 
                    remainingBalance -= effectivePayment;               
  
                totalInterest += Math.Round(intrestAmount,2);

                totalAmount -= effectivePayment;


                Payment payment = new Payment(seq, effectivePayment + intrestAmount, remainingBalance, dateOfNextPay, totalAmount, intrestAmount, effectivePayment, totalInterest, totalAmount);
               // payment.PresentValue = remainingBalance.ToString("C2");
                 
                //payment.Seq = seq.ToString();
                //payment.NextPayDate = dateOfNextPay.ToShortDateString();

                //payment.InterestPaid = interestAmount.ToString("C2");
                //payment.EffectivePayment = effectivePayment.ToString("C2");
                //payment.RemainingAmount = (remainingBalance != 0) ? (remainingBalance -= effectivePayment).ToString("C2") : 0.ToString("C2");
                //payment.TotalInterest = totalInterest.ToString("C2");
               

                if (Parameter.PayFrequancy < Frequancy.Monthly)
                    dateOfNextPay = dateOfNextPay.AddDays((int)Parameter.PayFrequancy);
                else
                    dateOfNextPay = dateOfNextPay.AddMonths(((int)Parameter.PayFrequancy) / 30);
                listOfData.Add(payment);
            }
            
            if (method == CalculationMethod.NumOfPayRateOfIntrest)
            {
                FindTheExactEMI(ref listOfData, totalAmount);
            }

            return listOfData;
        }

        /// <summary>
        /// BuildPlan is a working method the Plan Generation Logic for Loan Repayment and Saving when Payment Amount and Interest are known along with a Future Value
        /// </summary>
        /// <param name="isLoanRepayment"></param>
        /// <returns></returns>
        List<object> BuildPlanForSavingRepaying_ByPaymentAmount_RateOfIntrest(bool isLoanRepayment, CalculationMethod method)
        {
            DateTime nextPayDate = Parameter.DateOfFirstPayment;
            DateTime dateOfLastPayment = Parameter.DateOfFirstPayment.AddYears(Parameter.NumOfPayments.Value);
            List<object> listOfData = new List<object>();
            decimal totalInterest = 0;
            decimal targetValue = (Parameter.FutureValue > 0) ? Parameter.FutureValue : Parameter.PresentValue;
            
            if (Parameter.NumOfPayments.HasValue && Parameter.NumOfPayments > 0)
            {
                dateOfLastPayment = Parameter.DateOfFirstPayment.AddDays((Parameter.NumOfPayments.Value - 1) * (int)Parameter.PayFrequancy);
            }
            
            if (!Parameter.PaymentAmount.HasValue || Parameter.PaymentAmount == 0)
                Parameter.PaymentAmount = ((targetValue - Parameter.PresentValue) / Parameter.NumOfPayments);

            decimal roiInterest = Parameter.RateOfIntrest.Value / (decimal)100.0;
            decimal remainingBalance = Parameter.FutureValue - Parameter.PresentValue;
            decimal requiredPayment = Parameter.PaymentAmount.Value;
            decimal intrestAmount = InterestCalculator_RateOfIntrest(roiInterest, remainingBalance, (int)Parameter.PayFrequancy);
            if (intrestAmount > Parameter.PaymentAmount.Value)
            {
                throw new CalculationException(CalculationException.ErrorPlannedPaymentAmountTooLess);
            }

            decimal totalAmount = Parameter.PresentValue;
            int seq = 0;
            DateTime previousPaymentDate = DateTime.Today;

            while (true)
            {
                seq++;
                if(method == CalculationMethod.NumOfPayRateOfIntrest)
                {
                    if(seq > Parameter.NumOfPayments   ) break;
                    if(nextPayDate > dateOfLastPayment ) break;
                }
                else if (totalAmount >= Parameter.FutureValue) break;

                
                //interestAmount = InterestCalculator_RateOfIntrest(roiInterest, remainingBalance, (int)Parameter.PayFrequancy);
                intrestAmount = InterestCalculator_RateOfIntrest(roiInterest, totalAmount, previousPaymentDate, nextPayDate);
               
                decimal effectivePayment = 0.00M;


                if (isLoanRepayment)
                    effectivePayment = Math.Round(Parameter.PaymentAmount.Value - intrestAmount, 2);
                else
                    effectivePayment = Math.Round(Parameter.PaymentAmount.Value + intrestAmount, 2);
                    //effectivePayment = Math.Round(Parameter.PaymentAmount.Value + interestAmount, 2);

                if (Parameter.FutureValue != 0 && totalAmount + requiredPayment + intrestAmount > Parameter.FutureValue)
                    requiredPayment = Parameter.FutureValue - totalAmount - intrestAmount;

                totalInterest += Math.Round(intrestAmount, 2);
                totalAmount = totalAmount + requiredPayment + intrestAmount;

                //Payment payment = new Payment(seq, effectivePayment - interestAmount, remainingBalance -totalAmount, nextPayDate, Parameter.FutureValue - totalAmount, interestAmount, effectivePayment, totalInterest, totalAmount);

                Payment payment = new Payment(seq, requiredPayment, Parameter.FutureValue - totalAmount, nextPayDate, Parameter.FutureValue - totalAmount, intrestAmount, requiredPayment + intrestAmount, totalInterest, totalAmount);

                previousPaymentDate = nextPayDate;

                if (Parameter.PayFrequancy < Frequancy.Monthly)
                    nextPayDate = nextPayDate.AddDays((int)Parameter.PayFrequancy);
                else
                    nextPayDate = nextPayDate.AddMonths(((int)Parameter.PayFrequancy) / 30);

            listOfData.Add(payment);
            }

            //if (method == CalculationMethod.NumOfPayRateOfIntrest)
            //{
            //    FindTheExactEMI(ref listOfData, totalAmount);
            //}
            Parameter.FutureValue = totalAmount;
            return listOfData;
        }


        /// <summary>
        /// BuildPlan is the Core Plan Generation Logic for Loan Repayment and Saving when number of payment is known but amount not known,
        /// </summary>
        /// <param name="isLoanRepayment"></param>
        /// <returns></returns>
        List<object> BuildPlanForSavingRepaying_ByNumberOfPayment_RateOfIntrest(bool isLoanRepayment, CalculationMethod method)
        {
            DateTime nextPayDate = Parameter.DateOfFirstPayment;
            DateTime dateOfLastPayment = Parameter.DateOfFirstPayment.AddYears(Parameter.NumOfPayments.Value);
            List<object> listOfData = new List<object>();
            decimal totalInterest = 0;
            decimal targetValue = 0;
            decimal estimatedPayment = 0;

            if (isLoanRepayment)
                targetValue = (Parameter.FutureValue > 0) ? Parameter.FutureValue + Parameter.PresentValue : 0;
            else
                targetValue = (Parameter.FutureValue > 0) ? Parameter.FutureValue - Parameter.PresentValue : 0;


            if (Parameter.NumOfPayments.HasValue && Parameter.NumOfPayments > 0)
            {
                if (!Parameter.PaymentAmount.HasValue || Parameter.PaymentAmount == 0)
                {
                    //Parameter.PaymentAmount = ((targetValue - Parameter.PresentValue) / Parameter.NumOfPayments);
                    if (targetValue > 0)
                        estimatedPayment = targetValue / Parameter.NumOfPayments.Value;
                    else
                        estimatedPayment = Parameter.PresentValue / Parameter.NumOfPayments.Value;

                    Parameter.PaymentAmount = estimatedPayment;
                }

                if (estimatedPayment < 0) estimatedPayment = (-1) * estimatedPayment;

                dateOfLastPayment = Parameter.DateOfFirstPayment.AddDays((Parameter.NumOfPayments.Value - 1) * (int)Parameter.PayFrequancy);
            }

            decimal roiInterest = Parameter.RateOfIntrest.Value / (decimal)100.0;
            decimal remainingBalance = Parameter.FutureValue - Parameter.PresentValue;
            decimal requiredPayment = Parameter.PaymentAmount.Value;
            decimal interestAmount = InterestCalculator_RateOfIntrest(roiInterest, remainingBalance, (int)Parameter.PayFrequancy);

            if (interestAmount > Parameter.PaymentAmount.Value)
            {
                throw new CalculationException(CalculationException.ErrorPlannedPaymentAmountTooLess);
            }

            //decimal totalAmount = (isLoanRepayment) ? 0 - Parameter.PresentValue : Parameter.PresentValue;
            decimal totalAmount = isLoanRepayment ? 0 - Parameter.PresentValue : Parameter.PresentValue;
            int seq = 0;
            DateTime previousPaymentDate = DateTime.Today;

            while (true)
            {
                seq++;
                if(totalAmount >= targetValue ||( Parameter.NumOfPayments.Value >0 && seq > Parameter.NumOfPayments) )
                    break;

                interestAmount = InterestCalculator_RateOfIntrest(roiInterest, totalAmount, previousPaymentDate, nextPayDate);

                decimal effectivePayment = 0.00M;

                if (isLoanRepayment)
                {
                    if (Parameter.PaymentAmount.HasValue)
                        requiredPayment = Math.Round(Parameter.PaymentAmount.Value, 2);
                    else
                        requiredPayment = estimatedPayment + interestAmount;

                    if ((totalAmount + requiredPayment) > targetValue)
                        requiredPayment = targetValue - totalAmount + interestAmount;
                    //else
                    //    requiredPayment = Parameter.PaymentAmount.Value - interestAmount;
                }
                else
                {
                    if (Parameter.PaymentAmount.HasValue)
                        requiredPayment = Math.Round(Parameter.PaymentAmount.Value, 2);
                    else
                        requiredPayment = estimatedPayment + interestAmount; 

                    if ((totalAmount + requiredPayment) > targetValue)
                        requiredPayment = targetValue - totalAmount + interestAmount;
                    //else
                    //    requiredPayment = Parameter.FutureValue - totalAmount - interestAmount ;
                }

                totalInterest += Math.Round(interestAmount, 2);
                totalAmount = Math.Round(totalAmount + requiredPayment + interestAmount,2);

                Payment payment = new Payment(seq, requiredPayment, totalAmount , nextPayDate, Parameter.FutureValue - totalAmount, interestAmount, requiredPayment + interestAmount, totalInterest, totalAmount);

                previousPaymentDate = nextPayDate;

                if (Parameter.PayFrequancy < Frequancy.Monthly)
                    nextPayDate = nextPayDate.AddDays((int)Parameter.PayFrequancy);
                else
                    nextPayDate = nextPayDate.AddMonths(((int)Parameter.PayFrequancy) / 30);

                listOfData.Add(payment);
            }

            Parameter.FutureValue = totalAmount;
            return listOfData;
        }





        /// <summary>
        /// FindTheExactEMI - Method to calculate EMI when user want to find payment with  Number of Payments and Rate Of Intrest
        /// </summary>
        /// <param name="listOfData"></param>
        /// <param name="totalAmount"></param>
        void FindTheExactEMI(ref List<object> listOfData, decimal totalAmount)
        {
            //Logic to calculate EMI when user want to find payment with  Number of Payments and Rate Of Intrest
            decimal effectivePayment = totalAmount / Parameter.NumOfPayments.Value;
            Parameter.PaymentAmount = effectivePayment;
            foreach (Payment payment in listOfData)
            {
                payment.EffectivePayment = effectivePayment.ToString("C2");
            }
        }

//        /// <summary>
//        /// BuildPlan is the Core Plan Generation Logic for Loan Repayment and Saving
//        /// </summary>
//        /// <param name="isLoanRepayment"></param>
//        /// <returns></returns>
//        List<object> BuildPlan_EMI_NOPROI(bool isLoanRepayment, CalculationMethod method)
//        {
//            DateTime dateOfNextPay = Parameter.DateOfFirstPayment;
//            DateTime dateOfStart = Parameter.DateOfFirstPayment.AddYears(Parameter.NumOfPayments.Value);
//            List<object> listOfData = new List<object>();
//            decimal totalInterest = 0;
//            dateOfStart = Parameter.DateOfFirstPayment.AddDays((Parameter.NumOfPayments.Value - 1) * (int)Parameter.PayFrequancy);
//            int seq = 1;
//            if (!Parameter.PaymentAmount.HasValue || Parameter.PaymentAmount == 0)
//                Parameter.PaymentAmount = (Parameter.PresentValue / Parameter.NumOfPayments);

//            decimal totalAmount = 0;
//            for (decimal remainingBalance = Parameter.PresentValue; (remainingBalance > Parameter.FutureValue || dateOfNextPay <= dateOfStart) || (Parameter.NumOfPayments != 0 && Parameter.NumOfPayments > seq); seq++)
//            {
//                Payment payment = new Payment();
//                decimal roiInterest = Parameter.RateOfIntrest.Value / (decimal)100.0;
//                decimal interstAmount = InterestCalculator_RateOfIntrest(roiInterest, remainingBalance, (int)Parameter.PayFrequancy);
//                decimal effectivePayment;//= Parameter.PaymentAmount.Value + interstAmount;
//                ////////if (method == CalculationMethod.NOPPA || method == CalculationMethod.NumOfPayRateOfIntrest)
//                //    effectivePayment = Parameter.PaymentAmount.Value - interstAmount;
//                //else
//                effectivePayment = Parameter.PaymentAmount.Value - interstAmount;
//                payment.PresentValue = remainingBalance.ToString("C2");
//                if (effectivePayment > remainingBalance)
//                {
//                    effectivePayment = remainingBalance + interstAmount;
//                    remainingBalance = 0;
//                }
//                if (method == CalculationMethod.NumOfPayRateOfIntrest)
//                {
//                    effectivePayment = Parameter.PaymentAmount.Value + interstAmount;
//                }

//                totalInterest += interstAmount;

//                payment.Seq = seq.ToString();
//                payment.NextPayDate = dateOfNextPay.ToShortDateString();

//                payment.InterestPaid = interstAmount.ToString("C2");
//                payment.EffectivePayment = effectivePayment.ToString("C2");
//                payment.RemainingAmount = (remainingBalance != 0) ? (remainingBalance -= effectivePayment).ToString("C2") : 0.ToString("C2");
//                payment.TotalInterest = totalInterest.ToString("C2");
//                //totalAmount += (effectivePayment + interstAmount); 
//                totalAmount += effectivePayment;

//                #region commented
//                //payment.NextPayDate = dateOfNextPay;//.ToShortDateString();
//                //payment.PresentValue = remainingBalance;//.ToString("C2")
//                //payment.InterestPaid = interstAmount;//.ToString("C2")
//                //payment.EffectivePayment = effectivePayment;//.ToString("C2")
//                //payment.RemainingAmount = (remainingBalance += effectivePayment);//.ToString("C2")
//                //payment.TotalInterest = totalInterest;//.ToString("C2")

//                //var objValue = new { NextPayDate = dateOfNextPay.ToShortDateString(), PresentValue = remainingBalance.ToString("C2"), InterestPaid = interstAmount.ToString("C2"), EffectivePayment = effectivePayment.ToString("C2"), RemainingAmount = (remainingBalance += effectivePayment).ToString("C2"), TotalInterest = totalInterest.ToString("C2") };
//                #endregion commented
//                if (Parameter.PayFrequancy < Frequancy.Monthly)
//                    dateOfNextPay = dateOfNextPay.AddDays((int)Parameter.PayFrequancy);
//                else
//                    dateOfNextPay = dateOfNextPay.AddMonths(((int)Parameter.PayFrequancy) / 30);
//                listOfData.Add(payment);
//            }
//            if (method == CalculationMethod.NumOfPayRateOfIntrest)
//            {
//                decimal effectivePayment = totalAmount / Parameter.NumOfPayments.Value;
//                foreach (Payment payment in listOfData)
//                {
//                    payment.EffectivePayment = effectivePayment.ToString("C2");
//                }
//            }
//            return listOfData;
//        }
//}



        /// <summary>
        /// InterestCalculator_RateOfIntrest
        /// </summary>
        /// <param name="roiInterest"></param>
        /// <param name="balance"></param>
        /// <param name="daysInBillingCycle"></param>
        /// <returns></returns>
        decimal InterestCalculator_RateOfIntrest(decimal roiInterest, decimal balance, int daysInBillingCycle)
        {
            return (roiInterest * balance * daysInBillingCycle / 365);
        }

        /// <summary>
        /// InterestCalculator_RateOfIntrest
        /// </summary>
        /// <param name="roiInterest"></param>
        /// <param name="balance"></param>
        /// <param name="daysInBillingCycle"></param>
        /// <returns></returns>
        decimal InterestCalculator_RateOfIntrest(decimal roiInterest, decimal balance, DateTime lastPaymentDate, DateTime thisPaymentDate)
        {
            TimeSpan daysInBillingCycle = thisPaymentDate - lastPaymentDate;
            return (roiInterest * balance * (int) daysInBillingCycle.TotalDays / 365);
        }

        ///// <summary>
        ///// BuildPlan is the Core Plan Generation Logic for Loan Repayment and Saving
        ///// </summary>
        ///// <param name="isLoanRepayment"></param>
        ///// <returns></returns>
        //List<object> BuildPlan_PaymentAmount(bool isLoanRepayment,CalculationMethod method)
        //{
        //    DateTime dateOfNextPay = Parameter.DateOfFirstPayment;
        //    DateTime dateOfStart = DateTime.Today;
            
        //    List<object> listOfData = new List<object>();
        //    decimal totalInterest = 0;
        //    decimal year = 360;
        //    //if (Parameter.PayFrequancy < Frequancy.Monthly)
        //    //{
        //        dateOfStart = Parameter.DateOfFirstPayment.AddDays(Parameter.NumOfYears.Value * (int)Parameter.PayFrequancy);
 
        //        //Parameter.PaymentAmount = Parameter.PresentValue / (decimal)Parameter.PayFrequancy;
        //        Parameter.PaymentAmount = -(Parameter.PresentValue / Parameter.NumOfYears );// (year / (int)Parameter.PayFrequancy);
        //    //}
        //    //else
        //    //{
        //    //    Parameter.PaymentAmount = -(Parameter.PresentValue / Parameter.NumOfYears); // (year / (int)Parameter.PayFrequancy);
        //    //    //Parameter.PaymentAmount = Parameter.PresentValue / (decimal)Parameter.PayFrequancy
        //    //}

        //        decimal totalAmount = 0;
        //    for (decimal remainingBalance = Parameter.PresentValue; remainingBalance < Parameter.FutureValue || dateOfNextPay <= dateOfStart; )
        //    {
                
        //        decimal roiInterest = Parameter.RateOfIntrest.Value / (decimal)100.0;
        //        decimal interstAmount = InterestCalculator_RateOfIntrest(roiInterest, remainingBalance, (int)Parameter.PayFrequancy);
        //        decimal effectivePayment = 0;
                
        //        if(method == CalculationMethod.PAROI)
        //            effectivePayment = Parameter.PaymentAmount.Value - interstAmount;
        //        else
        //            effectivePayment = Parameter.PaymentAmount.Value + interstAmount;

        //        totalInterest += interstAmount;
        //        Payment payment = new Payment();
        //        payment.NextPayDate = dateOfNextPay.ToShortDateString();
        //        payment.PresentValue = remainingBalance.ToString("C2");
        //        payment.InterestPaid = interstAmount.ToString("C2");
        //        payment.EffectivePayment = effectivePayment.ToString("C2");
        //        payment.RemainingAmount = (remainingBalance += effectivePayment).ToString("C2");
        //        payment.TotalInterest = totalInterest.ToString("C2");
        //        totalAmount += effectivePayment;
        //        if (Parameter.PayFrequancy < Frequancy.Monthly)
        //            dateOfNextPay = dateOfNextPay.AddDays((int)Parameter.PayFrequancy);
        //        else
        //            dateOfNextPay = dateOfNextPay.AddMonths(((int)Parameter.PayFrequancy) / 30);
        //        //listOfData.Add(objValue);
        //        listOfData.Add(payment);
        //    }

        //    if (method == CalculationMethod.PAROI)
        //    {
        //        decimal effectivePayment = totalAmount / Parameter.NumOfYears.Value;
        //        decimal total = 0;
        //        foreach (Payment payment in listOfData)
        //        {
        //            payment.EffectivePayment = effectivePayment.ToString("C2");  
        //        }
        //    }

        //    return listOfData;
        //}

        //decimal InterestCalculator_PaymentAmount(decimal roiInterest, decimal balance, int daysInBillingCycle)
        //{
        //    return (roiInterest * Math.Round(balance, 2) * daysInBillingCycle / 365);
        //}
    }
}