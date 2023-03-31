using FirebirdSql.Data.FirebirdClient;
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
using System.Windows.Shapes;

namespace KSDictionaryEditor
{
    /// <summary>
    /// Interaction logic for DictionaryItemWindow.xaml
    /// </summary>
    public partial class ItemWindow : Window
    {
        FbConnection connection;
        private int idSlownika;

        public ItemWindow(FbConnection connection, string Usluga, string Wzorzec, string Slownik, string Pracownik, int idSlownika)
        {
            InitializeComponent();
            this.textBlockPracownik.Text = Pracownik;
            this.textBlockUsluga.Text = Usluga;
            this.textBlockWzorzec.Text = Wzorzec;
            this.textBlockSlownik.Text = Slownik;
            this.idSlownika = idSlownika;
            this.connection = connection;
            DictionaryItem.Focus();
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sql = "select opis from slow where id = @id";
                FbCommand command = new FbCommand(sql, connection);
                command.Parameters.AddWithValue("@id", idSlownika);
                FbDataAdapter adapter = new FbDataAdapter(command);
                DataTable slowDataTable = new DataTable();
                adapter.Fill(slowDataTable);
                string opis = slowDataTable.Rows[0]["OPIS"].ToString();
                opis = opis + DictionaryItem.Text + AsciiConverter.HEX2ASCII("0D0A");
                string updateSql = "update slow set opis = @opis where id = @id";
                FbCommand updateCommand = new FbCommand(updateSql, connection);
                updateCommand.Parameters.AddWithValue("@opis", opis);
                updateCommand.Parameters.AddWithValue("@id", idSlownika);
                connection.Open();
                updateCommand.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            //adapter.Fill()
            //string newItem = item.Replace("˙", AsciiConverter.HEX2ASCII("0D0A"));
            this.Close();
        }
    }
}
