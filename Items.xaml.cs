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
using MySql.Data.MySqlClient;
using System.Data;

namespace OutdoorCenterRentals
{
    /// <summary>
    /// Interaction logic for Items.xaml
    /// </summary>
    public partial class Items : Window
    {

        // The string used to connect to the database. Edit for the location of the file in the machine running the program.
        private string connString = System.IO.File.ReadAllText(@"D:\Senior Project\ConnectionString.txt");
        private DataTable addedItems;
        private DataTable items;

        MainWindow main;

        public Items(MainWindow main)
        {
            this.main = main;
            addedItems = new DataTable();
            addedItems.Columns.Add("order_num", typeof(int));
            addedItems.Columns.Add("item_id", typeof(int));
            addedItems.Columns.Add("item_name", typeof(string));
            addedItems.Columns.Add("replacement_cost", typeof(float));
            addedItems.Columns.Add("quantity", typeof(int));
            InitializeComponent();

            string searchBy = "SELECT category, subcategory, item_id, item_name, replacement_cost, quantity_remaining, quantity_rented FROM Inventory";
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                using (MySqlCommand populateTable = new MySqlCommand(searchBy))
                {
                    try
                    {
                        populateTable.Connection = openCon;
                        items = new DataTable();
                        openCon.Open();
                        MySqlDataAdapter dataAdaptor = new MySqlDataAdapter(populateTable);
                        dataAdaptor.Fill(items);
                        itemsDataGrid.ItemsSource = items.DefaultView;
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                    }
                }
            }
        }



        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRow row in addedItems.Rows)
            {
                // Insert ordered items into the database
                using (MySqlConnection openCon = new MySqlConnection(connString))
                {
                    string addItemsString = "INSERT INTO Ordered_Items VALUES (@order_num, @item_id, @item_name, @replacement_cost, @quantity, 0)";
                    using (MySqlCommand addItemsQuery = new MySqlCommand(addItemsString))
                    {
                        addItemsQuery.Connection = openCon;
                        addItemsQuery.Parameters.Add("@order_num", MySqlDbType.Int16, 9).Value = main.orderNumberTextBox.Text;
                        addItemsQuery.Parameters.Add("@item_id", MySqlDbType.Int16, 10).Value = row[1];
                        addItemsQuery.Parameters.Add("@item_name", MySqlDbType.VarChar, 50).Value = row[2];
                        addItemsQuery.Parameters.Add("@replacement_cost", MySqlDbType.Float).Value = row[3];
                        addItemsQuery.Parameters.Add("@quantity", MySqlDbType.Int16, 10).Value = row[4];
                        try
                        {
                            openCon.Open();
                            addItemsQuery.ExecuteNonQuery();
                            openCon.Close();
                        }
                        catch (Exception ex)
                        {
                            openCon.Close();
                            Console.WriteLine("An error occurred: '{0}'", ex);
                        }
                    }
                }

                // Get current quantity remaining from inventory
                int quantityInInventory = 0;
                using (MySqlConnection openCon = new MySqlConnection(connString))
                {
                    string getRemainingItems = "SELECT quantity_remaining FROM Inventory WHERE item_id = @item_id";
                    using (MySqlCommand getRemainingItemsQuery = new MySqlCommand(getRemainingItems))
                    {
                        getRemainingItemsQuery.Connection = openCon;
                        getRemainingItemsQuery.Parameters.Add("@item_id", MySqlDbType.Int16, 10).Value = row[1];
                        try
                        {
                            openCon.Open();
                            using (MySqlDataReader reader = getRemainingItemsQuery.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    quantityInInventory = reader.GetInt16("quantity_remaining");
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

                // Adjust remaining quantity in inventory
                using (MySqlConnection openCon = new MySqlConnection(connString))
                {
                    string updateInventory = "UPDATE Inventory SET quantity_remaining = @quantity_remaining WHERE item_id = @item_id";
                    using (MySqlCommand updateInventoryQuery = new MySqlCommand(updateInventory))
                    {
                        updateInventoryQuery.Connection = openCon;
                        updateInventoryQuery.Parameters.Add("@quantity_remaining", MySqlDbType.Int16, 11).Value = quantityInInventory - Convert.ToInt16(row[4]);
                        updateInventoryQuery.Parameters.Add("@item_id", MySqlDbType.Int16, 10).Value = row[1];
                        try
                        {
                            openCon.Open();
                            updateInventoryQuery.ExecuteNonQuery();
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
            this.Close();
        }



        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // column 4 is quantity remaining
            // column 5 is quantity rented

            // Determine if there are any of these left
            DataRowView selectedRow = (DataRowView)itemsDataGrid.SelectedItem;
            // If not, complain about it in a message, and do nothing
            if (Convert.ToInt16(selectedRow[5]) == 0)
            {
                Message message = new Message("No more remaining");
                message.ShowDialog();
            }
            else
            {
                // Change the quantities seen
                int newRemaining = Convert.ToInt16(selectedRow[5]);
                newRemaining--;
                selectedRow[5] = newRemaining;
                int newRented = Convert.ToInt16(selectedRow[6]);
                newRented++;
                selectedRow[6] = newRented;
                // Change the unseen quantities
                Boolean added = false;
                int itemId = Convert.ToInt16(selectedRow[2]);
                foreach (DataRow row in addedItems.Rows)
                {
                    // If one or more of the same items has been added already, just adjust the quantity
                    if(Convert.ToInt16(row[1]) == itemId)
                    {
                        row[4] = newRented;
                        added = true;
                    }
                }
                // Otherwise add a new row for the new item
                // order#, item id, item name, replacement cost, quantity
                if (!added)
                {
                    int order_num = Convert.ToInt16(main.orderNumberTextBox.Text);
                    int item_id = Convert.ToInt16(selectedRow[2]);
                    string itemName = selectedRow[3].ToString();
                    float replacementCost = (float)Convert.ToDouble(selectedRow[4]);
                    addedItems.Rows.Add(order_num, itemId, itemName, replacementCost, 1);
                }
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchItems = "SELECT category, subcategory, item_id, item_name, replacement_cost, quantity_remaining, quantity_rented FROM Inventory WHERE ";
            if(CategoryRadioButton.IsChecked == true)
            {
                searchItems += "category LIKE '" + itemSearchTextBox.Text + "'";
            }
            if(subcategoryRadioButton.IsChecked == true)
            {
                searchItems += "subcategory LIKE '" + itemSearchTextBox.Text + "'";
            }
            if(itemNameRadioButton.IsChecked == true)
            {
                searchItems += "item_name LIKE '" + itemSearchTextBox.Text + "'";
            }

            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                using (MySqlCommand populateTable = new MySqlCommand(searchItems))
                {
                    try
                    {
                        // Clear the dataGrid
                        items.Clear();
                        itemsDataGrid.Items.Refresh();
                        populateTable.Connection = openCon;
                        openCon.Open();
                        MySqlDataAdapter dataAdaptor = new MySqlDataAdapter(populateTable);
                        dataAdaptor.Fill(items);
                        itemsDataGrid.ItemsSource = items.DefaultView;
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                    }
                }
            }
        }
    }
}
