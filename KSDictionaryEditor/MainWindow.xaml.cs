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
        ObservableCollection<string> DictionaryItems_BigPanel = new ObservableCollection<string>();
        ObservableCollection<string> DictionaryItems_SmallPanel = new ObservableCollection<string>();

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


                ShowPersonel(Personel_BigPanel);
                ShowDictionaries(Dictionaries_BigPanel);
                ShowPersonel(Personel_SmallPanel);
                ShowDictionaries(Dictionaries_SmallPanel);
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
                case "Dictionaries_BigPanel":
                    if (Personel_BigPanel.SelectedItems.Count > 0)
                    {
                        //command.Parameters.AddWithValue()
                        foreach (DataRowView item in Personel_BigPanel.SelectedItems)
                        {
                            query = query + item["id"].ToString() + ",";
                        }
                    }
                    if (SharedDictionaries_BigPanel.IsChecked == true)
                    {
                        query = query + "0,";
                    }

                    if (FiltrDictionarySet1TextBox.Text.Length>0)
                    {
                        switch (FiltrDictionarySet1Combobox.SelectedIndex)
                        {
                            case 0: //wszystko
                                filter = " and (upper(u.nazw) like '%"+FiltrDictionarySet1TextBox.Text.ToUpper()+"%' ";
                                filter += " or upper(w.nazw) like '%" + FiltrDictionarySet1TextBox.Text.ToUpper() + "%' ";
                                filter += " or upper(s.nazw) like '%" + FiltrDictionarySet1TextBox.Text.ToUpper() + "%') ";
                                break;
                            case 1: //Słownik
                                filter = " and upper(s.nazw) like '%" + FiltrDictionarySet1TextBox.Text.ToUpper() + "%' ";
                                break;
                            case 2: //Wzorzec
                                filter = " and upper(w.nazw) like '%" + FiltrDictionarySet1TextBox.Text.ToUpper() + "%' ";
                                break;
                            case 3: //Usługa
                                filter = " and upper(u.nazw) like '%" + FiltrDictionarySet1TextBox.Text.ToUpper() + "%' ";
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                    
                case "Dictionaries_SmallPanel":

                    if (SharedDictionaries_SmallPanel.IsChecked == true)
                    {
                        query = query + "0,";
                    }
                    else
                    {
                        if (Personel_SmallPanel.SelectedValue != null)
                        {
                            query = query + Personel_SmallPanel.SelectedValue + ",";
                            //MessageBox.Show(Personel_SmallPanel.SelectedValue.ToString());
                        }
                    }

                    if (FiltrDictionarySet2TextBox.Text.Length > 0)
                    {
                        switch (FiltrDictionarySet2Combobox.SelectedIndex)
                        {
                            case 0: //wszystko
                                filter = " and (upper(u.nazw) like '%" + FiltrDictionarySet2TextBox.Text.ToUpper() + "%' ";
                                filter += " or upper(w.nazw) like '%" + FiltrDictionarySet2TextBox.Text.ToUpper() + "%' ";
                                filter += " or upper(s.nazw) like '%" + FiltrDictionarySet2TextBox.Text.ToUpper() + "%') ";
                                break;
                            case 1: //Słownik
                                filter = " and upper(s.nazw) like '%" + FiltrDictionarySet2TextBox.Text.ToUpper() + "%' ";
                                break;
                            case 2: //Wzorzec
                                filter = " and upper(w.nazw) like '%" + FiltrDictionarySet2TextBox.Text.ToUpper() + "%' ";
                                break;
                            case 3: //Usługa
                                filter = " and upper(u.nazw) like '%" + FiltrDictionarySet2TextBox.Text.ToUpper() + "%' ";
                                break;
                            default:
                                break;
                        }
                    }

                    lvPersonel = Personel_SmallPanel;
                    checkSharedDictonaries = SharedDictionaries_SmallPanel;
                    break;
            }

            try
            {
                FbCommand command = new FbCommand(query, connection);

                query = query + "-99) "+filter+" order by usluga, wzorzec, slownik";
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
                tempOpis = item.Replace(AsciiConverter.HEX2ASCII("0D0A"),"˙");
                opis += tempOpis+AsciiConverter.HEX2ASCII("0D0A");
            }

            DataRowView drv = dictionariesListView.SelectedItem as DataRowView;
            string id = drv["s_id"].ToString();

            string sql = "update slow set opis = @opis where id = @id";

            FbCommand command = new FbCommand(sql, connection);
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        private void UpdatePersonel_TextBox_BigPanel()
        {
            int printedElements = 0;
            if (SharedDictionaries_BigPanel.IsChecked == true)
            {
                Personel_BigPanel_TextBlock.Text = "<WSPÓŁNE SŁOWNIKI>";
                printedElements++;
            }
            else
            {
                Personel_BigPanel_TextBlock.Text = "";
            }

            if (Personel_BigPanel.SelectedItems.Count > 0)
            {
                try
                {
                    //Personel_SmallPanel_TextBlock.Text = Personel_SmallPanel.SelectedValue
                    string sql = "select imie||' '||nazw as imienazw from prac where id in(-99";
                    int i = 0;

                    foreach (DataRowView item in Personel_BigPanel.SelectedItems)
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
                            Personel_BigPanel_TextBlock.Text += ", ";
                        }

                        if (printedElements > 3)
                        {
                            Personel_BigPanel_TextBlock.Text += "<I INNI ...>";
                            break;
                        }

                        printedElements++;
                        Personel_BigPanel_TextBlock.Text += item["IMIENAZW"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void UpdatePersonel_TextBox_SmallPanel()
        {
            if (SharedDictionaries_SmallPanel.IsChecked == true)
            {
                Personel_SmallPanel_TextBlock.Text = "<WSPÓŁNE SŁOWNIKI>";
            }
            else
            {
                Personel_SmallPanel_TextBlock.Text = "";
                if (Personel_SmallPanel.SelectedValue != null)
                {
                    try
                    {
                        //Personel_SmallPanel_TextBlock.Text = Personel_SmallPanel.SelectedValue
                        string sql = "select imie||' '||nazw as imienazw from prac where id  = @id";
                        FbCommand command = new FbCommand(sql, connection);
                        command.Parameters.AddWithValue("@id", Personel_SmallPanel.SelectedValue);
                        FbDataAdapter dataAdapter = new FbDataAdapter(command);
                        DataTable PersonelTable = new DataTable();
                        dataAdapter.Fill(PersonelTable);
                        Personel_SmallPanel_TextBlock.Text = PersonelTable.Rows[0]["IMIENAZW"].ToString();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }

        private void Personel_BigPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowDictionaries(Dictionaries_BigPanel);
            ShowElements(Items_BigPanel, DictionaryItems_BigPanel, Dictionaries_BigPanel);
            UpdatePersonel_TextBox_BigPanel();
        }

        private void SharedDictionaries_BigPanel_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(Dictionaries_BigPanel);
            ShowElements(Items_BigPanel, DictionaryItems_BigPanel, Dictionaries_BigPanel);
            UpdatePersonel_TextBox_BigPanel();
        }

        private void ClearPersonel_BigPanel_Click(object sender, RoutedEventArgs e)
        {
            Personel_BigPanel.UnselectAll();
            ShowDictionaries(Dictionaries_BigPanel);
            ShowElements(Items_BigPanel, DictionaryItems_BigPanel, Dictionaries_BigPanel);
        }

        private void AllPersonel_BigPanel_Click(object sender, RoutedEventArgs e)
        {
            Personel_BigPanel.SelectAll();
            ShowDictionaries(Dictionaries_BigPanel);
            ShowElements(Items_BigPanel, DictionaryItems_BigPanel, Dictionaries_BigPanel);
        }

        private void Dictionaries_BigPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowElements(Items_BigPanel, DictionaryItems_BigPanel, Dictionaries_BigPanel);
        }

        private void Set1AddElementButton_Click(object sender, RoutedEventArgs e)
        {
            string usluga, pracownik, wzorzec, slownik;
            string idPracownika;
            int idSlownika;
            try
            {
                string sIdSlownika = ((DataRowView)Dictionaries_BigPanel.SelectedItems[0]).Row["S_ID"].ToString();

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

        private void Personel_SmallPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("Changed_Personel");
            ShowDictionaries(Dictionaries_SmallPanel);
            ShowElements(Items_SmallPanel, DictionaryItems_SmallPanel, Dictionaries_SmallPanel);
            UpdatePersonel_TextBox_SmallPanel();
        }
        
        private void SharedDictionaries_SmallPanel_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Changed_Checkbox");
            Personel_SmallPanel.IsEnabled = !(bool)SharedDictionaries_SmallPanel.IsChecked;
            ShowDictionaries(Dictionaries_SmallPanel);
            ShowElements(Items_SmallPanel, DictionaryItems_SmallPanel, Dictionaries_SmallPanel);
            //clear combobox
            Personel_SmallPanel.SelectedIndex = -1;
            UpdatePersonel_TextBox_SmallPanel();
        }

        private void Dictionaries_SmallPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowElements(Items_SmallPanel, DictionaryItems_SmallPanel, Dictionaries_SmallPanel);
        }

        private void Set1SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            Items_BigPanel.SelectAll();
        }

        private void Set1UnSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            Items_BigPanel.UnselectAll();
        }

        private void Set2SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            Items_SmallPanel.SelectAll();
        }

        private void Set2UnSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            Items_SmallPanel.UnselectAll();
        }

        private void FiltrDictionarySet1Button_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(Dictionaries_BigPanel);
        }

        private void FiltrDictionarySet2Button_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(Dictionaries_SmallPanel);
        }

        private void FilterSet1KeyPressed(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ShowDictionaries(Dictionaries_BigPanel);
            }
        }

        private void FilterSet2KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowDictionaries(Dictionaries_SmallPanel);
            }
        }
    }
}
