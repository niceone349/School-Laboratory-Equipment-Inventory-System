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
using Microsoft.Data.SqlClient;

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for StockNotification.xaml
    /// </summary>
    public partial class StockNotification : Page
    {
        private readonly string connectionString = Server.ConnString;
        private readonly List<StockItem> stockList = new List<StockItem>(); 

        public StockNotification()
        {
            InitializeComponent();
            LoadStockData();
        }

        private void LoadStockData()
        {
            stockList.Clear();

            using SqlConnection conn = new(connectionString);
            try
            {
                conn.Open();
                string query = @"
                SELECT a.Item_Name, a.Item_Quantity, a.Item_ID, a.Item_Description,
                    CASE 
                        WHEN d.Item_ID IS NOT NULL THEN 'Damaged'
                        WHEN b.Item_ID IS NOT NULL THEN 'Borrowed'
                        WHEN a.Item_Quantity = 0 THEN 'No Stock'
                        WHEN a.Item_Quantity <= a.Item_Low_Indicator THEN 'Low Stock'
                        ELSE 'Available'
                    END AS Status
                FROM AvailableItems a
                LEFT JOIN BorrowedItems b ON a.Item_ID = b.Item_ID
                LEFT JOIN ReportedItems d ON a.Item_ID = d.Item_ID";

                SqlCommand cmd = new(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    stockList.Add(new StockItem
                    {
                        EquipmentID = reader["Item_ID"]?.ToString() ?? string.Empty,
                        Name = reader["Item_Name"]?.ToString() ?? string.Empty,
                        Stock = reader["Item_Quantity"] != DBNull.Value ? Convert.ToInt32(reader["Item_Quantity"]) : 0, 
                        Description = reader["Item_Description"]?.ToString() ?? string.Empty,
                        Status = reader["Status"]?.ToString() ?? string.Empty
                    });
                }

                reader.Close();
                StockNotificationDataGrid.ItemsSource = stockList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading stock data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        private void Button_Damaged_Click(object sender, RoutedEventArgs e)
        {
            FilterData("Reported");
        }

        private void Button_Borrowed_Click(object sender, RoutedEventArgs e)
        {
            FilterData("Borrowed");
        }

        private void Button_LowStock_Click(object sender, RoutedEventArgs e)
        {
            FilterData("Low Stock");
        }

        private void Button_NoStock_Click(object sender, RoutedEventArgs e)
        {
            FilterData("No Stock");
        }

        private void Button_Reset_Click(object sender, RoutedEventArgs e)
        {
            LoadStockData();
            StockNotificationDataGrid.Items.Refresh();
        }


        private void FilterData(string status)
        {
            stockList.Clear();
            using SqlConnection conn = new(connectionString);
            try
            {
                conn.Open();
                string query = status switch
                {
                    "Borrowed" => @"
                SELECT b.Item_ID, a.Item_Name, b.Borrowed_Quantity AS Item_Quantity, a.Item_Description
                FROM BorrowedItems b
                JOIN AvailableItems a ON b.Item_ID = a.Item_ID",

                    "Low Stock" => @"
                SELECT Item_ID, Item_Name, Item_Quantity, Item_Description
                FROM AvailableItems
                WHERE Item_Quantity > 0 AND Item_Quantity <= Item_Low_Indicator",

                    "No Stock" => @"
                SELECT Item_ID, Item_Name, Item_Quantity, Item_Description
                FROM AvailableItems
                WHERE Item_Quantity = 0",

                    "Reported" => @"
                SELECT r.Item_ID, a.Item_Name, r.Item_Quantity, a.Item_Description
                FROM ReportedItems r
                JOIN AvailableItems a ON r.Item_ID = a.Item_ID",

                    _ => null
                };

                if (query == null) return;

                using SqlCommand cmd = new(query, conn);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    stockList.Add(new StockItem
                    {
                        EquipmentID = reader["Item_ID"]?.ToString() ?? string.Empty,
                        Name = reader["Item_Name"]?.ToString() ?? string.Empty,
                        Stock = reader["Item_Quantity"] != DBNull.Value ? Convert.ToInt32(reader["Item_Quantity"]) : 0,
                        Description = reader["Item_Description"]?.ToString() ?? string.Empty,
                        Status = status
                    });
                }

                StockNotificationDataGrid.ItemsSource = stockList;
                StockNotificationDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading {status} stock data: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void StockNotificationDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is StockItem stockItem)
            {
                e.Row.Background = stockItem.Status switch
                {
                    "Borrowed" => new SolidColorBrush(Colors.LightBlue),
                    "Low Stock" => new SolidColorBrush(Colors.Yellow),
                    "No Stock" => new SolidColorBrush(Colors.Red),
                    "Damaged" => new SolidColorBrush(Colors.Gray),
                    _ => e.Row.Background
                };
            }
        }

        private void StockNotificationDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StockNotificationDataGrid.SelectedItem is StockItem selectedItem)
            {
                MessageBox.Show($"Selected Item: {selectedItem.Name}", "Selection Changed");
            }
        }
    }

    public class StockItem
    {
        public string EquipmentID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
