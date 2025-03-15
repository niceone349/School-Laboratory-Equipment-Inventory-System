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

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Main.Content = new Home();
        }
        // product management page
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new ProductManagements();
        }
        // equipment tempalte apge
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Main.Content = new EquipmentTemplate();
        }
        //product categorization
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Main.Content = new ProductCategorization();
        }
        //inventory reporting
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Main.Content = new InventoryReport();
        }
        //Home 
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Main.Content = new Home();
        }
        //activity Log
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Main.Content = new ActivityLog();
        }
        // borrow
        private void BorrowButton_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new Borrow();
        }
        // logout button
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Login login = new Login();
            this.Close();
            login.ShowDialog();

        }
        // return button
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Main.Content = new Return();
        }
        //stock notif
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            Main.Content = new StockNotification();
        }
    }
}
