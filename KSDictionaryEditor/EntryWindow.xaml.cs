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
    /// Interaction logic for DictionaryDictionaryEntryWindow.xaml
    /// </summary>
    public partial class EntryWindow : Window
    {
        FbConnection connection;
        private string DictionaryId;
        string personel;
        string service;
        string layout;
        string dictionary;
        bool isEdited;

        ListView ListView_Entries;


        /*
        public EntryWindow(FbConnection connection, string Usluga, string Wzorzec, string Slownik, string Pracownik, int idSlownika)
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
        */

        //public EntryWindow(FbConnection connection, string Usluga, string Wzorzec, string Slownik, string Pracownik, int idSlownika)
        public EntryWindow(FbConnection connection, ListView ListView_Entries, string DictionaryId, bool isEdited)
        {
            InitializeComponent();
            this.isEdited = isEdited;

            this.DictionaryId = DictionaryId;
            this.connection = connection;
            DictionaryItem.Focus();

            
            string slownikSql = "select s.idprac as idprac, s.nazw as slownik, w.nazw as wzorzec, u.nazw as usluga from slow s " +
                    "join wzfo w on s.idwzfo = w.id " +
                    "join uslg u on u.id = w.iduslg  " +
                    "where s.id = @id";
                    
            try
            {
                //string sIdSlownika = ((DataRowView)P1_ListView_Dictionaries.SelectedItems[0]).Row["S_ID"].ToString();

                //idSlownika = Convert.ToInt32(sIdSlownika);

                FbCommand dictionaryCommand = new FbCommand(slownikSql, connection);
                dictionaryCommand.Parameters.AddWithValue("@id", DictionaryId);
                FbDataAdapter dictionaryAdapter = new FbDataAdapter(dictionaryCommand);
                DataTable dictionaryTable = new DataTable();
                dictionaryAdapter.Fill(dictionaryTable);

                //string idPracownika = from entry in ListView_Dictionaries.SelectedItems where entry.Item
                this.service = dictionaryTable.Rows[0]["USLUGA"].ToString();
                this.layout = dictionaryTable.Rows[0]["WZORZEC"].ToString();
                this.dictionary = dictionaryTable.Rows[0]["SLOWNIK"].ToString();

                if (idPracownika == "0")
                {
                    pracownik = "<<WSPÓLNY SŁOWNIK>>";
                }
                else
                {
                    string pracownikSql = "select imie||' '||nazw as IMIENAZW from prac where id = @id";
                    FbCommand pracownikCommand = new FbCommand(pracownikSql, connection);
                    pracownikCommand.Parameters.AddWithValue("@id", idPracownika);
                    FbDataAdapter pracownikAdapter = new FbDataAdapter(pracownikCommand);
                    DataTable pracownikTable = new DataTable();
                    pracownikAdapter.Fill(pracownikTable);
                    pracownik = pracownikTable.Rows[0]["IMIENAZW"].ToString();
                }

                //DictionaryEntryWindow DictionaryEntryWindow = new DictionaryEntryWindow(connection, usluga, wzorzec, slownik, pracownik, idSlownika);
                DictionaryEntryWindow DictionaryEntryWindow = new DictionaryEntryWindow(connection, usluga, wzorzec, slownik, pracownik, idSlownika);
                DictionaryEntryWindow.ShowDialog();
                //MessageBox.Show(DictionaryEntryWindow.DictionaryItem.Text);


                this.textBlockPracownik.Text = Pracownik;
                this.textBlockUsluga.Text = Usluga;
                this.textBlockWzorzec.Text = Wzorzec;
                this.textBlockSlownik.Text = Slownik;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            
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
