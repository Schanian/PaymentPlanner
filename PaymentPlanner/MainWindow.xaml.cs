using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Calculations;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CustomInitialize();
            txtDateOfFirstPayment.Text = DateTime.Today.ToShortDateString();
            calculation = new Calculation();
            calculation.ReadInputParameters += calculation_ReadInputParameters; 
        }

        void calculation_ReadInputParameters(Calculation calculation)
        {
            decimal presentValue, futureValue;
            decimal rateOfIntrest;
            ushort numOfYears;
            DateTime dateOfFirstPayment;
            DateTime dateOfPreviousPayment;
            Calculation.Frequancy payFreq;
            decimal paymentAmount;

            object selectedItem = cboPayPeriod.SelectedItem;
            int freq = (int)((System.Windows.Controls.ComboBoxItem)selectedItem).Tag;
            payFreq = (Calculation.Frequancy)freq;
            decimal.TryParse(txtPV.Text, out presentValue);
            decimal.TryParse(txtFV.Text, out futureValue);
            decimal.TryParse(txtROI.Text, out rateOfIntrest);
            DateTime.TryParse(txtDateOfFirstPayment.Text, out dateOfFirstPayment);
            if (!DateTime.TryParse(txtDateOfStart.Text, out dateOfPreviousPayment))
                dateOfPreviousPayment = dateOfFirstPayment;
            ushort.TryParse(txtNumOfPayments.Text, out numOfYears);
            string pa = txtPaymentAmount.Text.Replace("$","");
            decimal.TryParse(pa, out paymentAmount);

            //calculation.Parameter.Set(presentValue, futureValue, rateOfIntrest, numOfYears, dateOfFirstPayment, payFreq, paymentAmount);
            calculation.Parameter.PresentValue = presentValue;
            calculation.Parameter.FutureValue = futureValue;
            calculation.Parameter.NumOfPayments = numOfYears;
            calculation.Parameter.PayFrequancy = payFreq;
            calculation.Parameter.PaymentAmount = paymentAmount;
            calculation.Parameter.RateOfIntrest = rateOfIntrest;
            calculation.Parameter.DateOfFirstPayment = dateOfFirstPayment;
            calculation.Parameter.EndCycleBilling = (chkEndCycleBill.IsChecked.HasValue && chkEndCycleBill.IsChecked.Value);
        }


        Calculation calculation;

        void CustomInitialize()
        {
            Calculation.Frequancy[] items = { Calculation.Frequancy.Daily, Calculation.Frequancy.Weekly, Calculation.Frequancy.BiWeekly, Calculation.Frequancy.HalfMonthly, Calculation.Frequancy.Monthly, Calculation.Frequancy.BiMonthly, Calculation.Frequancy.Quaterly, Calculation.Frequancy.SemiAnnual, Calculation.Frequancy.Annual };
     
            for(int i=0;i<items.Length;i++)
            {
                ComboBoxItem cbItem = new ComboBoxItem();
                cbItem.Tag = (int)items[i];
                cbItem.Content = items[i].ToString();
                cboPayPeriod.Items.Add(cbItem);
            }

            cboPayPeriod.SelectedIndex = 3; 
  
        }

        private void cboPayPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
        private void btnSavePlan_Click(object sender, RoutedEventArgs e)
        {
            //txtPV.Text = " 0";
            //txtFV.Text = "1000";
            //txtNumOfPayments.Text = "4";
            //txtPaymentAmount.Text = "";
            calculation.ReadParameters();

            if (this.txtNumOfPayments.Text == string.Empty && this.txtROI.Text == string.Empty && this.txtPaymentAmount.Text == string.Empty)
            {
                MessageBox.Show("At least, Any two of the following parameters are required 'Number of Year', 'Rate Of Interest', 'Payment Amount'");
                return;
            }
            else if (this.txtNumOfPayments.Text != string.Empty && this.txtROI.Text != string.Empty)
            {
                //Payment amount to find
                dgData.ItemsSource = calculation.FindSavingPaymentPlan(Calculation.CalculationMethod.NumOfPayRateOfIntrest);
            }
            else if (this.txtROI.Text != string.Empty && this.txtPaymentAmount.Text != string.Empty)
            {
                //Number of year to find
                dgData.ItemsSource = calculation.FindSavingPaymentPlan(Calculation.CalculationMethod.PaymentAmountRateOfIntrest);
            }
            else if (this.txtNumOfPayments.Text != string.Empty && this.txtPaymentAmount.Text != string.Empty)
            {
                //Rate of Interest to find

                dgData.ItemsSource = calculation.FindSavingPaymentPlan(Calculation.CalculationMethod.NumOfPayPaymentAmount);
            }

           // dgData.ItemsSource = calculation.FindSavingPaymentPlan  (Calculation.CalculationMethod.None);//(presentValue, futureValue, rateOfIntrest, numOfYears, dateOfFirstPayment, payFreq, paymentAmount);

        }


        private void btnPayPlan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               // txtPV.Text = " 1000";
               //txtFV.Text = "0";
               //// txtPaymentAmount.Text =" 270";
               // txtNumOfPayments.Text = "4";
                calculation.ReadParameters();

                if (this.txtNumOfPayments.Text == string.Empty && this.txtROI.Text == string.Empty && this.txtPaymentAmount.Text == string.Empty)
                {
                    MessageBox.Show("At least, Any two of the following parameters are required 'Number of Year', 'Rate Of Interest', 'Payment Amount'");
                    return;
                }
                else if (this.txtNumOfPayments.Text != string.Empty && this.txtROI.Text != string.Empty)
                {
                    //Payment amount to find
                    dgData.ItemsSource = calculation.FindLoanRePaymentPlan(Calculation.CalculationMethod.NumOfPayRateOfIntrest);
                }
                else if (this.txtROI.Text != string.Empty && this.txtPaymentAmount.Text != string.Empty)
                {
                    //Number of year to find
                    dgData.ItemsSource = calculation.FindLoanRePaymentPlan(Calculation.CalculationMethod.PaymentAmountRateOfIntrest);
                }
                if (this.txtNumOfPayments.Text != string.Empty && this.txtPaymentAmount.Text != string.Empty)
                {
                    //Rate of Interest to find

                    dgData.ItemsSource = calculation.FindLoanRePaymentPlan(Calculation.CalculationMethod.NumOfPayPaymentAmount);
                }

                txtPaymentAmount.Text = calculation.Parameter.PaymentAmount.Value.ToString("C2");

            }
            catch(CalculationException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (SystemException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void txtPV_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void txtROI_TargetUpdated(object sender, DataTransferEventArgs e)
        {

        }


      
//        void createGrid()
//        {
//            this.grdResult.ColumnDefinitions.Add(new ColumnDefinition());
//            this.grdResult.ColumnDefinitions.Add(new ColumnDefinition());
//            this.grdResult.ColumnDefinitions.Add(new ColumnDefinition());
//            this.grdResult.ColumnDefinitions.Add(new ColumnDefinition());

//            string[] values = {"Date Of Next Pay","Payment Amt","Interest","Total Paid","Date Of Next Pay"}
//            this.grdResult.RowDefinitions.Add(values,0,4));
          
//          this.grdResult.Visibility = System.Windows.Visibility.Visible;
//        }

//        RowDefinition CreateNewRow(string[] strTitles,int rowID,int columnCount)
//        {
//            //RowDefinition row = new RowDefinition();
//            //for (int i = 0; i < columnCount; i++)
//            //{
//            //     row. CreateCell(strTitles[i],  rowID, i)
//            //}

//           return row;
//        }

//        TextBox CreateCell(string strTitle, int rowID, int columnID)
//        {
//            TextBox txtBlock = new TextBox();
//            txtBlock.Text = strTitle;
//            Grid.SetRow(txtBlock, rowID);
//            Grid.SetColumn(txtBlock, columnID);
//            return txtBlock;
//        }

//       // decimal Calculate()
      }
}
