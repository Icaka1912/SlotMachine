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
using System.Windows.Shapes;

namespace SlotGame
{
    public partial class BalanceWindow : Window
    {
  
        public decimal Balance { get; private set; }

        public BalanceWindow()
        {
            InitializeComponent();
        }


        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
 
            if (decimal.TryParse(BalanceTextBox.Text, out decimal balance))
            {

                Balance = balance;
                this.DialogResult = true; 
                this.Close();
            }
            else
            {
              
                MessageBox.Show("Please enter a valid balance amount.", "Invalid entry", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}