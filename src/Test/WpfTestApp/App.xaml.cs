using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Terminal1;
using WpfTerminalControlLib;

namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            NewWindowHandler();
            Task.Run(() =>
            {
                for (;;)
                {
                    Console.WriteLine("ping");
                    Thread.Sleep(15000);
                }
            });
        }

        private static void NewWindowHandler()
        {
            Thread newWindowThread = new Thread(new ThreadStart(ThreadStartingPoint));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
        }

        private static void ThreadStartingPoint()
        {
            WpfTerminalControl consoleTerm = new WpfTerminalControl() { AutoResize = true, CursorBrush = Brushes.BlueViolet};
            TerminalCharacteristics.AddNumRowsChangedEventHandler(consoleTerm, (sender, e) =>
            {
                Debug.WriteLine("Rows: " + e.NewValue);
            });
            consoleTerm.FontSize = 18.0;
            // consoleTerm.BackgroundColor = ConsoleColor.Black;
            // consoleTerm.ForegroundColor = ConsoleColor.Green;
            // consoleTerm.Background = Brushes.Black;

            Window consoleWindow = new Window();
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition(){Width=new GridLength(1,GridUnitType.Star)});

            grid.Children.Add(consoleTerm);
            StackPanel sp = new StackPanel(){Orientation = Orientation.Horizontal,Margin=new Thickness(5)};
            TextBlock t1 = new TextBlock();
            t1.SetBinding(TextBlock.TextProperty,
                new Binding("CursorColumn") {Source = consoleTerm, StringFormat = "Col: {N2}"});
            TextBlock t3 = new TextBlock();
            t3.SetBinding(TextBlock.TextProperty, new Binding("CursorRow") { Source = consoleTerm, StringFormat = "Row: {N2}"});

            sp.Children.Add(t3);
            sp.Children.Add(new TextBlock {Text = " , "});
            sp.Children.Add(t1);
            Grid.SetRow(sp, 1);
            grid.Children.Add(sp);
            consoleWindow.Content = grid;
            consoleWindow.Show();

            Console.SetOut(new MyConsoleWriter(consoleTerm));

            consoleWindow.Show();
            System.Windows.Threading.Dispatcher.Run();
        }

    }

    internal class MyConsoleWriter : TextWriter
    {
        public WpfTerminalControl ConsoleTerm { get; }

        public MyConsoleWriter(WpfTerminalControl consoleTerm)
        {
            ConsoleTerm = consoleTerm;
        }

        public override Encoding Encoding { get; } = Encoding.UTF8;

        public override void WriteLine(string value)
        {
            ConsoleTerm.Dispatcher.InvokeAsync(() => { ConsoleTerm.WriteLine(DateTime.Now.ToString() + ": " + value); });
        }
    }
}
