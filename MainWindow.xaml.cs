// TODO: Stop working on the little stuff like error checking. It will work if you do it right during the demo.
// Start actually working on import scripts and order and item selection.

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
using Microsoft.VisualBasic;
using System.ComponentModel;

namespace OutdoorCenterRentals
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // The string used to connect to the database. This will not be the same if the database is migrated to the university servers.
        private string connString = System.IO.File.ReadAllText(@"D:\Senior Project\ConnectionString.txt");
        private string inventoryLocation = "D:\\Senior Project\\InventoryWithoutSerials.txt";
        private string customerLocation = "D:\\Senior Project\\Customers.csv";
        public Boolean querySuccessful = false;
        public Boolean initialsEntered = false;
        public DataTable itemsOrdered;
        private readonly BackgroundWorker worker;
        //public string orderNumber = "";

        public MainWindow()
        {
            InitializeComponent();
            // These methods do almost the same thing, so can't I condense them into one or two? -- Not if I want these actions to be initialed. Best leave them seperate to be easily modded.
            customerNotesTextBox.LostFocus += new RoutedEventHandler(customerNotesTextBox_LostFocus);
            orderNotesTextBox.LostFocus += new RoutedEventHandler(orderNotesTextBox_LostFocus);
            startDatePicker.LostFocus += new RoutedEventHandler(startDatePicker_LostFocus);
            endDatePicker.LostFocus += new RoutedEventHandler(endDatePicker_LostFocus);
            itemsOrdered = new DataTable();
            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }


        /*   User interactions with the form   */


        // When the Add button is clicked
        private void newCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            // Edit the form
            if (newCustomerButton.Content.Equals("New"))
            {
                newCustomerButton.Content = "Add";
                customerNameTextBox.IsEnabled = true;
                idTextBox.IsEnabled = true;
                cellTextBox.IsEnabled = true;
                phoneTextBox.IsEnabled = true;
                addressTextBox.IsEnabled = true;
                cityTextBox.IsEnabled = true;
                stateTextBox.IsEnabled = true;
                zipTextBox.IsEnabled = true;
                emailTextBox.IsEnabled = true;
                statusComboBox.IsEnabled = true;
                editInformationButton.IsEnabled = false;
            }
            else
            {
                newCustomerButton.Content = "New";
                customerNameTextBox.IsEnabled = false;
                idTextBox.IsEnabled = false;
                cellTextBox.IsEnabled = false;
                phoneTextBox.IsEnabled = false;
                addressTextBox.IsEnabled = false;
                cityTextBox.IsEnabled = false;
                stateTextBox.IsEnabled = false;
                zipTextBox.IsEnabled = false;
                emailTextBox.IsEnabled = false;
                statusComboBox.IsEnabled = false;
                editInformationButton.IsEnabled = true;
                // Edit the database
                addCustomer();
            }
        }
        // End of newCustomerButton_Click



        // When the Edit button is clicked
        private void editInformationButton_Click(object sender, RoutedEventArgs e)
        {
            // Edit the form
            if (editInformationButton.Content.Equals("Edit Info"))
            {
                editInformationButton.Content = "Confirm";
                customerNameTextBox.IsEnabled = true;
                idTextBox.IsEnabled = true;
                cellTextBox.IsEnabled = true;
                phoneTextBox.IsEnabled = true;
                addressTextBox.IsEnabled = true;
                cityTextBox.IsEnabled = true;
                stateTextBox.IsEnabled = true;
                zipTextBox.IsEnabled = true;
                emailTextBox.IsEnabled = true;
                statusComboBox.IsEnabled = true;
                newCustomerButton.IsEnabled = false;
            }
            else
            {
                editInformationButton.Content = "Edit Info";
                customerNameTextBox.IsEnabled = false;
                idTextBox.IsEnabled = false;
                cellTextBox.IsEnabled = false;
                phoneTextBox.IsEnabled = false;
                addressTextBox.IsEnabled = false;
                cityTextBox.IsEnabled = false;
                stateTextBox.IsEnabled = false;
                zipTextBox.IsEnabled = false;
                emailTextBox.IsEnabled = false;
                statusComboBox.IsEnabled = false;
                newCustomerButton.IsEnabled = true;
                // Connect to the database
                editCustomer();
            }
        }
        // End of editInformationButton_Click



        // When the New Order menuItem is clicked
        private void newOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Replace PSU logo with the rental form
            orderGrid.Visibility = Visibility.Visible;
            psuLogoImageBrush.Opacity = 0;
            // Reset the form
            resetFormInfo();
            idTextBox.Text = "";
            idTextBox.Focus();
        }



        private void openOrdersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //string orderNum;
            Orders orders = new Orders(this);
            orders.ShowDialog();
            if (querySuccessful)
            {
                // Replace PSU logo with the rental form
                orderGrid.Visibility = Visibility.Visible;
                psuLogoImageBrush.Opacity = 0;
                openOrder();
                querySuccessful = false;
                showItems();
            }
        }



        // When the focus leaves the ID textBox
        // Only when a valid ID is entered should an order be created.
        private void idTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (newCustomerButton.Content.Equals("New") && editInformationButton.Content.Equals("Edit Info"))
            {
                if (!idTextBox.Text.Equals(""))
                {
                    if (idTextBox.Text.Length == 9)
                    {
                        displayCustomerInfo();
                        if (orderNumberTextBox.Text == "" && querySuccessful)
                        {
                            querySuccessful = false;
                            createNewOrder();
                        }
                    }
                    else
                    {
                        Message message = new Message("Invalid ID Length");
                        message.ShowDialog();
                    }
                }
                else
                {
                    idTextBox.Text = "ID";
                }
            }
        }



        private void reservedButton_Click(object sender, RoutedEventArgs e)
        {
            if (orderStatusTextBox.Text.Equals("Picked Up") || orderStatusTextBox.Text.Equals("Returned") || orderStatusTextBox.Text.Equals("Completed"))
            {
                Message message = new Message("Order already " + orderStatusTextBox.Text + ". Create new order.");
                return;
            }
            // Only update if there is an order selected
            if (!orderNumberTextBox.Equals(""))
            {
                // Pass the order number and the action performed to the initial form
                int x = 0;
                Int32.TryParse(orderNumberTextBox.Text, out x);
                Initial initial = new Initial(x, "Reserved", this);
                // Pop-up initial form
                initial.ShowDialog();
                // Modify this form
                if (initialsEntered)
                {
                    orderStatusTextBox.Text = "Reserved";
                    editOpenOrder();
                    initialsEntered = false;
                }
            }
            else
            {
                Message message = new Message("No order selected");
                message.ShowDialog();
            }
        }



        private void pickedUpButton_Click(object sender, RoutedEventArgs e)
        {
            if(orderStatusTextBox.Text.Equals("Returned") || orderStatusTextBox.Text.Equals("Completed"))
            {
                Message message = new Message("Order already " + orderStatusTextBox.Text + ". Create new order.");
                return;
            }
            // Only update if there is an order selected
            if (!orderNumberTextBox.Equals(""))
            {
                // Pass the order number and the action performed to the initial form
                int x = 0;
                Int32.TryParse(orderNumberTextBox.Text, out x);
                Initial initial = new Initial(x, "Picked Up", this);
                // Pop-up initial form
                initial.ShowDialog();
                // Modify this form
                if (initialsEntered)
                {
                    orderStatusTextBox.Text = "Picked Up";
                    editOpenOrder();
                    initialsEntered = false;
                }
            }
            else
            {
                Message message = new Message("No order selected");
                message.ShowDialog();
            }
        }



        private void returnedButton_Click(object sender, RoutedEventArgs e)
        {
            if (orderStatusTextBox.Text.Equals("Completed"))
            {
                Message message = new Message("Order already " + orderStatusTextBox.Text + ". Create new order.");
                return;
            }
            // Only update if there is an order selected
            if (!orderNumberTextBox.Equals(""))
            {
                // Pass the order number and the action performed to the initial form
                int x = 0;
                Int32.TryParse(orderNumberTextBox.Text, out x);
                Initial initial = new Initial(x, "Returned", this);
                // Pop-up initial form
                initial.ShowDialog();
                // Modify this form
                if (initialsEntered)
                {
                    orderStatusTextBox.Text = "Returned";
                    editOpenOrder();
                    initialsEntered = false;
                    // Calculate late fee
                    getDaysLate();
                }
            }
            else
            {
                Message message = new Message("No order selected");
                message.ShowDialog();
            }
        }



        private void completedButton_Click(object sender, RoutedEventArgs e)
        {
            // Only update if there is an order selected
            if (!orderNumberTextBox.Equals(""))
            {
                // Pass the order number and the action performed to the initial form
                int x = 0;
                Int32.TryParse(orderNumberTextBox.Text, out x);
                Initial initial = new Initial(x, "Completed", this);
                // Pop-up initial form
                initial.ShowDialog();
                // Modify this form
                if (initialsEntered)
                {
                    orderStatusTextBox.Text = "Completed";
                    editOpenOrder();
                    initialsEntered = false;
                    resetFormInfo();
                    customerNameTextBox.IsEnabled = false;
                    psuLogoImageBrush.Opacity = 100;
                    orderGrid.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                Message message = new Message("No order selected");
                message.ShowDialog();
            }
        }



        // TODO later: choose either no initial for this action, or figure out how to rollback changes if no initials are given
        private void startDatePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            // Only update if there is an order selected
            if (!orderNumberTextBox.Text.Equals(""))
            {
                //    // Pass the order number and the action performed to the initial form
                //    int x = 0;
                //    Int32.TryParse(orderNumberTextBox.Text, out x);
                //    Initial initial = new Initial(x, "Start date changed");
                //    // Pop-up initial form
                //    initial.ShowDialog();
                //    // Modify this form
                //    if (initial.initialsEntered)
                //    {
                //        orderStatusTextBox.Text = "Completed";
                editOpenOrder();
                //    }
            }
            else
            {
                Message message = new Message("No order selected");
                message.ShowDialog();
            }
        }



        // This will be the same as whatever the methods above turns out looking like
        private void endDatePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!orderNumberTextBox.Text.Equals(""))
            {
                editOpenOrder();
            }
            else
            {
                Message message = new Message("No order selected");
                message.ShowDialog();
            }
        }



        private void orderNotesTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!orderNumberTextBox.Text.Equals(""))
            {
                editOpenOrder();
            }
            else
            {
                Message message = new Message("No order selected");
                message.ShowDialog();
            }
        }



        private void customerNotesTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!idTextBox.Text.Equals("ID"))
            {
                editCustomer();
            }
            else
            {
                Message message = new Message("No customer selected");
                message.ShowDialog();
            }
        }



        private void addItemButton_Click(object sender, RoutedEventArgs e)
        {
            Items addItems = new Items(this);
            addItems.ShowDialog();
            showItems();
        }



        private void addPaymentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            amountPaidTextBox.IsEnabled = true;
            amountPaidTextBox.Focus();
        }



        private void adjustRentalFeeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            rentalFeeTextBox.IsEnabled = true;
            rentalFeeTextBox.Focus();
        }



        private void adjustLateFeeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            lateFeeTextBox.IsEnabled = true;
            lateFeeTextBox.Focus();
        }



        private void rentalFeeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            totalDueTextBox.Text = (Convert.ToDouble(lateFeeTextBox.Text) + Convert.ToDouble(rentalFeeTextBox.Text)).ToString();
            amountDueTextBox.Text = (Convert.ToDouble(totalDueTextBox.Text) - Convert.ToDouble(amountPaidTextBox.Text)).ToString();
            rentalFeeTextBox.IsEnabled = false;
            editOpenOrder();

        }



        private void lateFeeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            totalDueTextBox.Text = (Convert.ToDouble(lateFeeTextBox.Text) + Convert.ToDouble(rentalFeeTextBox.Text)).ToString();
            amountDueTextBox.Text = (Convert.ToDouble(totalDueTextBox.Text) - Convert.ToDouble(amountPaidTextBox.Text)).ToString();
            lateFeeTextBox.IsEnabled = false;
            editOpenOrder();
        }



        private void amountPaidTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            totalDueTextBox.Text = (Convert.ToDouble(lateFeeTextBox.Text) + Convert.ToDouble(rentalFeeTextBox.Text)).ToString();
            amountDueTextBox.Text = (Convert.ToDouble(totalDueTextBox.Text) - Convert.ToDouble(amountPaidTextBox.Text)).ToString();
            lateFeeTextBox.IsEnabled = false;
            editOpenOrder();
        }



        private void quickStatsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            QuickStats stats = new QuickStats();
            stats.ShowDialog();
        }



        private void orderHistoryMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }


        /*   Database connection methods   */


        private void createNewOrder()
        {
            // Connect to database
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                // Make sure the date is compatable with MySQL server date type
                startDatePicker.SelectedDate = DateTime.Today;
                endDatePicker.SelectedDate = DateTime.Today;
                DateTime startDate = (DateTime)startDatePicker.SelectedDate;
                startDate.ToString("yyyy-MM-dd");
                DateTime endDate = (DateTime)endDatePicker.SelectedDate;
                endDate.ToString("yyyy-MM-dd");
                // We shouldn't need this, as the box can't be checked when creating a new order
                /*
                float soar;
                if ((bool)soarCheckBox.IsChecked)
                {
                    soar = 1;
                }
                else
                {
                    soar = 0;
                }
                */
                string newOrder = "INSERT INTO Orders (customer_id, name, start_date, end_date, order_status, notes) VALUES (@customer_id, @name, @start_date, @end_date, @order_status, @notes)";
                using (MySqlCommand insertNewOrder = new MySqlCommand(newOrder))
                {
                    try
                    {
                        insertNewOrder.Connection = openCon;
                        insertNewOrder.Parameters.Add("@customer_id", MySqlDbType.VarChar, 9).Value = idTextBox.Text;
                        insertNewOrder.Parameters.Add("@name", MySqlDbType.VarChar, 30).Value = customerNameTextBox.Text;
                        insertNewOrder.Parameters.Add("@start_date", MySqlDbType.Date).Value = startDate;
                        insertNewOrder.Parameters.Add("@end_date", MySqlDbType.Date).Value = endDate;
                        insertNewOrder.Parameters.Add("@order_status", MySqlDbType.VarChar, 10).Value = orderStatusTextBox.Text;
                        insertNewOrder.Parameters.Add("@notes", MySqlDbType.Text).Value = orderNotesTextBox.Text;
                        openCon.Open();
                        insertNewOrder.ExecuteNonQuery();
                        int insertedId = (int)insertNewOrder.LastInsertedId;
                        openCon.Close();
                        // Only modify the form once we know the query was successful
                        customerNameTextBox.IsEnabled = false;
                        idTextBox.IsEnabled = false;
                        orderNotesTextBox.IsEnabled = true;
                        startDatePicker.IsEnabled = true;
                        endDatePicker.IsEnabled = true;
                        amountDueTextBox.Text = "0";
                        amountPaidTextBox.Text = "0";
                        totalDueTextBox.Text = "0";
                        rentalFeeTextBox.Text = "0";
                        lateFeeTextBox.Text = "0";
                        orderNumberTextBox.Text = insertedId.ToString();
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                        customerNameTextBox.IsEnabled = true;
                        idTextBox.IsEnabled = true;
                    }
                }
            }
        }



        private void openOrder()
        {
            // Connect to database
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                string orderQuery = "SELECT * FROM Orders WHERE order_num = @order_num";
                using (MySqlCommand orderCmd = openCon.CreateCommand())
                {
                    orderCmd.Parameters.Add("@order_num", MySqlDbType.Int16, 9).Value = orderNumberTextBox.Text;
                    try
                    {
                        openCon.Open();
                        orderCmd.CommandText = orderQuery;
                        using (MySqlDataReader reader = orderCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                idTextBox.Text = reader.GetString(1);
                                startDatePicker.SelectedDate = reader.GetDateTime(3);
                                endDatePicker.SelectedDate = reader.GetDateTime(4);
                                orderStatusTextBox.Text = reader.GetString(5);
                                rentalFeeTextBox.Text = reader.GetString(6);
                                lateFeeTextBox.Text = reader.GetString(7);
                                totalDueTextBox.Text = reader.GetString(8);
                                amountPaidTextBox.Text = reader.GetString(9);
                                amountDueTextBox.Text = reader.GetString(10);
                                if (Int32.Parse(reader.GetString(11)) == 1)
                                {
                                    soarCheckBox.IsChecked = true;
                                }
                                else
                                {
                                    soarCheckBox.IsChecked = false;
                                }
                                orderNotesTextBox.Text = reader.GetString(12);
                            }
                            openCon.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                    }
                }
                displayCustomerInfo();
            }
        }



        private void editOpenOrder()
        {
            // Make sure the date is compatable with MySQL server date type
            DateTime startDate = (DateTime)startDatePicker.SelectedDate;
            startDate.ToString("yyyy-MM-dd");
            DateTime endDate = (DateTime)endDatePicker.SelectedDate;
            endDate.ToString("yyyy-MM-dd");
            int soar;
            if ((bool)soarCheckBox.IsChecked)
            {
                soar = 1;
            }
            else
            {
                soar = 0;
            }
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                string updateOrder = "UPDATE Orders SET order_num = @order_num, customer_id = @customer_id, name = @name, start_date = @start_date, end_date = @end_date, order_status = @order_status, rental_fee = @rental_fee, late_fee = @late_fee, total = @total, balance_paid = @balance_paid, balance_due = @balance_due, soar_trip = @soar_trip, notes = @notes WHERE order_num = @order_num";
                using (MySqlCommand updateOpenOrder = new MySqlCommand(updateOrder))
                {
                    try
                    {
                        updateOpenOrder.Connection = openCon;
                        updateOpenOrder.Parameters.Add("@order_num", MySqlDbType.Int16, 9).Value = orderNumberTextBox.Text;
                        updateOpenOrder.Parameters.Add("@customer_id", MySqlDbType.VarChar, 9).Value = idTextBox.Text;
                        updateOpenOrder.Parameters.Add("@name", MySqlDbType.VarChar, 30).Value = customerNameTextBox.Text;
                        updateOpenOrder.Parameters.Add("@start_date", MySqlDbType.Date).Value = startDate;
                        updateOpenOrder.Parameters.Add("@end_date", MySqlDbType.Date).Value = endDate;
                        updateOpenOrder.Parameters.Add("@order_status", MySqlDbType.VarChar, 10).Value = orderStatusTextBox.Text;
                        updateOpenOrder.Parameters.Add("@rental_fee", MySqlDbType.Float).Value = rentalFeeTextBox.Text;
                        updateOpenOrder.Parameters.Add("@late_fee", MySqlDbType.Float).Value = lateFeeTextBox.Text;
                        updateOpenOrder.Parameters.Add("@total", MySqlDbType.Float).Value = totalDueTextBox.Text;
                        updateOpenOrder.Parameters.Add("@balance_paid", MySqlDbType.Float).Value = amountPaidTextBox.Text;
                        updateOpenOrder.Parameters.Add("@balance_due", MySqlDbType.Float).Value = amountDueTextBox.Text;
                        updateOpenOrder.Parameters.Add("@soar_trip", MySqlDbType.Bit).Value = soar;
                        updateOpenOrder.Parameters.Add("@notes", MySqlDbType.Text).Value = orderNotesTextBox.Text;
                        openCon.Open();
                        updateOpenOrder.ExecuteNonQuery();
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



        private void displayCustomerInfo()
        {
            // Connect to database
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                string selectCust = "SELECT * FROM Customers WHERE psu_id = @psu_id";
                using (MySqlCommand read = openCon.CreateCommand())
                {
                    read.Parameters.Add("@psu_id", MySqlDbType.VarChar, 9).Value = idTextBox.Text;
                    try
                    {
                        openCon.Open();
                        read.CommandText = selectCust;
                        Boolean existingCustomer = true;
                        using (MySqlDataReader reader = read.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.GetString(2).Equals(null))
                                {
                                    existingCustomer = false;
                                    break;
                                }
                                else
                                {
                                    idTextBox.Text = reader.GetString(1).Trim('"');
                                    customerNameTextBox.Text = reader.GetString(2).Trim('"');
                                    string status = reader.GetString(4).Trim('"');
                                    if (status.EndsWith("/STAFF"))
                                    {
                                        status = status.Substring(0, status.LastIndexOf("/STAFF"));
                                    }
                                    foreach (ComboBoxItem item in statusComboBox.Items)
                                    {
                                        if (item.Content.ToString().Equals(status))
                                        {
                                            statusComboBox.SelectedValue = item;
                                        }
                                    }
                                    phoneTextBox.Text = reader.GetString(5).Trim('"');
                                    cellTextBox.Text = reader.GetString(6).Trim('"');
                                    addressTextBox.Text = reader.GetString(7).Trim('"');
                                    cityTextBox.Text = reader.GetString(8).Trim('"');
                                    stateTextBox.Text = reader.GetString(9).Trim('"');
                                    zipTextBox.Text = reader.GetString(10).Trim('"');
                                    emailTextBox.Text = reader.GetString(12).Trim('"');
                                    customerNotesTextBox.Text = reader.GetString(14).Trim('"');
                                }
                            }
                        }
                        openCon.Close();
                        // Only modify the form if the given customer is in the system
                        if (existingCustomer)
                        {
                            querySuccessful = true;
                            customerNameTextBox.IsEnabled = false;
                            idTextBox.IsEnabled = false;
                            customerNotesTextBox.IsEnabled = true;
                            newCustomerButton.IsEnabled = false;
                            editInformationButton.IsEnabled = true;
                            startDatePicker.IsEnabled = true;
                            endDatePicker.IsEnabled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                    }

                }
            }
        }



        private void showItems()
        {
            // Clear the dataGrid
            itemsOrdered.Clear();
            itemsDataGrid.Items.Refresh();
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                string getItems = "SELECT o.item_name, o.quantity, i.replacement_cost, i.faculty_price, i.general_public_price FROM Ordered_Items o, Inventory i WHERE o.order_num = @order_num AND o.item_id = i.item_id ORDER BY i.item_id";
                using(MySqlCommand queryGetItems = new MySqlCommand(getItems))
                {
                    try
                    {
                        queryGetItems.Connection = openCon;
                        queryGetItems.Parameters.Add("@order_num", MySqlDbType.Int16, 9).Value = orderNumberTextBox.Text;
                        openCon.Open();
                        MySqlDataAdapter dataAdaptor = new MySqlDataAdapter(queryGetItems);
                        dataAdaptor.Fill(itemsOrdered);
                        itemsDataGrid.ItemsSource = itemsOrdered.DefaultView;
                        openCon.Close();
                    }
                    catch(Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                    }
                }
            }
            // Calculate the rental fee
            int priceLevel = 0;
            double rentalFee = 0;
            if (statusComboBox.Text.Equals("FACULTY") || statusComboBox.Text.Equals("ALUMNI"))
            {
                priceLevel = 3;
            }
            else if (statusComboBox.Text.Equals("PUBLIC"))
            {
                priceLevel = 4;
            }
            // show prices as long as price is not 0
            if (priceLevel != 0)
            {
                foreach (DataRow row in itemsOrdered.Rows)
                {
                    rentalFee += (Convert.ToDouble(row[priceLevel]) * Convert.ToInt16(row["quantity"]));
                }
                rentalFeeTextBox.Text = Convert.ToString(rentalFee);
                double amountDue = rentalFee - Convert.ToDouble(amountPaidTextBox.Text);
                totalDueTextBox.Text = Convert.ToString(amountDue);
            }
            editOpenOrder();
        }



        public void getDaysLate()
        {
            int daysLate = 0;
            int daysLateExcludingWeekdays = 0;
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                string lateFee = "SELECT DATEDIFF(CURDATE(), end_date) FROM Orders WHERE order_num = @order_num";
                using(MySqlCommand queryLateFee = new MySqlCommand(lateFee))
                {
                    try
                    {
                        queryLateFee.Connection = openCon;
                        queryLateFee.Parameters.Add("@order_num", MySqlDbType.Int16, 9).Value = orderNumberTextBox.Text;
                        openCon.Open();
                        using (MySqlDataReader reader = queryLateFee.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                daysLate = Convert.ToInt16(reader.GetString(0));
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
            Console.WriteLine("Days late: " + daysLate);
            if (daysLate > 0)
            {
                daysLateExcludingWeekdays = calculateDaysLate(daysLate);
                Console.WriteLine("Days late excluding weekdays: " + daysLateExcludingWeekdays);
                calculateLateFee(daysLateExcludingWeekdays);
            }
            else
            {
                return;
            }
        }



        private void editCustomer()
        {
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                string editCust = "UPDATE Customers SET psu_id = @psu_id, name = @name, status = @status, phone = @phone, cell_phone = @cell_phone, address = @address, city = @city, state = @state, zip = @zip, email_address = @email_address WHERE psu_id = @psu_id";
                using (MySqlCommand queryEditCust = new MySqlCommand(editCust))
                {
                    try
                    {
                        queryEditCust.Connection = openCon;
                        queryEditCust.Parameters.Add("@psu_id", MySqlDbType.VarChar, 9).Value = idTextBox.Text;
                        queryEditCust.Parameters.Add("@name", MySqlDbType.VarChar, 30).Value = customerNameTextBox.Text;
                        queryEditCust.Parameters.Add("@status", MySqlDbType.VarChar, 15).Value = statusComboBox.Text;
                        queryEditCust.Parameters.Add("@phone", MySqlDbType.VarChar, 12).Value = phoneTextBox.Text;
                        queryEditCust.Parameters.Add("@cell_phone", MySqlDbType.VarChar, 12).Value = cellTextBox.Text;
                        queryEditCust.Parameters.Add("@address", MySqlDbType.VarChar, 50).Value = addressTextBox.Text;
                        queryEditCust.Parameters.Add("@city", MySqlDbType.VarChar, 25).Value = cityTextBox.Text;
                        queryEditCust.Parameters.Add("@state", MySqlDbType.VarChar, 2).Value = stateTextBox.Text;
                        queryEditCust.Parameters.Add("@zip", MySqlDbType.VarChar, 9).Value = zipTextBox.Text;
                        queryEditCust.Parameters.Add("@email_address", MySqlDbType.VarChar, 9).Value = emailTextBox.Text;
                        openCon.Open();
                        queryEditCust.ExecuteNonQuery();
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



        private void addCustomer()
        {
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                string newCust = "INSERT INTO Customers (psu_id, name, status, phone, cell_phone, address, city, state, zip, email_address, notes) VALUES (@psu_id, @name, @status, @phone, @cell_phone, @address, @city, @state, @zip, @email_address, @notes)";
                using (MySqlCommand queryNewCust = new MySqlCommand(newCust))
                {
                    try
                    {
                        queryNewCust.Connection = openCon;
                        queryNewCust.Parameters.Add("@psu_id", MySqlDbType.VarChar, 9).Value = idTextBox.Text;
                        queryNewCust.Parameters.Add("@name", MySqlDbType.VarChar, 30).Value = customerNameTextBox.Text;
                        queryNewCust.Parameters.Add("@status", MySqlDbType.VarChar, 15).Value = statusComboBox.Text;
                        queryNewCust.Parameters.Add("@phone", MySqlDbType.VarChar, 12).Value = phoneTextBox.Text;
                        queryNewCust.Parameters.Add("@cell_phone", MySqlDbType.VarChar, 12).Value = cellTextBox.Text;
                        queryNewCust.Parameters.Add("@address", MySqlDbType.VarChar, 50).Value = addressTextBox.Text;
                        queryNewCust.Parameters.Add("@city", MySqlDbType.VarChar, 25).Value = cityTextBox.Text;
                        queryNewCust.Parameters.Add("@state", MySqlDbType.VarChar, 2).Value = stateTextBox.Text;
                        queryNewCust.Parameters.Add("@zip", MySqlDbType.VarChar, 9).Value = zipTextBox.Text;
                        queryNewCust.Parameters.Add("@email_address", MySqlDbType.VarChar, 9).Value = emailTextBox.Text;
                        queryNewCust.Parameters.Add("@notes", MySqlDbType.Text).Value = customerNotesTextBox.Text;
                        openCon.Open();
                        queryNewCust.ExecuteNonQuery();
                        openCon.Close();
                    }
                    catch (Exception ex)
                    {
                        openCon.Close();
                        Console.WriteLine("An error occurred: '{0}'", ex);
                        customerNameTextBox.IsEnabled = true;
                        idTextBox.IsEnabled = true;
                        System.Windows.Forms.MessageBox.Show("You cannot have duplicate IDs");
                    }
                }
            }
        }



        private void importInventoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                MySqlBulkLoader loader = new MySqlBulkLoader(openCon);
                loader.TableName = "Inventory";
                loader.FieldTerminator = "\t";
                loader.LineTerminator = "\n";
                loader.FileName = inventoryLocation;
                loader.NumberOfLinesToSkip = 1;
                try
                {
                    MySqlCommand truncate = new MySqlCommand("TRUNCATE TABLE Inventory");
                    truncate.Connection = openCon;
                    openCon.Open();
                    truncate.ExecuteNonQuery();
                    int count = loader.Load();
                    openCon.Close();
                    Message loaded = new Message(count + " items imported");
                    loaded.ShowDialog();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: '{0}'", ex);
                    openCon.Close();
                }
            }
        }



        private void importCustomersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            worker.RunWorkerAsync();

            // The stuff beneath is old and didn't quite work, but leave it here just in case.

            //using (MySqlConnection openCon = new MySqlConnection(connString))
            //{
            //    MySqlBulkLoader loader = new MySqlBulkLoader(openCon);
            //    loader.TableName = "Customers";
            //    loader.FieldTerminator = ",";
            //    loader.LineTerminator = "\n";
            //    loader.FileName = customerLocation;
            //    loader.NumberOfLinesToSkip = 1;
            //    try
            //    {
            //        MySqlCommand truncate = new MySqlCommand("TRUNCATE TABLE Customers");
            //        truncate.Connection = openCon;
            //        openCon.Open();
            //        truncate.ExecuteNonQuery();
            //        int count = loader.Load();
            //        openCon.Close();
            //        Message loaded = new Message(count + " items imported");
            //        loaded.ShowDialog();
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("An error occurred: '{0}'", ex);
            //        openCon.Close();
            //    }
            //}
        }


        /*   Form modification methods   */


        private void resetFormInfo()
        {
            customerNameTextBox.Text = "Name";
            customerNameTextBox.IsEnabled = true;
            idTextBox.Text = "ID";
            idTextBox.IsEnabled = true;
            cellTextBox.Text = "Cell Phone";
            phoneTextBox.Text = "Phone Number";
            addressTextBox.Text = "Address";
            cityTextBox.Text = "City";
            stateTextBox.Text = "State";
            zipTextBox.Text = "Zip";
            emailTextBox.Text = "Email";
            statusComboBox.Text = "";
            customerNotesTextBox.Text = "";
            orderNotesTextBox.Text = "";
            rentalFeeTextBox.Text = "";
            lateFeeTextBox.Text = "";
            totalDueTextBox.Text = "";
            amountPaidTextBox.Text = "";
            amountDueTextBox.Text = "";
            editInformationButton.Content = "Edit Info";
            editInformationButton.IsEnabled = false;
            newCustomerButton.Content = "New";
            newCustomerButton.IsEnabled = true;
            orderStatusTextBox.Text = "Reserved";
            soarCheckBox.IsChecked = false;
            statusComboBox.SelectedIndex = -1;
            startDatePicker.SelectedDate = null;
            startDatePicker.DisplayDate = DateTime.Today;
            startDatePicker.IsEnabled = false;
            endDatePicker.SelectedDate = null;
            endDatePicker.DisplayDate = DateTime.Today;
            endDatePicker.IsEnabled = false;
            customerNotesTextBox.IsEnabled = false;
            orderNotesTextBox.IsEnabled = false;
            orderNumberTextBox.Text = "";
            // Reset the items dataGrid
            itemsOrdered.Clear();
            itemsDataGrid.Items.Refresh();
        }


        /*   Calculation methods   */


        private int calculateDaysLate(int daysLate)
        {
            int weekdaysExcluded = 0;
            string dayOfWeek = DateTime.Now.DayOfWeek.ToString();
            switch (dayOfWeek)
            {
                case "Monday":
                    while (daysLate > 0)
                    {
                        daysLate -= 2;
                        if (daysLate > 5)
                        {
                            weekdaysExcluded += 5;
                            if(daysLate < 2)
                            {
                                weekdaysExcluded += daysLate;
                                break;
                            }
                        }
                        else
                        {
                            weekdaysExcluded += daysLate;
                            break;
                        }
                    }
                    break;
                case "Tuesday":
                    daysLate -= 1;
                    weekdaysExcluded += 1;
                    while (daysLate > 0)
                    {
                        daysLate -= 2;
                        if (daysLate > 5)
                        {
                            weekdaysExcluded += 5;
                            if (daysLate < 2)
                            {
                                weekdaysExcluded += daysLate;
                                break;
                            }
                        }
                        else
                        {
                            weekdaysExcluded += daysLate;
                            break;
                        }
                    }
                    break;
                case "Wednesday":
                    if(daysLate < 2)
                    {
                        return daysLate;
                    }
                    daysLate -= 2;
                    weekdaysExcluded += 2;
                    while (daysLate > 0)
                    {
                        daysLate -= 2;
                        if (daysLate > 5)
                        {
                            weekdaysExcluded += 5;
                            if (daysLate < 2)
                            {
                                weekdaysExcluded += daysLate;
                                break;
                            }
                        }
                        else
                        {
                            weekdaysExcluded += daysLate;
                            break;
                        }
                    }
                    break;
                case "Thursday":
                    if (daysLate < 3)
                    {
                        return daysLate;
                    }
                    daysLate -= 3;
                    weekdaysExcluded += 3;
                    while (daysLate > 0)
                    {
                        daysLate -= 2;
                        if (daysLate > 5)
                        {
                            weekdaysExcluded += 5;
                            if (daysLate < 2)
                            {
                                weekdaysExcluded += daysLate;
                                break;
                            }
                        }
                        else
                        {
                            weekdaysExcluded += daysLate;
                            break;
                        }
                    }
                    break;
                case "Friday":
                    if (daysLate < 4)
                    {
                        return daysLate;
                    }
                    daysLate -= 4;
                    weekdaysExcluded += 4;
                    while (daysLate > 0)
                    {
                        daysLate -= 2;
                        if (daysLate > 5)
                        {
                            weekdaysExcluded += 5;
                            if (daysLate < 2)
                            {
                                weekdaysExcluded += daysLate;
                                break;
                            }
                        }
                        else
                        {
                            weekdaysExcluded += daysLate;
                            break;
                        }
                    }
                    break;
                case "Saturday":
                    daysLate -= 1;
                    if (daysLate < 4)
                    {
                        return daysLate;
                    }
                    daysLate -= 4;
                    weekdaysExcluded += 4;
                    while (daysLate > 0)
                    {
                        daysLate -= 2;
                        if (daysLate > 5)
                        {
                            weekdaysExcluded += 5;
                            if (daysLate < 2)
                            {
                                weekdaysExcluded += daysLate;
                                break;
                            }
                        }
                        else
                        {
                            weekdaysExcluded += daysLate;
                            break;
                        }
                    }
                    break;
                case "Sunday":
                    daysLate -= 2;
                    if(daysLate < 0)
                    {
                        return 0;
                    }
                    if (daysLate < 4)
                    {
                        return daysLate;
                    }
                    daysLate -= 4;
                    weekdaysExcluded += 4;
                    while (daysLate > 0)
                    {
                        daysLate -= 2;
                        if (daysLate > 5)
                        {
                            weekdaysExcluded += 5;
                            if (daysLate < 2)
                            {
                                weekdaysExcluded += daysLate;
                                break;
                            }
                        }
                        else
                        {
                            weekdaysExcluded += daysLate;
                            break;
                        }
                    }
                    break;
                default:
                    return daysLate;
            }
            return weekdaysExcluded;
        }



        public void calculateLateFee(int daysLate)
        {
            /* row[1] is the quantity */
            float lateFee = 0;
            // Late fee cannot accumulate more than $10 per day
            int itemCounter = 0;
            // Late fee cannot exceed total replacement cost
            float maxLateFee = 0;

            foreach (DataRow row in itemsOrdered.Rows)
            {
                // Late fee cannot exceed replacement cost
                if(daysLate >= Convert.ToInt16(row[2]))
                {
                    lateFee += ((float) Convert.ToDouble(row[2]) * Convert.ToInt16(row[1]));
                }
                else
                {
                    lateFee += ((float)Convert.ToInt16(row[1]) * daysLate);
                    maxLateFee += ((float)Convert.ToDouble(row[2]) * Convert.ToInt16(row[1]));
                }
                itemCounter += Convert.ToInt16(row[1]);
            }
            Console.WriteLine("Item counter: " + itemCounter);
            if(itemCounter >= 10)
            {
                lateFee = daysLate * 10;
                if(lateFee > maxLateFee)
                {
                    lateFee = maxLateFee;
                }
            }

            lateFeeTextBox.Text = lateFee.ToString();
            totalDueTextBox.Text = (lateFee + Convert.ToDouble(rentalFeeTextBox.Text)).ToString();
            amountDueTextBox.Text = (lateFee - Convert.ToDouble(totalDueTextBox.Text)).ToString();
            editOpenOrder();
        }


        /*   Multithreading Methods   */


        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here
            using (MySqlConnection openCon = new MySqlConnection(connString))
            {
                String importCustomers = "TRUNCATE TABLE Customers; LOAD DATA LOCAL INFILE 'Customers.csv' INTO TABLE Customers FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '\"' LINES TERMINATED BY '\n' IGNORE 1 LINES;";
                using (MySqlCommand importCustomersQuery = new MySqlCommand(importCustomers))
                {
                    try
                    {
                        importCustomersQuery.Connection = openCon;
                        openCon.Open();
                        importCustomersQuery.ExecuteNonQuery();
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



        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work
            Message loaded = new Message("Customers imported");
            loaded.ShowDialog();
        }
    }
}
