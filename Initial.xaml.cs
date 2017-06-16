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
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Microsoft.VisualBasic;

namespace OutdoorCenterRentals
{
    /// <summary>
    /// Interaction logic for Initial.xaml
    /// </summary>
    public partial class Initial : Window
    {

        // The string used to connect to the database. Edit for the location of the file in the machine running the program.
        private string connString = System.IO.File.ReadAllText(@"D:\Senior Project\ConnectionString.txt");
        private MainWindow main;
        private int orderNumber;
        private string actionPerformed;
        public bool initialsEntered = false;

        public Initial(int orderNumber, string actionPerformed, MainWindow main)
        {
            this.main = main;
            this.orderNumber = orderNumber;
            this.actionPerformed = actionPerformed;
            InitializeComponent();
            initialsTextBox.Focus();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (initialsTextBox.Text.Equals(""))
            {
                initialsLabel.Content = "Please enter initials";
            }
            else
            {
                DateTime dateTime = DateTime.Now;
                dateTime.ToString("yyyy-MM-dd H:mm:ss");
                using (MySqlConnection openCon = new MySqlConnection(connString))
                {
                    string newCust = "INSERT INTO Order_History (order_num, action_performed, time_performed, initials) VALUES (@order_num, @action_performed, @time_performed, @initials)";
                    using (MySqlCommand queryNewCust = new MySqlCommand(newCust))
                    {
                        try
                        {
                            queryNewCust.Connection = openCon;
                            queryNewCust.Parameters.Add("@order_num", MySqlDbType.VarChar, 9).Value = orderNumber;
                            queryNewCust.Parameters.Add("@action_performed", MySqlDbType.VarChar, 20).Value = actionPerformed;
                            queryNewCust.Parameters.Add("@time_performed", MySqlDbType.DateTime).Value = dateTime;
                            queryNewCust.Parameters.Add("@initials", MySqlDbType.VarChar, 3).Value = initialsTextBox.Text;
                            openCon.Open();
                            queryNewCust.ExecuteNonQuery();
                            openCon.Close();
                            main.initialsEntered = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("An error occurred: '{0}'", ex);
                        }
                    }
                }
                this.Close();
            }
        }
    }
}
