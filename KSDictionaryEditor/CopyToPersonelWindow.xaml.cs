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
using System.Windows.Shapes;


namespace KSDictionaryEditor
{
    /// <summary>
    /// Interaction logic for CopyToPersonelWindow.xaml
    /// </summary>
    public partial class CopyToPersonelWindow : Window
    {
        public int copyMode = 0; //0-kopiowanie domyślne, 1-kopiowanie do innego wzorca
        Selector SourceDictionaryPanel { get; set; }
        ListView DestinationPersonelPanel { get; set; }
        string ConnectionString;


        public CopyToPersonelWindow(string connectionString, Selector sourceDictionaryPanel, ListView destinationPersonelPanel, bool isCheckedSharedDictionaries)
        {
            InitializeComponent();

            ConnectionString = connectionString;
            SourceDictionaryPanel = sourceDictionaryPanel;
            DestinationPersonelPanel = destinationPersonelPanel;

            //SkopiujDomyslnie_Text1.Text = "Skopiuj wybrane słowniki: ";

            string prefix = "";

            if (SourceDictionaryPanel is ListView)
            {
                foreach (DataRowView dictionaryDataRow in ((ListView)SourceDictionaryPanel).SelectedItems)
                {
                    //SkopiujDomyslnie_Text1.Text += prefix + dataRow["SLOWNIK"].ToString();
                    //prefix = ", ";
                    //SkopiujDomyslnie_Button.ToolTip += item.ToString();
                    Copy_ListView_CopyDictionary.Items.Add(dictionaryDataRow);
                }
            }

            //prefix = "";


            if (isCheckedSharedDictionaries) //wspolne slowniki
            {
                SkopiujDomyslnie_Text2.Visibility = Visibility.Visible;

                if (destinationPersonelPanel.SelectedItems.Count > 0) //i lista
                {
                    SkopiujDomyslnie_Text3.Text = "oraz do pracowników:";
                    MessageBox.Show(destinationPersonelPanel.Items.Count.ToString());
                }
                else
                {
                    SkopiujDomyslnie_Text3.Visibility = Visibility.Collapsed;
                    Copy_ListView_Personel.Visibility = Visibility.Collapsed;
                }
                    
            }
            else //bez wspólnych słowników
            {

                if (destinationPersonelPanel.SelectedItems.Count > 0) //jesli zaznaczony jest rekord na liscie
                {
                    SkopiujDomyslnie_Text3.Text = "do pracowników:";
                }
                
            }

            /*
            if (destinationPersonelPanel.Items.Count > 0) //jesli zaznaczony jest rekord na liscie
            {
                SkopiujDomyslnie_Text2.Visibility = Visibility.Visible;
                if (isCheckedSharedDictionaries)
                {
                    SkopiujDomyslnie_Text3.Text = "oraz do pracowników:";
                }else
                {
                    SkopiujDomyslnie_Text3.Visibility = Visibility.Collapsed;
                    Copy_ListView_Personel.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (isCheckedSharedDictionaries)
                {
                    SkopiujDomyslnie_Text3.Text = "Do personelu:";
                }
            }
            */

            foreach (DataRowView personelDataRow in DestinationPersonelPanel.SelectedItems)
            {
                Copy_ListView_Personel.Items.Add(personelDataRow);
            }
            
        }

        private void SkopiujDomyslnie_Button_Click(object sender, RoutedEventArgs e)
        {
            FbConnection FbConnection = new FbConnection(ConnectionString);
            FbConnection.Open();
            FbCommand FbCommand = new FbCommand();
            FbCommand.Connection = FbConnection;

            string sql;
            DateTime now = DateTime.Now;
            foreach (DataRowView item in Copy_ListView_CopyDictionary.Items)
            {
                sql = "insert into SLOW (IDUSLG,NAZW, DEL,USUN, IDWZFO, GODAT, GOGDZ, GIDOPER, MODAT, MOGDZ, MIDOPER, IDPOD, IDINS, IDZRO, OPIS, IDPRAC)" +
                    " values (@IDUSLG, @NAZW, 0, 0, @IDWZFO, @GODAT, @GOGDZ, @GIDOPER, @MODAT, @MOGDZ, @MIDOPER, @IDPOD, @IDINS, @IDZRO, @OPIS, @IDPRAC);";

                FbCommand.CommandText = sql;

                FbCommand.Parameters.Add("@IDUSLG", item["u_id"].ToString());
                FbCommand.Parameters.AddWithValue("@NAZW", item["slownik"].ToString());
                FbCommand.Parameters.AddWithValue("@IDWZFO", item["u_id"].ToString());
                FbCommand.Parameters.AddWithValue("@GODAT", TimeStamp.date(now));
                FbCommand.Parameters.AddWithValue("@GOGDZ", TimeStamp.godz(now));

                FbCommand.Parameters.AddWithValue("@MODAT", TimeStamp.date(now));
                FbCommand.Parameters.AddWithValue("@MOGDZ", TimeStamp.godz(now));

                FbCommand.Parameters.AddWithValue("@IDPOD", item["idpod"].ToString());
                FbCommand.Parameters.AddWithValue("@IDINS", item["idins"].ToString());
                FbCommand.Parameters.AddWithValue("@IDZRO", item["idzro"].ToString());
                FbCommand.Parameters.AddWithValue("@OPIS", item["opis"].ToString());


                MessageBox.Show(item["u_id"].ToString());

                foreach (DataRowView prac in Copy_ListView_Personel.Items)
                {
                    FbCommand.Parameters.AddWithValue("@GIDOPER", prac["id"].ToString());
                    FbCommand.Parameters.AddWithValue("@MIDOPER", prac["id"].ToString());
                    FbCommand.Parameters.AddWithValue("@IDPRAC", prac["id"].ToString());

                    Trace.WriteLine(sql);

                    try
                    {
                        FbCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.ToString());
                        FbConnection.Close();
                    }
                    
                }


                
            }


            FbConnection.Close();
        }
    }
}
