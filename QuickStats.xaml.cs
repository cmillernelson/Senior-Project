using System;
using System.Data;
using System.Data.SqlClient;
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

namespace OutdoorCenterRentals
{
    /// <summary>
    /// Interaction logic for QuickStats.xaml
    /// </summary>
    public partial class QuickStats : Window
    {
        private string connString = System.IO.File.ReadAllText(@"D:\Senior Project\ConnectionString.txt");

        public QuickStats()
        {
            InitializeComponent();
            getStats();
        }

        private void getStats()
        {
            string allOrders = "SELECT COUNT(*) FROM Orders";
            string totalItems = "SELECT quantity FROM Ordered_Items";
            string uniqueCustomers = "SELECT COUNT(DISTINCT customer_id) AS countUsers FROM Orders";

            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                using (MySqlCommand getAllOrders = new MySqlCommand(allOrders))
                {
                    try
                    {
                        getAllOrders.Connection = openCon;
                        openCon.Open();
                        using (MySqlDataReader reader = getAllOrders.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                rentalsLabel.Content += reader.GetString(0);
                            }
                        }
                        openCon.Close();
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                    }
                }

                using (MySqlCommand getTotalItems = new MySqlCommand(totalItems))
                {
                    try
                    {
                        getTotalItems.Connection = openCon;
                        openCon.Open();
                        using (MySqlDataReader reader = getTotalItems.ExecuteReader())
                        {
                            int quantityOrdered = 0;
                            while (reader.Read())
                            {
                                quantityOrdered += Convert.ToInt16(reader.GetString(0));
                            }
                            itemsLabel.Content += quantityOrdered.ToString();
                        }
                        openCon.Close();
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                    }
                }

                using (MySqlCommand getUniqueCustomers = new MySqlCommand(uniqueCustomers))
                {
                    try
                    {
                        getUniqueCustomers.Connection = openCon;
                        openCon.Open();
                        using (MySqlDataReader reader = getUniqueCustomers.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                customersLabel.Content += reader.GetString(0);
                            }
                        }
                        openCon.Close();
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                    }
                }
            }
        }

        private void okayButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
