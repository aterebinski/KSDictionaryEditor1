using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
        private string dictionaryId;
        string personel;
        string personelId;
        string service;
        string layout;
        string dictionary;
        //bool isEdited;
        WindowMode windowMode;

        ListView ListView_Entries;

        public enum WindowMode{
            Add,
            Edit,
            Delete
        }

        public EntryWindow(FbConnection connection, ListView ListView_Entries, string dictionaryId, WindowMode windowMode)
        {
            InitializeComponent();
            this.ListView_Entries= ListView_Entries;
            this.windowMode = windowMode;

            this.dictionaryId = dictionaryId;
            this.connection = connection;
            DictionaryItem.Focus();

            
            string slownikSql = "select s.idprac as idprac, s.nazw as slownik, w.nazw as wzorzec, u.nazw as usluga from slow s " +
                    "join wzfo w on s.idwzfo = w.id " +
                    "join uslg u on u.id = w.iduslg  " +
                    "where s.id = @id";
                    
            try
            {

                FbCommand dictionaryCommand = new FbCommand(slownikSql, connection);
                dictionaryCommand.Parameters.AddWithValue("@id", dictionaryId);
                FbDataAdapter dictionaryAdapter = new FbDataAdapter(dictionaryCommand);
                DataTable dictionaryTable = new DataTable();
                dictionaryAdapter.Fill(dictionaryTable);

                this.personelId = dictionaryTable.Rows[0]["IDPRAC"].ToString();
                this.service = dictionaryTable.Rows[0]["USLUGA"].ToString();
                this.layout = dictionaryTable.Rows[0]["WZORZEC"].ToString();
                this.dictionary = dictionaryTable.Rows[0]["SLOWNIK"].ToString();

                if (personelId == "0")
                {
                    personel = "<<WSPÓLNY SŁOWNIK>>";
                }
                else
                {
                    string personelSql = "select imie||' '||nazw as IMIENAZW from prac where id = @id";
                    FbCommand personelCommand = new FbCommand(personelSql, connection);
                    personelCommand.Parameters.AddWithValue("@id", personelId);
                    FbDataAdapter personelAdapter = new FbDataAdapter(personelCommand);
                    DataTable personelTable = new DataTable();
                    personelAdapter.Fill(personelTable);
                    personel = personelTable.Rows[0]["IMIENAZW"].ToString();
                }

                if(this.windowMode==WindowMode.Edit) //jesli edytujemy
                {
                    DictionaryItem.Text = (string)this.ListView_Entries.SelectedValue;
                }

                this.textBlockPracownik.Text = personel;
                this.textBlockUsluga.Text = service;
                this.textBlockWzorzec.Text = layout;
                this.textBlockSlownik.Text = dictionary;
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
            if (this.windowMode == WindowMode.Edit) //edytujemy element
            {
                string description = "";
                int i= 0;

                foreach (var item in this.ListView_Entries.Items)
                {
                    if (ListView_Entries.SelectedIndex == i)
                    {
                        description += DictionaryItem.Text + AsciiConverter.HEX2ASCII("0D0A");
                    }
                    else
                    {
                        description += item.ToString() + AsciiConverter.HEX2ASCII("0D0A");
                    }
                    
                    i++;
                }
                string updateSql = "update slow set opis = @opis where id = @id";
                FbCommand updateCommand = new FbCommand(updateSql, connection);
                updateCommand.Parameters.AddWithValue("@opis", description);
                updateCommand.Parameters.AddWithValue("@id", dictionaryId);

                Trace.WriteLine(updateCommand);

                connection.Open();
                updateCommand.ExecuteNonQuery();
                connection.Close();


            }
            else if(this.windowMode == WindowMode.Add)//dodajemy element
            {
                try
                {
                    string sql = "select opis from slow where id = @id";
                    FbCommand command = new FbCommand(sql, connection);
                    command.Parameters.AddWithValue("@id", dictionaryId);
                    FbDataAdapter adapter = new FbDataAdapter(command);
                    DataTable slowDataTable = new DataTable();
                    adapter.Fill(slowDataTable);
                    string opis = slowDataTable.Rows[0]["OPIS"].ToString();
                    opis = opis + DictionaryItem.Text + AsciiConverter.HEX2ASCII("0D0A");
                    string updateSql = "update slow set opis = @opis where id = @id";
                    FbCommand updateCommand = new FbCommand(updateSql, connection);
                    updateCommand.Parameters.AddWithValue("@opis", opis);
                    updateCommand.Parameters.AddWithValue("@id", dictionaryId);
                    connection.Open();
                    updateCommand.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            

            this.Close();
            
        }
    }
}
