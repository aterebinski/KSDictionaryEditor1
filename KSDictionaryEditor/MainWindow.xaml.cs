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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KSDictionaryEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FbConnection connection;

        public MainWindow()
        {
            InitializeComponent();

            ConnectionWindow connectionWindow = new ConnectionWindow();
            connectionWindow.ShowDialog();

            string connectionString = connectionWindow.ConnectionString;

            if(connectionString == "")
            {
                this.Close();
            }

            //string path = KsConnector.getKsPlIniFilePath();

            //string connectionString =  KsConnector.getConnectionString(path);


            //            MessageBox.Show(connectionString);

            connection = new FbConnection(connectionString);

            ShowPersonel(Personel_Left);
            ShowDictionaries(Dictonaries_Left);
            //ShowPersonel(Personel_Right);
        }

        public void ShowPersonel(ListView panel)
        {
            try
            {
                string query = "select id, imie, nazw from prac where del=0";
                FbDataAdapter adapter = new FbDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                panel.ItemsSource = table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ShowPersonel: "+ex.ToString());
            }
            
            
        }

        public void ShowDictionaries(ListView panel)
        {
            ListView lvPersonel;
            CheckBox checkSharedDictonaries;
            

            switch (panel.Name)
            {
                default:
                    lvPersonel = Personel_Left;
                    checkSharedDictonaries = SharedDictionaries_Left;
                    break;
                case "DictionariesLeft":
                    lvPersonel = Personel_Left;
                    checkSharedDictonaries = SharedDictionaries_Left;
                    break;
            }

            try
            {
                string query = "select u.kod, u.id as u_id,  u.nazw as usluga," +
                " w.id as w_id,  w.nazw as wzorzec," +
                " s.id as s_id, s.nazw as slownik, s.opis," +
                " p.imie||' '||p.nazw as pracownik" +
                " from uslg u" +
                " join wzfo w on w.iduslg = u.id" +
                " join slow s on s.idwzfo = w.id" +
                " join prac p on p.id = s.idprac" +
                " where w.del = 0 and s.del = 0 and s.usun = 0" +
                " and s.idprac in (";

                FbCommand command = new FbCommand(query, connection);



                if (checkSharedDictonaries.IsChecked == true)
                {
                    query = query + "0,";
                }
                if (lvPersonel.SelectedItems.Count > 0)
                {
                    //command.Parameters.AddWithValue()
                    foreach (DataRowView item in lvPersonel.SelectedItems)
                    {
                        query = query + item["id"].ToString() + ",";
                    }


                }
                query = query + "-99) order by usluga, wzorzec, slownik";

                command.CommandText = query;

                //MessageBox.Show(query);
                Trace.WriteLine(query);


                FbDataAdapter adapter = new FbDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);
                panel.ItemsSource = table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ShowDictionaries: " + ex.ToString());
            }

            
        }

        private void ShowElements(ListView panel)
        {

            try
            {
                DataRowView drv = Dictonaries_Left.SelectedItem as DataRowView;
                if (drv != null)
                {
                    string id = drv["S_ID"].ToString();

                    string query = "select * from slow where id = @id";
                    FbCommand command = new FbCommand(query, connection);
                    command.Parameters.AddWithValue("@id", id);

                    FbDataAdapter adapter = new FbDataAdapter(command);

                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    string info = table.Rows[0].Field<string>("opis").ToString();

                    /*
                    string[] podzielone = info.Split(
                            new string[] { "\r\n", "\r", "\n" }, 
                            StringSplitOptions.None
                        );
                        */

                    string[] podzielone = info.Split(
                            //new string[] { AsciiConverter.HEX2ASCII("0D0A"), AsciiConverter.HEX2ASCII("0D"), AsciiConverter.HEX2ASCII("0A") },
                            new string[] { AsciiConverter.HEX2ASCII("0D0A") },
                            StringSplitOptions.None
                        );

                    List<string> allItems = new List<string>();

                    foreach (string item in podzielone)
                    {
                        //MessageBox.Show(AsciiConverter.ASCIITOHex(item));
                        string newItem = item.Replace("˙", AsciiConverter.HEX2ASCII("0D0A"));
                        allItems.Add(newItem);
                    }
                    
                    //panel.ItemsSource = table.DefaultView;
                    panel.ItemsSource = allItems;
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("showSlow: " + ex.ToString());
            }
            

            //MessageBox.Show("Id = " + id);
        }

        private void Personel_Left_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowDictionaries(Dictonaries_Left);
        }

        private void SharedDictionaries_Left_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(Dictonaries_Left);
        }

        private void ClearPersonel_Left_Click(object sender, RoutedEventArgs e)
        {
            Personel_Left.UnselectAll();
            ShowDictionaries(Dictonaries_Left);
        }

        private void AllPersonel_Left_Click(object sender, RoutedEventArgs e)
        {
            Personel_Left.SelectAll();
            ShowDictionaries(Dictonaries_Left);
        }

        private void Dictonaries_Left_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowElements(Items_Left);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ItemWindow itemWindow = new ItemWindow();
            itemWindow.ShowDialog();
            MessageBox.Show(itemWindow.DictionaryItem.Text);
        }
    }
}
