using FirebirdSql.Data.FirebirdClient;
using System;
using System.Xml;
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
using Microsoft.Win32;
using System.Runtime.Remoting.Messaging;

namespace KSDictionaryEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FbConnection connection;
        string connectionString = "";
        ObservableCollection<string> P2_DictionaryItems = new ObservableCollection<string>();
        ObservableCollection<string> P1_DictionaryItems = new ObservableCollection<string>();
        ObservableCollection<Pracownik> P2_PersonelItems = new ObservableCollection<Pracownik>();
        ObservableCollection<Pracownik> P1_PersonelItems = new ObservableCollection<Pracownik>();


        //Id wybranych słowników poszczególnych Setów
        string P1_SelectedDictionaryId = "0";
        string P2_SelectedDictionaryId = "0";

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


            //test połączenia
            if (connectionString != "" && connectionWindow.TestConnection())
            {
                connection = new FbConnection(connectionString);

                ShowPersonel(P2_ListView_Personel, P2_PersonelItems);
                ShowDictionaries(P2_ListView_Dictionaries);
                ShowPersonel(P1_ComboBox_Personel, P1_PersonelItems);
                ShowDictionaries(P1_ListView_Dictionaries);
                ShowServices(P2_ListView_Services);
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

        //Pokaz Wzorce i Usługi
        private void ShowServices(ListView panel)
        {
            //Selector lvPersonel;
            //CheckBox checkSharedDictonaries;

            string filter = "";

            string query = "select u.kod, u.id as u_id,  u.nazw as usluga," +
                " w.id as w_id,  w.nazw as wzorzec " +
                " from uslg u" +
                " join wzfo w on w.iduslg = u.id" +
                " where w.del = 0 and u.del = 0 ";

            if (P2_TextBox_FiltrServices.Text.Length > 0) //jesli cokolwiek jest wpisane w filtrze
            {
                switch (P2_ComboBox_FiltrServices.SelectedIndex)
                {
                    case 0: //wszystko
                        filter = " and (upper(u.nazw) like '%" + P2_TextBox_FiltrServices.Text.ToUpper() + "%' ";
                        filter += " or upper(w.nazw) like '%" + P2_TextBox_FiltrServices.Text.ToUpper() + "%' )";
                        break;
                    case 1: //Wzorzec
                        filter = " and upper(w.nazw) like '%" + P2_TextBox_FiltrServices.Text.ToUpper() + "%' ";
                        break;
                    case 2: //Usługa
                        filter = " and upper(u.nazw) like '%" + P2_TextBox_FiltrServices.Text.ToUpper() + "%' ";
                        break;
                    default:
                        break;
                }
            }

            try
            {
                FbCommand command = new FbCommand(query, connection);

                query = query + filter + " order by usluga, wzorzec";
                command.CommandText = query;

                //MessageBox.Show(query);
                //Trace.WriteLine(query);

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

        //Pokaz Slowniki
        private void ShowDictionaries(ListView panel)
        {
            Selector lvPersonel;
            CheckBox checkSharedDictonaries;

            string filter = "";

            string query = "select u.kod, u.id as u_id,  u.nazw as usluga," +
                " w.id as w_id,  w.nazw as wzorzec," +
                " s.id as s_id, s.nazw as slownik, s.opis," +
                " p.imie||' '||p.nazw as pracownik," +
                " p.id as p_id, s.idpod, s.idins, s.idzro  " +
                " from uslg u" +
                " join wzfo w on w.iduslg = u.id" +
                " join slow s on s.idwzfo = w.id" +
                " join prac p on p.id = s.idprac" +
                " where w.del = 0 and s.del = 0 and s.usun = 0" +
                " and s.idprac in (";

            //Przygotowanie zapytań sql w zalezności od Panelu (Seta).
            //W lewym panelu możliwe jest pokazanie tylko słowników wybranych dla jednego pracownika lub wspólnych dla wszystkich pracowników (IDPRAC = 0).
            //W prawym panelu możliwe jest pokazanie słowników wybranych dla kilku pracowników i/lub wspólnych dla wszystkich pracowników (IDPRAC = 0).
            switch (panel.Name)
            {
                default:
                    break;
                case "P2_ListView_Dictionaries":
                    if (P2_ListView_Personel.SelectedItems.Count > 0) //dodanie wszystki zaznaczonych pracowników do sql
                    {
                        foreach (DataRowView item in P2_ListView_Personel.SelectedItems)
                        {
                            query = query + item["id"].ToString() + ",";
                        }
                    }
                    if (P2_CheckBox_SharedDictionaries.IsChecked == true) //dodanie wspólnych słowników, jeśli opcja została zaznaczona
                    {
                        query = query + "0,";
                    }

                    //dodanie filtrów do zapytania sql
                    SetFilter(P2_TextBox_FiltrDictionary, P2_ComboBox_FiltrDictionary.SelectedIndex);
                    break;

                case "P1_ListView_Dictionaries":

                    if (P1_CheckBox_SharedDictionaries.IsChecked == true) //dodanie wspólnych słowników, jeśli opcja została zaznaczona
                    {
                        query = query + "0,";
                    }
                    else //lub dodanie wybranego pracownika
                    {
                        if (P1_ComboBox_Personel.SelectedValue != null)
                        {
                            query = query + P1_ComboBox_Personel.SelectedValue + ",";
                            //MessageBox.Show(P1_ComboBox_Personel.SelectedValue.ToString());
                        }
                    }

                    //dodanie filtrów do zapytania sql
                    SetFilter(P1_TextBox_FiltrDictionary, P1_ComboBox_FiltrDictionary.SelectedIndex);

                    lvPersonel = P1_ComboBox_Personel;
                    checkSharedDictonaries = P1_CheckBox_SharedDictionaries;
                    break;
            }

            try
            {
                FbCommand command = new FbCommand(query, connection);

                query = query + "-99) " + filter + " order by pracownik, usluga, wzorzec, slownik";
                command.CommandText = query;

                //MessageBox.Show(query);
                //Trace.WriteLine(query);

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

        private string SetFilter(TextBox textBox, int selectedIndex)
        {
            string filter = "";

            if (textBox.Text.Length > 0)
            {    
                switch (selectedIndex)
                {
                    case 0: //wszystko
                        filter = " and (upper(u.nazw) like '%" + textBox.Text.ToUpper() + "%' ";
                        filter += " or upper(w.nazw) like '%" + textBox.Text.ToUpper() + "%' ";
                        filter += " or upper(s.nazw) like '%" + textBox.Text.ToUpper() + "%') ";
                        break;
                    case 1: //Słownik
                        filter = " and upper(s.nazw) like '%" + textBox.Text.ToUpper() + "%' ";
                        break;
                    case 2: //Wzorzec
                        filter = " and upper(w.nazw) like '%" + textBox.Text.ToUpper() + "%' ";
                        break;
                    case 3: //Usługa
                        filter = " and upper(u.nazw) like '%" + textBox.Text.ToUpper() + "%' ";
                        break;
                    default:
                        break;
                }
            }
            return filter;
        }

        //pokaz elementy slownika 
        private void ShowElements(ListView itemsListView, ObservableCollection<string> itemsList, ListView dictionariesListView)
        {
            string SelectedDictionaryId = "0";

            try
            {
                itemsList.Clear();

                DataTable table = new DataTable();
                DataRowView drv = dictionariesListView.SelectedItem as DataRowView;
                if (drv != null)
                {
                    string id = drv["S_ID"].ToString();

                    SelectedDictionaryId = id;

                    string query = "select * from slow where id = @id";
                    FbCommand command = new FbCommand(query, connection);
                    command.Parameters.AddWithValue("@id", id);

                    FbDataAdapter adapter = new FbDataAdapter(command);

                    adapter.Fill(table);
                    string info = table.Rows[0].Field<string>("opis").ToString();


                    //podziel pozycje slownika z pola OPIS i wrzuć do tablicy
                    string[] podzielone = info.Split(
                            new string[] { AsciiConverter.HEX2ASCII("0D0A") },
                            StringSplitOptions.None
                        );

                    foreach (string item in podzielone)
                    {
                        string newItem = item.Replace("˙", AsciiConverter.HEX2ASCII("0D0A"));
                        if (newItem.Length>0)
                        {
                            itemsList.Add(newItem);
                        }
                    }
                    itemsListView.ItemsSource = itemsList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("showSlow: " + ex.ToString());
            }
            finally
            {
                if (itemsListView == P1_ListView_DictionaryElements)
                {
                    P1_SelectedDictionaryId = SelectedDictionaryId;
                }
                else
                {
                    P2_SelectedDictionaryId = SelectedDictionaryId;
                }
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
            try
            {
                FbCommand command = new FbCommand(sql, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                connection.Close();
            }


        }

        private void UpdatePersonel_TextBox_P2() //wyświe
        {
            int printedElements = 0;

            //wyświetl <WSPÓŁNE SŁOWNIKI> w polu P2_TextBlock_Personel, jeśli zaznaczony checkbox
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

                    //pobierz listę personelu
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


                    //uzupełnij pole P2_TextBlock_Personel o listę wybranego personelu
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
            if (P1_CheckBox_SharedDictionaries.IsChecked == true) //wyświetl <WSPÓŁNE SŁOWNIKI> w polu P2_TextBlock_Personel, jeśli zaznaczony checkbox
            {
                P1_TextBlock_Personel.Text = "<WSPÓŁNE SŁOWNIKI>";
            }
            else // lub wyświetl zaznaczonego pracownika w polu P2_TextBlock_Personel
            {
                P1_TextBlock_Personel.Text = "";
                if (P1_ComboBox_Personel.SelectedValue != null)
                {
                    try
                    {
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
            ShowElements(P2_ListView_DictionaryElements, P2_DictionaryItems, P2_ListView_Dictionaries);
            UpdatePersonel_TextBox_P2();
        }

        private void P2_CheckBox_SharedDictionaries_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(P2_ListView_Dictionaries);
            ShowElements(P2_ListView_DictionaryElements, P2_DictionaryItems, P2_ListView_Dictionaries);
            UpdatePersonel_TextBox_P2();
        }

        private void P2_CheckBox_ClearPersonel_Click(object sender, RoutedEventArgs e)
        {
            P2_ListView_Personel.UnselectAll();
            ShowDictionaries(P2_ListView_Dictionaries);
            ShowElements(P2_ListView_DictionaryElements, P2_DictionaryItems, P2_ListView_Dictionaries);
        }

        private void P2_Button_AllPersonel_Click(object sender, RoutedEventArgs e)
        {
            P2_ListView_Personel.SelectAll();
            ShowDictionaries(P2_ListView_Dictionaries);
            ShowElements(P2_ListView_DictionaryElements, P2_DictionaryItems, P2_ListView_Dictionaries);
        }

        private void P2_ListView_Dictionaries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowElements(P2_ListView_DictionaryElements, P2_DictionaryItems, P2_ListView_Dictionaries);
        }

        private void P1_Button_AddElement_Click(object sender, RoutedEventArgs e)
        {
            if (P1_ListView_Dictionaries.SelectedItems.Count > 0)
            {
                EntryWindow EntryWindow = new EntryWindow(connection, P1_ListView_DictionaryElements, P1_SelectedDictionaryId, EntryWindow.WindowMode.Add);
                EntryWindow.ShowDialog();
                ShowElements(P1_ListView_DictionaryElements, P1_DictionaryItems, P1_ListView_Dictionaries);
            }
        }

        private void P1_Button_EditElement_Click(object sender, RoutedEventArgs e)
        {

            if (P1_ListView_Dictionaries.SelectedItems.Count == 1)
            {
                EntryWindow EntryWindow = new EntryWindow(connection, P1_ListView_DictionaryElements, P1_SelectedDictionaryId, EntryWindow.WindowMode.Edit);
                EntryWindow.ShowDialog();
                ShowElements(P1_ListView_DictionaryElements, P1_DictionaryItems, P1_ListView_Dictionaries);
            }
            else if(P1_ListView_Dictionaries.SelectedItems.Count > 1)
            {
                MessageBox.Show("Zaznacz tylko jedną pozycję słownika do edycji");
            }
        }

        private void P1_Button_RemoveElement_Click(object sender, RoutedEventArgs e)
        {

            if (P1_SelectedDictionaryId != "0" && P1_ListView_Dictionaries.SelectedItems.Count > 0)
            {
                if(MessageBox.Show("Usunąć zaznaczone pozycje słownika?", "Usuwanie pozycji słownika", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string description = "";
                    int i = 0;
                    
                    //stwórz liste elementów z pominięciem usuniętego elementu
                    foreach (var item in P1_ListView_DictionaryElements.Items)
                    {
                        
                        if (P1_ListView_DictionaryElements.SelectedIndex != i)
                        {
                            description += item.ToString() + AsciiConverter.HEX2ASCII("0D0A");
                        }

                        i++;
                    }
                    string updateSql = "update slow set opis = @opis where id = @id";

                    try
                    {
                        FbCommand updateCommand = new FbCommand(updateSql, connection);
                        updateCommand.Parameters.AddWithValue("@opis", description);
                        updateCommand.Parameters.AddWithValue("@id", P1_SelectedDictionaryId);
                        
                        //Trace.WriteLine(updateCommand);

                        connection.Open();
                        updateCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        connection.Close();
                    }

                    ShowElements(P1_ListView_DictionaryElements, P1_DictionaryItems, P1_ListView_Dictionaries);
                }
            }


        }

        private void P1_Personel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("Changed_Personel");
            ShowDictionaries(P1_ListView_Dictionaries);
            ShowElements(P1_ListView_DictionaryElements, P1_DictionaryItems, P1_ListView_Dictionaries);
            UpdatePersonel_TextBox_P1();
        }

        private void P1_CheckBox_SharedDictionaries_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Changed_Checkbox");
            P1_ComboBox_Personel.IsEnabled = !(bool)P1_CheckBox_SharedDictionaries.IsChecked;
            ShowDictionaries(P1_ListView_Dictionaries);
            ShowElements(P1_ListView_DictionaryElements, P1_DictionaryItems, P1_ListView_Dictionaries);
            //clear combobox
            P1_ComboBox_Personel.SelectedIndex = -1;
            UpdatePersonel_TextBox_P1();
        }

        private void P1_ListView_Dictionaries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowElements(P1_ListView_DictionaryElements, P1_DictionaryItems, P1_ListView_Dictionaries);
        }

        private void P2_Button_SelectAllItems_Click(object sender, RoutedEventArgs e)
        {
            P2_ListView_DictionaryElements.SelectAll();
        }

        private void P2_Button_UnselectAllItems_Click(object sender, RoutedEventArgs e)
        {
            P2_ListView_DictionaryElements.UnselectAll();
        }

        private void P1_Button_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            P1_ListView_DictionaryElements.SelectAll();
        }

        private void P1_Button_UnselectAll_Click(object sender, RoutedEventArgs e)
        {
            P1_ListView_DictionaryElements.UnselectAll();
        }

        private void P2_Button_FiltrDictionary_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(P2_ListView_Dictionaries);
        }

        private void P1_Button_FiltrDictionary_Click(object sender, RoutedEventArgs e)
        {
            ShowDictionaries(P1_ListView_Dictionaries);
        }

        private void P2_TextBox_FiltrDictionary_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowDictionaries(P2_ListView_Dictionaries);
            }
        }

        private void P1_TextBox_FiltrDictionary_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowDictionaries(P1_ListView_Dictionaries);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }


        //kopiowanie słowników do innych pracowników (id wzorca formularza ten sam co w źródle)
        private void P1_CopyDictionary_ToPersonel_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (P1_ListView_Dictionaries.SelectedItems.Count == 0)
            {
                MessageBox.Show("Musisz zaznaczyć słowniki do skopiowania. \nZaznacz je w lewym panelu słowników. \nUżyj klawisza Ctrl aby zaznaczyc kilka słowników.");
            }
            else if (P2_ListView_Personel.SelectedItems.Count == 0 && !P2_CheckBox_SharedDictionaries.IsChecked.Value)
            {
                MessageBox.Show("Musisz wybra personel któremu chcessz skopiować słowniki. \nZaznacz personel w prawym panelu Pracownicy. \nUżyj klawisza Ctrl aby zaznaczyc kilka rekordów.");
            }
            else
            {
                bool isChecked = P2_CheckBox_SharedDictionaries.IsChecked.HasValue ? P2_CheckBox_SharedDictionaries.IsChecked.Value : false;
                CopyToPersonelWindow CopyToPersonelWindow = new CopyToPersonelWindow(connectionString, P1_ListView_Dictionaries, P2_ListView_Personel, isChecked);
                CopyToPersonelWindow.ShowDialog();
            }
        }

        //kopiowanie słowników do innych wzorców (id personelu ten sam co w źródle)
        private void P1_CopyDictionary_ToLayout_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (P1_ListView_Dictionaries.SelectedItems.Count == 0)
            {
                MessageBox.Show("Musisz zaznaczyć słowniki do skopiowania. \nZaznacz je w lewym panelu słowników. \nUżyj klawisza Ctrl aby zaznaczyc kilka słowników.");
            }
            else if (P2_ListView_Services.SelectedItems.Count == 0)
            {
                MessageBox.Show("Musisz wybrać wzorce, do których chcessz skopiować słowniki. \nZaznacz wzorce w prawym panelu Wzorce i Usługi. \nUżyj klawisza Ctrl aby zaznaczyc kilka rekordów.");
            }
            else
            {
                CopyToLayoutWindow CopyToLayoutWindow = new CopyToLayoutWindow(connectionString, P1_ListView_Dictionaries, P2_ListView_Services);
                CopyToLayoutWindow.ShowDialog();
            }
        }

        private void P2_Button_FiltrServices_Click(object sender, RoutedEventArgs e)
        {
            ShowServices(P2_ListView_Services);
        }

        private void P2_TextBox_FiltrServices_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowServices(P2_ListView_Services);
            }
        }

        private void P1_Delete_Dictionary_Click(object sender, RoutedEventArgs e)
        {
            string SelectedId;
            string sql = "update slow set del = 1 where id = @s_id";
            int counter = 0;

            if (P1_ListView_Dictionaries.SelectedItems.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("Czy na pewno usunąć zaznaczone słowniki?", "Usunięcie słowników", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        connection.Open();

                        foreach (DataRowView item in P1_ListView_Dictionaries.SelectedItems)
                        {
                            SelectedId = item["s_id"].ToString();

                            FbCommand fbCommand = new FbCommand(sql, connection);
                            fbCommand.Parameters.Add("@s_id", SelectedId);
                            counter += fbCommand.ExecuteNonQuery();
                        }
                        MessageBox.Show($"Usunięto {counter} słowniki/słowników z bazy danych.");

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            ShowDictionaries(P1_ListView_Dictionaries);
        }

        //eksport słowika do pliku XML
        private void P1_Export_Dictionary_Click(object sender, RoutedEventArgs e)
        {
            if (P1_ListView_Dictionaries != null && P1_SelectedDictionaryId!="0") {
                if (MessageBox.Show("Eksportować słownik do pliku XML?", "Eksport do XML", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string filename = ((DataRowView)P1_ListView_Dictionaries.SelectedItem).Row["SLOWNIK"].ToString().Trim();
                    string description = ((DataRowView)P1_ListView_Dictionaries.SelectedItem).Row["OPIS"].ToString();

                    string[] splited = description.Split(
                           new string[] { AsciiConverter.HEX2ASCII("0D0A") },
                           StringSplitOptions.None
                       );

                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "Plik XML | *.xml";
                    saveFileDialog.FileName = filename;
                    if (saveFileDialog.ShowDialog() == true) {
                        filename = saveFileDialog.FileName;
                        XmlWriter xmlWriter = XmlWriter.Create(filename);
                        filename = saveFileDialog.SafeFileName;
                        xmlWriter.WriteStartDocument();
                        xmlWriter.WriteStartElement("Dictionary");
                        xmlWriter.WriteAttributeString("Name", filename);

                        //zapisanie poszczególnych pozycji słownika
                        foreach (string item in splited)
                        {
                            string newItem = item.Replace("˙", AsciiConverter.HEX2ASCII("0D0A"));
                            if (newItem.Length > 0)
                            {
                                xmlWriter.WriteStartElement("Entry");
                                xmlWriter.WriteString(newItem);
                                xmlWriter.WriteEndElement();
                            }
                        }

                        xmlWriter.WriteEndElement();
                        xmlWriter.Close();
                        MessageBox.Show("Słownik został wyeksportowany");
                    };
                }
            }
        }

        //import słownka z pliku XML
        private void P2_Import_Dictionary_Click(object sender, RoutedEventArgs e)
        {
            if (P2_ListView_Personel.SelectedItems.Count == 0 && P2_CheckBox_SharedDictionaries.IsChecked != true)
            {
                MessageBox.Show($"Zaznacz checkbox \"Wspólne Słowniki całęgo personelu\" lub wybierz z zakładki \"Pracownicy\" któremu chcesz zaimportować słownik");
            }
            else if (P2_ListView_Services.SelectedItems.Count == 0)
            {
                MessageBox.Show($"Wybierz z zakładki \"Wzorce i usługi\" do jakiego wzorca formularza chcesz zaimportować słownik");
            }
            else
            {
                string element;
                string dictionaryName;
                string description = "";

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Plik XML | *.xml";
                if (openFileDialog.ShowDialog() == true)
                {
                    string sql = "insert into SLOW (IDUSLG,NAZW, DEL,USUN, IDWZFO, GODAT, GOGDZ, GIDOPER, RPDAT, RPMDAT, MODAT, MOGDZ, MIDOPER, OPIS, IDPRAC)" +
                        " values (@IDUSLG, @NAZW, 0, 0, @IDWZFO, @GODAT, @GOGDZ, @GIDOPER, @RPDAT, @RPMDAT, @MODAT, @MOGDZ, @MIDOPER, @OPIS, @IDPRAC);";
                    FbConnection FbConnection = new FbConnection(connectionString);
                    XmlReader xmlReader = XmlReader.Create(openFileDialog.FileName);
                    try
                    {
                        xmlReader.ReadToFollowing("Dictionary");
                        xmlReader.MoveToAttribute("Name");
                        dictionaryName = xmlReader.Value;

                        while (xmlReader.Read())
                        {
                            if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Entry")
                            {
                                xmlReader.Read();
                                if (xmlReader.NodeType == XmlNodeType.Text)
                                {
                                    element = xmlReader.Value;
                                    description += element + AsciiConverter.HEX2ASCII("0D0A");
                                    Trace.WriteLine(description);
                                }
                            }
                        }


                        DateTime now = DateTime.Now;

                        int howManyCopies = 0;

                        FbConnection.Open();
                        FbCommand FbCommand = new FbCommand(sql, FbConnection);

                        FbCommand.Parameters.Add("@NAZW", dictionaryName);
                        FbCommand.Parameters.Add("@OPIS", description);
                        FbCommand.Parameters.Add("@GODAT", TimeStamp.date(now));
                        FbCommand.Parameters.Add("@GOGDZ", TimeStamp.godz(now));
                        FbCommand.Parameters.Add("@MODAT", TimeStamp.date(now));
                        FbCommand.Parameters.Add("@MOGDZ", TimeStamp.godz(now));
                        FbCommand.Parameters.Add("@RPDAT", TimeStamp.nullDate());
                        FbCommand.Parameters.Add("@RPMDAT", TimeStamp.nullDate());

                        foreach (DataRowView layout in P2_ListView_Services.SelectedItems)
                        {
                            FbCommand.Parameters.Add("@IDUSLG", layout["u_id"].ToString());
                            FbCommand.Parameters.Add("@IDWZFO", layout["w_id"].ToString());

                            foreach (DataRowView personel in P2_ListView_Personel.SelectedItems)
                            {
                                FbCommand.Parameters.Add("@GIDOPER", personel["id"].ToString());
                                FbCommand.Parameters.Add("@MIDOPER", personel["id"].ToString());
                                FbCommand.Parameters.Add("@IDPRAC", personel["id"].ToString());

                                howManyCopies += FbCommand.ExecuteNonQuery();

                                Trace.WriteLine(sql);

                                FbCommand.Parameters.RemoveAt("@GIDOPER");
                                FbCommand.Parameters.RemoveAt("@MIDOPER");
                                FbCommand.Parameters.RemoveAt("@IDPRAC");
                            }
                            FbCommand.Parameters.RemoveAt("@IDUSLG");
                            FbCommand.Parameters.RemoveAt("@IDWZFO");
                        }

                        MessageBox.Show($"Stworzono {howManyCopies} słowników na podstawie pliku XML.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    finally {
                        xmlReader.Close();
                        FbConnection.Close();
                        ShowDictionaries(P1_ListView_Dictionaries);
                        ShowDictionaries(P2_ListView_Dictionaries);
                    }
                };
            };
        }
    }
}

