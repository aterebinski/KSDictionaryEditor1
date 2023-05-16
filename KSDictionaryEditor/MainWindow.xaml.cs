﻿using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ObservableCollection<string> DictionaryP2_ListView_Items = new ObservableCollection<string>();
        ObservableCollection<string> DictionaryP1_ListView_Items = new ObservableCollection<string>();
        ObservableCollection<Pracownik> PersonelP2_ListView_Items = new ObservableCollection<Pracownik>();
        ObservableCollection<Pracownik> PersonelP1_ListView_Items = new ObservableCollection<Pracownik>();

        public MainWindow()
        {
            InitializeComponent();

            ConnectionWindow connectionWindow = new ConnectionWindow();
            connectionWindow.ShowDialog();

            try
            {
                connectionString = connectionWindow.ConnectionString;
            }
            catch (Exception)
            {
                App.Current.Shutdown();
            }

            if (connectionString != "" && connectionWindow.TestConnection())
            {
                connection = new FbConnection(connectionString);

                ShowPersonel(P2_ListView_Personel, PersonelP2_ListView_Items);
                ShowDictionaries(P2_ListView_Dictionaries);
                ShowPersonel(P1_ComboBox_Personel, PersonelP1_ListView_Items);
                ShowDictionaries(P1_ListView_Dictionaries);
                connection.Close();
            }
            else
            {
                this.Close();
            }
        }

        //Pokaz personel
        private void ShowPersonel(Selector panel, ObservableCollection<Pracownik> PersonelItemsList)
        {
            try
            {
                connection.Open();

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
            finally
            {
                connection.Close();
            }
        }

        private void ShowPersonelAlternate(Selector panel, ObservableCollection<Pracownik> PersonelItemsList)
        {
            try
            {
                connection.Open();

                string query = "select id, imie, nazw, imie||' '||nazw as imienazw from prac where del=0";

                FbCommand command = new FbCommand(query, connection);
                FbDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int Id = reader.GetInt32(reader.GetOrdinal("Id"));
                    string Imie = reader.GetString(reader.GetOrdinal("Imie"));
                    string Nazw = reader.GetString(reader.GetOrdinal("Nazw"));

                    PersonelItemsList.Add(new Pracownik()
                    {
                        IsSelected = false,
                        Id = Id,
                        Imie = Imie,
                        Nazw = Nazw,
                        ImieNazw = Imie + " " + Nazw
                    });
                };

                panel.ItemsSource = PersonelItemsList;
                panel.DisplayMemberPath = "ImieNazw";
                panel.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("ShowPersonel: " + ex.ToString());
            }
            finally
            {
                connection.Close();
            }
        }

        //Pokaz Slowniki
        private void ShowDictionaries(ListView panel)
        {
            Selector lvPersonel;
            CheckBox checkSharedDictonaries;

            string filter = "";

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
                case "P2_ListView_Dictionaries":
                    if (P2_ListView_Personel.SelectedItems.Count > 0)
                    {
                        foreach (DataRowView item in P2_ListView_Personel.SelectedItems)
                        {
                            query = query + item["id"].ToString() + ",";
                        }
                    }
                    if (P2_CheckBox_SharedDictionaries.IsChecked == true)
                    {
                        query = query + "0,";
                    }

                    if (P2_TextBox_FiltrDictionary.Text.Length > 0)
                    {
                        switch (P2_ComboBox_FiltrDictionary.SelectedIndex)
                        {
                            case 0: //wszystko
                                filter = " and (upper(u.nazw) like '%" + P2_TextBox_FiltrDictionary.Text.ToUpper() + "%' ";
                                filter += " or upper(w.nazw) like '%" + P2_TextBox_FiltrDictionary.Text.ToUpper() + "%' ";
                                filter += " or upper(s.nazw) like '%" + P2_TextBox_FiltrDictionary.Text.ToUpper() + "%') ";
                                break;
                            case 1: //Słownik
                                filter = " and upper(s.nazw) like '%" + P2_TextBox_FiltrDictionary.Text.ToUpper() + "%' ";
                                break;
                            case 2: //Wzorzec
                                filter = " and upper(w.nazw) like '%" + P2_TextBox_FiltrDictionary.Text.ToUpper() + "%' ";
                                break;
                            case 3: //Usługa
                                filter = " and upper(u.nazw) like '%" + P2_TextBox_FiltrDictionary.Text.ToUpper() + "%' ";
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                case "P1_ListView_Dictionaries":

                    if (P1_CheckBox_SharedDictionaries.IsChecked == true)
                    {
                        query = query + "0,";
                    }
                    else
                    {
                        if (P1_ComboBox_Personel.SelectedValue != null)
                        {
                            query = query + P1_ComboBox_Personel.SelectedValue + ",";
                            //MessageBox.Show(P1_ComboBox_Personel.SelectedValue.ToString());
                        }
                    }

                    if (P1_TextBox_FiltrDictionary.Text.Length > 0)
                    {
                        switch (P1_ComboBox_FiltrDictionary.SelectedIndex)
                        {
                            case 0: //wszystko
                                filter = " and (upper(u.nazw) like '%" + P1_TextBox_FiltrDictionary.Text.ToUpper() + "%' ";
                                filter += " or upper(w.nazw) like '%" + P1_TextBox_FiltrDictionary.Text.ToUpper() + "%' ";
                                filter += " or upper(s.nazw) like '%" + P1_TextBox_FiltrDictionary.Text.ToUpper() + "%') ";
                                break;
                            case 1: //Słownik
                                filter = " and upper(s.nazw) like '%" + P1_TextBox_FiltrDictionary.Text.ToUpper() + "%' ";
                                break;
                            case 2: //Wzorzec
                                filter = " and upper(w.nazw) like '%" + P1_TextBox_FiltrDictionary.Text.ToUpper() + "%' ";
                                break;
                            case 3: //Usługa
                                filter = " and upper(u.nazw) like '%" + P1_TextBox_FiltrDictionary.Text.ToUpper() + "%' ";
                                break;
                            default:
                                break;
                        }
                    }

                    lvPersonel = P1_ComboBox_Personel;
                    checkSharedDictonaries = P1_CheckBox_SharedDictionaries;
                    break;
            }

            try
            {
                FbCommand command = new FbCommand(query, connection);

                query = query + "-99) " + filter + " order by usluga, wzorzec, slownik";
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
        private void ShowElements(ListView itemsListView, ObservableCollection<string> itemsList, ListView dictionariesListView)
        {
            try
            {
                itemsList.Clear();

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

                    foreach (string item in podzielone)
                    {
                        //MessageBox.Show(AsciiConverter.ASCIITOHex(item));
                        string newItem = item.Replace("˙", AsciiConverter.HEX2ASCII("0D0A"));
                        //allItems.Add(newItem);
                        itemsList.Add(newItem);
                    }
                    itemsListView.ItemsSource = itemsList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("showSlow: " + ex.ToString());
            }
        }

        private void SaveElements(ListView itemsListView, ObservableCollection<string> itemsList, ListView dictionariesListView)
        {
            string opis = "", tempOpis = "";

            foreach (string item in itemsList)
            {
                tempOpis = item.Replace(AsciiConverter.HEX2ASCII("0D0A"), "˙");
                opis += tempOpis + AsciiConverter.HEX2ASCII("0D0A");
            }

            DataRowView drv = dictionariesListView.SelectedItem as DataRowView;
            string id = drv["s_id"].ToString();

            string sql = "update slow set opis = @opis where id = @id";

            FbCommand command = new FbCommand(sql, connection);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        private void UpdatePersonel_TextBox_P2()
        {
            int printedElements = 0;
            if (P2_CheckBox_SharedDictionaries.IsChecked == true)
            {
                P2_TextBlock_Personel.Text = "<WSPÓŁNE SŁOWNIKI>";
                printedElements++;
            }
            else
            {
                P2_TextBlock_Personel.Text = "";
            }

            if (P2_ListView_Personel.SelectedItems.Count > 0)
            {
                try
                {
                    //P1_TextBlock_Personel.Text = P1_ComboBox_Personel.SelectedValue
                    string sql = "select imie||' '||nazw as imienazw from prac where id in(-99";
                    int i = 0;

                    foreach (DataRowView item in P2_ListView_Personel.SelectedItems)
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
                            P2_TextBlock_Personel.Text += ", ";
                        }

                        if (printedElements > 3)
                        {
                            P2_TextBlock_Personel.Text += "<I INNI ...>";
                            break;
                        }

                        printedElements++;
                        P2_TextBlock_Personel.Text += item["IMIENAZW"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void UpdatePersonel_TextBox_P1()
        {
            if (P1_CheckBox_SharedDictionaries.IsChecked == true)
            {
                P1_TextBlock_Personel.Text = "<WSPÓŁNE SŁOWNIKI>";
            }
            else
            {
                P1_TextBlock_Personel.Text = "";
                if (P1_ComboBox_Personel.SelectedValue != null)
                {
                    try
                    {
                        //P1_TextBlock_Personel.Text = P1_ComboBox_Personel.SelectedValue
                        string sql = "select imie||' '||nazw as imienazw from prac where id  = @id";
                        FbCommand command = new FbCommand(sql, connection);
                        command.Parameters.AddWithValue("@id", P1_ComboBox_Personel.SelectedValue);
                        FbDataAdapter dataAdapter = new FbDataAdapter(command);
                        DataTable PersonelTable = new DataTable();
                        dataAdapter.Fill(PersonelTable);
                        P1_TextBlock_Personel.Text = PersonelTable.Rows[0]["IMIENAZW"].ToString();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }

        private void P2_ListView_Personel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowDictionaries(P2_ListView_Dictionaries);
            ShowElements(P2_ListView_Items, DictionaryP2_ListView_Items, P2_ListView_Dictionaries);
            UpdatePersonel_TextBox_P2();
        }

        private void P2_CheckBox_SharedDictionaries_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(P2_ListView_Dictionaries);
            ShowElements(P2_ListView_Items, DictionaryP2_ListView_Items, P2_ListView_Dictionaries);
            UpdatePersonel_TextBox_P2();
        }

        private void P2_CheckBox_ClearPersonel_Click(object sender, RoutedEventArgs e)
        {
            P2_ListView_Personel.UnselectAll();
            ShowDictionaries(P2_ListView_Dictionaries);
            ShowElements(P2_ListView_Items, DictionaryP2_ListView_Items, P2_ListView_Dictionaries);
        }

        private void P2_Button_AllPersonel_Click(object sender, RoutedEventArgs e)
        {
            P2_ListView_Personel.SelectAll();
            ShowDictionaries(P2_ListView_Dictionaries);
            ShowElements(P2_ListView_Items, DictionaryP2_ListView_Items, P2_ListView_Dictionaries);
        }

        private void P2_ListView_Dictionaries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowElements(P2_ListView_Items, DictionaryP2_ListView_Items, P2_ListView_Dictionaries);
        }

        private void P1_Button_AddElement_Click(object sender, RoutedEventArgs e)
        {
            string usluga, pracownik, wzorzec, slownik;
            string idPracownika;
            int idSlownika;
            try
            {
                string sIdSlownika = ((DataRowView)P2_ListView_Dictionaries.SelectedItems[0]).Row["S_ID"].ToString();

                idSlownika = Convert.ToInt32(sIdSlownika);

                string slownikSql = "select s.idprac as idprac, s.nazw as slownik, w.nazw as wzorzec, u.nazw as usluga from slow s " +
                    "join wzfo w on s.idwzfo = w.id " +
                    "join uslg u on u.id = w.iduslg  " +
                    "where s.id = @id";

                FbCommand slownikCommand = new FbCommand(slownikSql, connection);
                slownikCommand.Parameters.AddWithValue("@id", sIdSlownika);
                FbDataAdapter slownikAdapter = new FbDataAdapter(slownikCommand);
                DataTable slownikTable = new DataTable();
                slownikAdapter.Fill(slownikTable);

                idPracownika = slownikTable.Rows[0]["IDPRAC"].ToString();
                usluga = slownikTable.Rows[0]["USLUGA"].ToString();
                wzorzec = slownikTable.Rows[0]["WZORZEC"].ToString();
                slownik = slownikTable.Rows[0]["SLOWNIK"].ToString();

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

                ItemWindow itemWindow = new ItemWindow(connection, usluga, wzorzec, slownik, pracownik, idSlownika);
                itemWindow.ShowDialog();
                //MessageBox.Show(itemWindow.DictionaryItem.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void P1_Personel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("Changed_Personel");
            ShowDictionaries(P1_ListView_Dictionaries);
            ShowElements(P1_ListView_Items, DictionaryP1_ListView_Items, P1_ListView_Dictionaries);
            UpdatePersonel_TextBox_P1();
        }

        private void P1_CheckBox_SharedDictionaries_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Changed_Checkbox");
            P1_ComboBox_Personel.IsEnabled = !(bool)P1_CheckBox_SharedDictionaries.IsChecked;
            ShowDictionaries(P1_ListView_Dictionaries);
            ShowElements(P1_ListView_Items, DictionaryP1_ListView_Items, P1_ListView_Dictionaries);
            //clear combobox
            P1_ComboBox_Personel.SelectedIndex = -1;
            UpdatePersonel_TextBox_P1();
        }

        private void P1_ListView_Dictionaries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowElements(P1_ListView_Items, DictionaryP1_ListView_Items, P1_ListView_Dictionaries);
        }

        private void P2_Button_SelectAllItems_Click(object sender, RoutedEventArgs e)
        {
            P2_ListView_Items.SelectAll();
        }

        private void P2_Button_UnselectAllItems_Click(object sender, RoutedEventArgs e)
        {
            P2_ListView_Items.UnselectAll();
        }

        private void P1_Button_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            P1_ListView_Items.SelectAll();
        }

        private void P1_Button_UnselectAll_Click(object sender, RoutedEventArgs e)
        {
            P1_ListView_Items.UnselectAll();
        }

        private void P2_Button_FiltrDictionary_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(P2_ListView_Dictionaries);
        }

        private void P1_Button_FiltrDictionary_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(P1_ListView_Dictionaries);
        }

        private void P2_TextBox_FiltrDictionary_KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowDictionaries(P2_ListView_Dictionaries);
            }
        }

        private void P1_TextBox_FiltrDictionary_KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowDictionaries(P1_ListView_Dictionaries);
            }
        }

        private void SwitchSidesButton_Click(object sender, RoutedEventArgs e)
        {
            var P2_ListView_Personel_SelectedItems = P2_ListView_Personel.SelectedItems;
            var P2_ListView_Dictionaries_SelectedItems = P2_ListView_Dictionaries.SelectedItems;
            var P2_ListView_Items_SelectedItems = P2_ListView_Items.SelectedItems;

            var P1_ComboBox_Personel_SelectedItem = P1_ComboBox_Personel.SelectedItem;
            var P1_ListView_Dictionaries_SelectedItems = P1_ListView_Dictionaries.SelectedItems;
            var P1_ListView_Items_SelectedItems = P1_ListView_Items.SelectedItems;


            //P2_ListView_Personel.SelectedItems = P1_ComboBox_Personel.SelectedItems;
            //P2_ListView_Dictionaries.SelectedItems = P1_ListView_Dictionaries.SelectedItems;
            //P2_ListView_Items.SelectedItems = P1_ListView_Items.SelectedItems;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void P1_CopyDictionary_Click(object sender, RoutedEventArgs e)
        {

        }

        private void P2_CopyDictionary_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (P2_ListView_Dictionaries.SelectedItems.Count > 0)
            {
                CopyDictionaryWindow CopyDictionaryWindow = new CopyDictionaryWindow(P2_ListView_Dictionaries, P1_ListView_Dictionaries, P1_ComboBox_Personel, clickedButton.Name);
                CopyDictionaryWindow.ShowDialog();

            }



        }
    }
}
