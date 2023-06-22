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
                    Copy_ListView_CopyDictionary.Items.Add(dictionaryDataRow);
                }
            }

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

            foreach (DataRowView personelDataRow in DestinationPersonelPanel.SelectedItems)
            {
                Copy_ListView_Personel.Items.Add(personelDataRow);
            }

        }

        private void Copy_Button_Click(object sender, RoutedEventArgs e)
        {
            bool isDictionaryCopied = false;

            FbConnection FbConnection = new FbConnection(ConnectionString);
            FbConnection.Open();



            string sql = "insert into SLOW (IDUSLG,NAZW, DEL,USUN, IDWZFO, GODAT, GOGDZ, GIDOPER, RPDAT, RPMDAT, MODAT, MOGDZ, MIDOPER, IDPOD, IDINS, IDZRO, OPIS, IDPRAC)" +
                    " values (@IDUSLG, @NAZW, 0, 0, @IDWZFO, @GODAT, @GOGDZ, @GIDOPER, @RPDAT, @RPMDAT, @MODAT, @MOGDZ, @MIDOPER, @IDPOD, @IDINS, @IDZRO, @OPIS, @IDPRAC);";
            DateTime now = DateTime.Now;
            foreach (DataRowView item in Copy_ListView_CopyDictionary.Items)
            {

                foreach (DataRowView prac in Copy_ListView_Personel.Items)
                {
                    FbCommand FbCommand = new FbCommand();
                    FbCommand.Connection = FbConnection;
                    FbCommand.CommandText = sql;

                    FbCommand.Parameters.Add("@IDUSLG", item["u_id"].ToString());
                    FbCommand.Parameters.AddWithValue("@NAZW", item["slownik"].ToString());
                    FbCommand.Parameters.AddWithValue("@IDWZFO", item["w_id"].ToString());
                    FbCommand.Parameters.AddWithValue("@GODAT", TimeStamp.date(now));
                    FbCommand.Parameters.AddWithValue("@GOGDZ", TimeStamp.godz(now));

                    FbCommand.Parameters.AddWithValue("@MODAT", TimeStamp.date(now));
                    FbCommand.Parameters.AddWithValue("@MOGDZ", TimeStamp.godz(now));

                    FbCommand.Parameters.AddWithValue("@RPDAT", TimeStamp.nullDate());
                    FbCommand.Parameters.AddWithValue("@RPMDAT", TimeStamp.nullDate());

                    FbCommand.Parameters.AddWithValue("@IDPOD", item["idpod"].ToString());
                    FbCommand.Parameters.AddWithValue("@IDINS", item["idins"].ToString());
                    FbCommand.Parameters.AddWithValue("@IDZRO", item["idzro"].ToString());
                    FbCommand.Parameters.AddWithValue("@OPIS", item["opis"].ToString());


                    //MessageBox.Show(item["u_id"].ToString());
                    Trace.WriteLine(sql);


                    FbCommand.Parameters.AddWithValue("@GIDOPER", prac["id"].ToString());
                    FbCommand.Parameters.AddWithValue("@MIDOPER", prac["id"].ToString());
                    FbCommand.Parameters.AddWithValue("@IDPRAC", prac["id"].ToString());

                    Trace.WriteLine(sql);

                    try
                    {
                        int iloscRekordow = FbCommand.ExecuteNonQuery();
                        if (iloscRekordow > 0)
                        {
                            isDictionaryCopied = true;  
                        }
                        Trace.WriteLine("Ilosc rekordow : " + iloscRekordow.ToString());
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.ToString());
                        FbConnection.Close();
                    }
                }
            }

            if (isDictionaryCopied)
            {
                MessageBox.Show("Słowniki zostały skopioiwane.");
            }
            FbConnection.Close();
            this.Close();
        }
    }
}
