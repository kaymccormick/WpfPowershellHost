using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
// #if NET472
// using System.Windows.Xps.Packaging;
// #endif

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for TerminalWindow2.xaml
    /// </summary>
    public partial class TerminalWindow2 : Window
    {
        public TerminalWindow2()
        {
            InitializeComponent();
        }
        private void InvokePrint(object sender, RoutedEventArgs e)
        {
            // Create the print dialog object and set options
            PrintDialog pDialog = new PrintDialog();
            pDialog.PageRangeSelection = PageRangeSelection.AllPages;
            pDialog.UserPageRangeEnabled = true;

            // Display the dialog. This returns true if the user presses the Print button.
            Nullable<Boolean> print = pDialog.ShowDialog();
            if (print == true)
            {
#if NETFRAMEWORK
                 // XpsDocument xpsDocument = new XpsDocument("C:\\FixedDocumentSequence.xps", FileAccess.ReadWrite);
                // FixedDocumentSequence fixedDocSeq = xpsDocument.GetFixedDocumentSequence();
                // pDialog.PrintDocument(fixedDocSeq.DocumentPaginator, "Test print job");
#endif
            }
        }
    }
}
