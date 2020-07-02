using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Terminal1;

namespace ScriptConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ScriptState<object> _state;

        public MainWindow()
        {
            InitializeComponent();
        }

        public ICommand ResizeCommand { get; set; } = MainWindow.DefaultResizeCommand;
        public static RoutedUICommand DefaultResizeCommand = new RoutedUICommand("Resize", "resize", typeof(MainWindow));

        private async void WpfTerminalControl_OnTextEntryComplete(object sender, TextEntryCompleteArgs e)
        {
            try
            {
                term.WriteLine("");
                var termCursorRow = term.CursorRow;
                // term.CursorRow = termCursorRow + 1;
                if (_state == null)
                {
                    var scriptOptions = ScriptOptions.Default.WithImports(new[]{"System.Windows", "System.Windows.Controls"}).WithReferences(MetadataReference.CreateFromFile(typeof(Window).Assembly.Location));
                    _state = await CSharpScript.RunAsync(e.Text, scriptOptions).ConfigureAwait(true);
                }
                else
                {
                    _state = await _state.ContinueWithAsync(e.Text).ConfigureAwait(true);
                }

                if (_state.ReturnValue != null)
                {
                    var s = _state.ReturnValue.ToString();
                    foreach (var line in s.Split("\r\n"))
                    {
                    term.WriteLine(line);
                    }
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

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Width = Width * 2;
            Height = Height * 2;
        }
    }
}
