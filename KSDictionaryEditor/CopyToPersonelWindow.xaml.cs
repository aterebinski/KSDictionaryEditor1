 using System;
using System.Collections.Generic;
using System.Data;
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


        public CopyToPersonelWindow(Selector sourceDictionaryPanel, ListView destinationPersonelPanel, bool isCheckedSharedDictionaries)
        {
            InitializeComponent();
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

                if (destinationPersonelPanel.Items.Count > 0) //i lista
                {
                    SkopiujDomyslnie_Text3.Text = "oraz do pracowników:";
                }
                else
                {
                    SkopiujDomyslnie_Text3.Visibility = Visibility.Collapsed;
                    Copy_ListView_Personel.Visibility = Visibility.Collapsed;
                }
                    
            }
            else //bez wspólnych słowników
            {

                if (destinationPersonelPanel.Items.Count > 0) //jesli zaznaczony jest rekord na liscie
                {
                    SkopiujDomyslnie_Text3.Text = "Do personelu:";
                }
                
            }


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
            

            foreach (DataRowView personelDataRow in DestinationPersonelPanel.SelectedItems)
            {
                Copy_ListView_Personel.Items.Add(personelDataRow);
            }
            
        }

        private void SkopiujDomyslnie_Button_Click(object sender, RoutedEventArgs e)
        {
            //SkopiujDomyslnie_Button.ToolTip = 
        }
    }
}
