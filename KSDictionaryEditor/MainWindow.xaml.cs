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
using System.Windows.Controls.Primitives;
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
        string connectionString = "";

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                ConnectionWindow connectionWindow = new ConnectionWindow();
                connectionWindow.ShowDialog();
                connectionString = connectionWindow.ConnectionString;
            }
            catch (Exception e)
            {
                App.Current.Shutdown();
            }

            if (connectionString != "")
            {
                connection = new FbConnection(connectionString);

                ShowPersonel(Personel_Left);
                ShowDictionaries(Dictionaries_Left);
                ShowPersonel(Personel_Right);
                ShowDictionaries(Dictionaries_Right);
            }
            else
            {
                this.Close();
            }
        }

        //Pokaz personel
        public void ShowPersonel(Selector panel)
        {
            try
            {
                string query = "select id, imie, nazw, imie||' '||nazw as imienazw from prac where del=0";
                FbDataAdapter adapter = new FbDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);

                //if(panel is ListBox)
                panel.DisplayMemberPath = "IMIENAZW";
                panel.SelectedValuePath = "ID";
                panel.ItemsSource = table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ShowPersonel: " + ex.ToString());
            }
        }

        //Pokaz Slowniki
        private void ShowDictionaries(ListView panel)
        {
            Selector lvPersonel;
            CheckBox checkSharedDictonaries;

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

            switch (panel.Name)
            {
                default:
                    break;
                case "Dictionaries_Left":
                    if (Personel_Left.SelectedItems.Count > 0)
                    {
                        //command.Parameters.AddWithValue()
                        foreach (DataRowView item in Personel_Left.SelectedItems)
                        {
                            query = query + item["id"].ToString() + ",";
                        }
                    }
                    if (SharedDictionaries_Left.IsChecked == true)
                    {
                        query = query + "0,";
                    }
                    break;
                case "Dictionaries_Right":

                    if (SharedDictionaries_Right.IsChecked == true)
                    {
                        query = query + "0,";
                    }
                    else
                    {
                        if (Personel_Right.SelectedValue != null)
                        {
                            query = query + Personel_Right.SelectedValue + ",";
                            //MessageBox.Show(Personel_Right.SelectedValue.ToString());
                        }
                    }

                    lvPersonel = Personel_Right;
                    checkSharedDictonaries = SharedDictionaries_Right;
                    break;
            }

            try
            {
                FbCommand command = new FbCommand(query, connection);

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

        //pokaz elementy slownika 
        private void ShowElements(ListView itemsListView, ListView dictionariesListView)
        {

            try
            {
                DataTable table = new DataTable();

                DataRowView drv = dictionariesListView.SelectedItem as DataRowView;
                if (drv != null)
                {
                    string id = drv["S_ID"].ToString();

                    string query = "select * from slow where id = @id";
                    FbCommand command = new FbCommand(query, connection);
                    command.Parameters.AddWithValue("@id", id);

                    FbDataAdapter adapter = new FbDataAdapter(command);
                   
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
                    itemsListView.ItemsSource = allItems;
                }
                else //jesli nie ma zaznaczonych słowników to pokaz pusta liste
                {
                    itemsListView.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("showSlow: " + ex.ToString());
            }

            //MessageBox.Show("Id = " + id);
        }

        private void UpdatePersonel_TextBox_Left()
        {
            int printedElements = 0;
            if (SharedDictionaries_Left.IsChecked == true)
            {
                Personel_Left_TextBlock.Text = "<WSPÓŁNE SŁOWNIKI>";
                printedElements++;
            }
            else
            {
                Personel_Left_TextBlock.Text = "";
            }

            if (Personel_Left.SelectedItems.Count > 0)
            {
                try
                {
                    //Personel_Right_TextBlock.Text = Personel_Right.SelectedValue
                    string sql = "select imie||' '||nazw as imienazw from prac where id in(-99";
                    int i = 0;

                    foreach (DataRowView item in Personel_Left.SelectedItems)
                    {
                        sql += "," + item[i].ToString();
                    }
                    sql += ")";

                    FbCommand command = new FbCommand(sql, connection);
                    FbDataAdapter dataAdapter = new FbDataAdapter(command);
                    DataTable PersonelTable = new DataTable();
                    dataAdapter.Fill(PersonelTable);

                    foreach (DataRow item in PersonelTable.Rows)
                    {
                        if (printedElements > 0)
                        {
                            Personel_Left_TextBlock.Text += ", ";
                        }

                        if (printedElements > 3)
                        {
                            Personel_Left_TextBlock.Text += "<I INNI ...>";
                            break;
                        }

                        printedElements++;
                        Personel_Left_TextBlock.Text += item["IMIENAZW"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void UpdatePersonel_TextBox_Right()
        {
            if (SharedDictionaries_Right.IsChecked == true)
            {
                Personel_Right_TextBlock.Text = "<WSPÓŁNE SŁOWNIKI>";
            }
            else
            {
                Personel_Right_TextBlock.Text = "";
                if (Personel_Right.SelectedValue != null)
                {
                    try
                    {
                        //Personel_Right_TextBlock.Text = Personel_Right.SelectedValue
                        string sql = "select imie||' '||nazw as imienazw from prac where id  = @id";
                        FbCommand command = new FbCommand(sql, connection);
                        command.Parameters.AddWithValue("@id", Personel_Right.SelectedValue);
                        FbDataAdapter dataAdapter = new FbDataAdapter(command);
                        DataTable PersonelTable = new DataTable();
                        dataAdapter.Fill(PersonelTable);
                        Personel_Right_TextBlock.Text = PersonelTable.Rows[0]["IMIENAZW"].ToString();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }

        private void Personel_Left_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowDictionaries(Dictionaries_Left);
            ShowElements(Items_Left, Dictionaries_Left);
            UpdatePersonel_TextBox_Left();
        }

        private void SharedDictionaries_Left_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(Dictionaries_Left);
            ShowElements(Items_Left, Dictionaries_Left);
            UpdatePersonel_TextBox_Left();
        }

        private void ClearPersonel_Left_Click(object sender, RoutedEventArgs e)
        {
            Personel_Left.UnselectAll();
            ShowDictionaries(Dictionaries_Left);
            ShowElements(Items_Left, Dictionaries_Left);
        }

        private void AllPersonel_Left_Click(object sender, RoutedEventArgs e)
        {
            Personel_Left.SelectAll();
            ShowDictionaries(Dictionaries_Left);
            ShowElements(Items_Left, Dictionaries_Left);
        }

        private void Dictionaries_Left_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowElements(Items_Left, Dictionaries_Left);
        }

        private void AddElement_Button_Click(object sender, RoutedEventArgs e)
        {
            ItemWindow itemWindow = new ItemWindow();
            itemWindow.ShowDialog();
            //MessageBox.Show(itemWindow.DictionaryItem.Text);
        }

        private void Personel_Right_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("Changed_Personel");
            ShowDictionaries(Dictionaries_Right);
            ShowElements(Items_Right, Dictionaries_Right);
            UpdatePersonel_TextBox_Right();
        }


        private void SharedDictionaries_Right_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Changed_Checkbox");
            Personel_Right.IsEnabled = !(bool)SharedDictionaries_Right.IsChecked;
            ShowDictionaries(Dictionaries_Right);
            ShowElements(Items_Right, Dictionaries_Right);
            //clear combobox
            Personel_Right.SelectedIndex = -1;
            UpdatePersonel_TextBox_Right();
        }

        private void Dictionaries_Right_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowElements(Items_Right, Dictionaries_Right);
        }

    }
}
