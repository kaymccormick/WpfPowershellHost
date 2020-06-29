using System;
using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Terminal1;

namespace ScriptConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void WpfTerminalControl_OnTextEntryComplete(object sender, TextEntryCompleteArgs e)
        {
            try
            {
                term.WriteLine("");
                var termCursorRow = term.CursorRow;
                // term.CursorRow = termCursorRow + 1;
                var result = await CSharpScript.EvaluateAsync(e.Text).ConfigureAwait(true);
                var s = result.ToString();
                foreach (var line in s.Split("\r\n"))
                {
                    term.WriteLine(line);
                }

            }
            catch (Exception ex)
            {
                var s = ex.ToString();
                foreach (var line in s.Split("\r\n"))
                {
                    term.WriteLine(line);
                }

            }
        }
    }
}
