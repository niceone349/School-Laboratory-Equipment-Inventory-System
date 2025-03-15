using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for Return.xaml
    /// </summary>
    public partial class Return : Page
    {
        private string connectionString = Server.ConnString;

        public Return()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                DataTable dt = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM BorrowedItems", conn);
                adapter.Fill(dt);
                myDataGrid.ItemsSource = dt.DefaultView; 
            }
        }
        private int GenerateActivityID()
        {
            DateTime baseDate = new DateTime(2020, 1, 1);
       
            int activityID = (int)(DateTime.Now - baseDate).TotalSeconds;
            return activityID;
        }
        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView row)
            {
                string itemName = row["Item_Name"].ToString();
                int borrowedQuantity = Convert.ToInt32(row["Borrowed_Quantity"]);
                int borrowedID = Convert.ToInt32(row["Borrowed_ID"]);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                    
                        string updateAvailableItems = @"
                            UPDATE AvailableItems
                            SET Item_Quantity = Item_Quantity + @BorrowedQuantity
                            WHERE Item_Name = @ItemName";

                        using (SqlCommand cmd = new SqlCommand(updateAvailableItems, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BorrowedQuantity", borrowedQuantity);
                            cmd.Parameters.AddWithValue("@ItemName", itemName);
                            cmd.ExecuteNonQuery();
                        }

                      
                        string deleteBorrowedItem = "DELETE FROM BorrowedItems WHERE Borrowed_ID = @BorrowedID";

                        using (SqlCommand cmd = new SqlCommand(deleteBorrowedItem, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@BorrowedID", borrowedID);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();

                        string insertActivityQuery = @"
                                INSERT INTO ActivityLog (Activity_ID, Action)
                                VALUES (@activityID, @action)";
                        using (SqlCommand activityCmd = new SqlCommand(insertActivityQuery, conn))
                        {
                            activityCmd.Parameters.AddWithValue("@activityID", GenerateActivityID());
                            activityCmd.Parameters.AddWithValue("@action", "Return Equipment");
                            activityCmd.ExecuteNonQuery();
                        }
                        
                        MessageBox.Show("Item returned successfully!");

                       
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }
        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is DataRowView row)
            {
                int itemID = Convert.ToInt32(row["Item_ID"]);  
                int borrowedQuantity = Convert.ToInt32(row["Borrowed_Quantity"]); 

             
                ReportWindow reportWindow = new ReportWindow(itemID, borrowedQuantity);
              
                reportWindow.ReportSubmitted += RefreshDataGrid;

                reportWindow.ShowDialog();
            }
        }
        private void RefreshDataGrid()
        {

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM BorrowedItems"; 

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    myDataGrid.ItemsSource = dt.DefaultView; 
                }
            }
        }
    }
}
