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

namespace KSDictionaryEditor
{
    /// <summary>
    /// Interaction logic for Password.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        public PasswordWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AnulujButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
            //((Window)this.Parent).Close();
            App.Current.Shutdown();
        }

        private void Password_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this.Close();
            }
        }
    }
}
