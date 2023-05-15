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
    /// Interaction logic for CopyDictionaryWindow.xaml
    /// </summary>
    public partial class CopyDictionaryWindow : Window
    {
        public int copyMode = 0; //0-kopiowanie domyślne, 1-kopiowanie do innego wzorca
        Selector SourceDictionaryPanel { get; set; }
        Selector DestinationDictionaryPanel { get; set; }

        public CopyDictionaryWindow(Selector sourceDictionaryPanel, Selector destinationDictionaryPanel, Selector destinationPersonelPanel, string buttonName)
        {
            InitializeComponent();
            SourceDictionaryPanel = sourceDictionaryPanel;
            DestinationDictionaryPanel = destinationDictionaryPanel;
           
            SkopiujDomyslnie_Text1.Text = "Skopiuj wybrane słowniki: ";

            if (SourceDictionaryPanel is ListView)
            {
                string prefix = "";
                foreach (DataRowView dataRow in ((ListView)SourceDictionaryPanel).SelectedItems)
                {
                    SkopiujDomyslnie_Text1.Text += prefix + dataRow["SLOWNIK"].ToString();
                    prefix = ", ";
                    //SkopiujDomyslnie_Button.ToolTip += item.ToString();
                }
            }
            
        }

        private void SkopiujDomyslnie_Button_Click(object sender, RoutedEventArgs e)
        {
            //SkopiujDomyslnie_Button.ToolTip = 
        }
    }
}
