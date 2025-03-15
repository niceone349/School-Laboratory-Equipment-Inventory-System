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
using Microsoft.Data.SqlClient;
namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {

        private int itemID;
        private int borrowedQuantity;
        private string connectionString = Server.ConnString;
        public event Action ReportSubmitted;
        public ReportWindow(int itemID, int borrowedQuantity)
        {
            InitializeComponent();
            this.itemID = itemID;
            this.borrowedQuantity = borrowedQuantity;
        }

        private int GenerateActivityID()
        {
            DateTime baseDate = new DateTime(2020, 1, 1);
            return (int)(DateTime.Now - baseDate).TotalSeconds;
        }

        private int GenerateReportID()
        {
            DateTime baseDate = new DateTime(2020, 1, 1);
            return (int)(DateTime.Now - baseDate).TotalSeconds;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string ReportStatus = ReportStatusTextBox.Text;
            int quantityToReport;

            // Validate input
            if (string.IsNullOrWhiteSpace(ReportStatus))
            {
                MessageBox.Show("Please fill in all fields before submitting.");
                return;
            }

            if (!int.TryParse(QuantityTextbox.Text, out quantityToReport) || quantityToReport <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.");
                return;
            }

            if (quantityToReport > borrowedQuantity)
            {
                MessageBox.Show("Reported quantity cannot be greater than borrowed quantity.");
                return;
            }

            int activityID = GenerateActivityID();
            int reportedID = GenerateReportID();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 🔹 1️⃣ Insert into ActivityLog
                    string activityQuery = @"
                INSERT INTO ActivityLog (Activity_ID, Action) 
                VALUES (@ActivityID, @Action)";

                    using (SqlCommand cmd = new SqlCommand(activityQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ActivityID", activityID);
                        cmd.Parameters.AddWithValue("@Action", $"Reported {quantityToReport} item(s) for Item ID {itemID}");
                        cmd.ExecuteNonQuery();
                    }

                    // 🔹 2️⃣ Insert into ReportedItems
                    string insertQuery = @"
                INSERT INTO ReportedItems (Reported_ID, Item_ID, Report_Status, Activity_ID, Item_Quantity)
                VALUES (@ReportedID, @ItemID, @ReportStatus, @ActivityID, @ReportedQuantity)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ReportedID", reportedID);
                        cmd.Parameters.AddWithValue("@ItemID", itemID);
                        cmd.Parameters.AddWithValue("@ReportStatus", ReportStatus);
                        cmd.Parameters.AddWithValue("@ActivityID", activityID);
                        cmd.Parameters.AddWithValue("@ReportedQuantity", quantityToReport);
                        cmd.ExecuteNonQuery();
                    }

                    // 🔹 3️⃣ Update BorrowedItems
                    int newBorrowedQuantity = borrowedQuantity - quantityToReport;

                    if (newBorrowedQuantity == 0)
                    {
                        string deleteQuery = "DELETE FROM BorrowedItems WHERE Item_ID = @ItemID";

                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ItemID", itemID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string updateQuery = @"
                    UPDATE BorrowedItems
                    SET Borrowed_Quantity = @NewQuantity
                    WHERE Item_ID = @ItemID";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NewQuantity", newBorrowedQuantity);
                            cmd.Parameters.AddWithValue("@ItemID", itemID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();

                    ReportSubmitted?.Invoke();
                    MessageBox.Show("Report submitted successfully.");
                    this.Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }



    }
}
