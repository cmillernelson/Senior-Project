using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace OutdoorCenterRentals
{
    /// <summary>
    /// Interaction logic for Orders.xaml
    /// </summary>
    public partial class Orders : Window
    {

        // The string used to connect to the database. Edit for the location of the file in the machine running the program.
        private string connString = System.IO.File.ReadAllText(@"D:\Senior Project\ConnectionString.txt");

        private MainWindow main;
        private DataTable orders;

        public Orders(MainWindow main)
        {
            InitializeComponent();
            this.main = main;
            string searchBy = "SELECT order_num, name, customer_id, order_status, start_date, end_date FROM Orders WHERE order_status IN ('Reserved', 'Picked Up', 'Returned')";
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                using (MySqlCommand populateTable = new MySqlCommand(searchBy))
                {
                    try
                    {
                        populateTable.Connection = openCon;
                        orders = new DataTable();
                        openCon.Open();
                        MySqlDataAdapter dataAdaptor = new MySqlDataAdapter(populateTable);
                        dataAdaptor.Fill(orders);
                        ordersDataGrid.ItemsSource = orders.DefaultView;
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                    }
                }
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchBy = "SELECT order_num, name, customer_id, order_status, start_date, end_date FROM Orders WHERE ";
            if (orderNumRadioButton.IsChecked == true)
            {
                searchBy += "order_num = " + searchTextBox.Text;
            }
            if (nameRadioButton.IsChecked == true)
            {
                searchBy += "name LIKE '" + searchTextBox.Text + "' ";
            }
            if (idRadioButton.IsChecked == true)
            {
                searchBy += "customer_id = " + searchTextBox.Text;
            }
            if (reservedCheckBox.IsChecked == true || pickedUpCheckBox.IsChecked == true || returnedCheckBox.IsChecked == true || completedCheckBox.IsChecked == true)
            {
                Boolean moreChecks = false;
                searchBy += " AND order_status IN (";
                if (reservedCheckBox.IsChecked != true)
                {
                    searchBy += "'Reserved'";
                    moreChecks = true;
                }
                if (pickedUpCheckBox.IsChecked != true)
                {
                    if (moreChecks)
                    {
                        searchBy += ",";
                    }
                    searchBy += "'Picked Up'";
                    moreChecks = true;
                }
                if (returnedCheckBox.IsChecked != true)
                {
                    if (moreChecks)
                    {
                        searchBy += ",";
                    }
                    searchBy += "'Returned'";
                    moreChecks = true;
                }
                if (completedCheckBox.IsChecked != true)
                {
                    if (moreChecks)
                    {
                        searchBy += ",";
                    }
                    searchBy += "'Completed'";
                }
                searchBy += ")";
            }
            // Query has now been generated
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                using (MySqlCommand populateTable = new MySqlCommand(searchBy))
                {
                    try
                    {
                        // Clear the dataGrid
                        orders.Clear();
                        ordersDataGrid.Items.Refresh();
                        populateTable.Connection = openCon;
                        openCon.Open();
                        MySqlDataAdapter dataAdaptor = new MySqlDataAdapter(populateTable);
                        dataAdaptor.Fill(orders);
                        ordersDataGrid.ItemsSource = orders.DefaultView;
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                    }
                }
            }
            // End of searchButton_Click
        }

        private void openOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // This might not be necessary if the code beneath works, but it can't hurt to copy/paste it here as well
            DataRowView selectedRow = (DataRowView)ordersDataGrid.SelectedItem;
            String orderNum = (selectedRow[0]).ToString();
            main.orderNumberTextBox.Text = orderNum;
            main.querySuccessful = true;
            this.Close();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)ordersDataGrid.SelectedItem;
            String orderNum = (selectedRow[0]).ToString();
            main.orderNumberTextBox.Text = orderNum;
            main.querySuccessful = true;
            this.Close();
        }
    }
}
