using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WpfCustomControlLibrary1;
using WpfTerminalControlLib;
using Xunit;
using Xunit.Abstractions;

namespace TerminalTest
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private WpfTerminalControl _terminal;
        private int _rowIndex;
        private int _colIndex;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _rowIndex = 0;
            _colIndex = 0;
        }

        [WpfFact]
        public void Test1()
        {
            var xx = new WpfTerminalControl();
            var w = CreateWindow();
            w.Content = xx;
            xx.SetCellCharacter(0, 0,
                'H', ConsoleColor.Black,
                ConsoleColor.White);
            var chi = xx.GetCellCharacter(0, 0);
            Assert.Equal('H', chi.Character);
            w.Show();
        }

        private static Window CreateWindow()
        {
            return new Window();
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame) f).Continue = false;

            return null;
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        //[WpfFact]
        public void Test2()
        {
            _terminal = CreateWpfTerminalControl(out var w);
            _terminal.SetCellCharacter(0, 0,
                'H', ConsoleColor.Black,
                ConsoleColor.White);

            _rowIndex = 0;
            _colIndex = 0;
            var fExit = false;
            w.Show();
            w.Activate();
            //var task1 = Task.Run(Writechar);
            for (;;)
            {
                DoEvents();
                var ch = '-';
                var cur = DateTime.Now;
                _terminal.SetCellCharacter(_rowIndex, _colIndex, ch, ConsoleColor.White, ConsoleColor.Black);
                Debug.WriteLine($"{_rowIndex},{_colIndex}");
                _colIndex++;
                if (_colIndex == _terminal.NumColumns)
                {
                    _colIndex = 0;
                    _rowIndex++;
                    if (_rowIndex == _terminal.NumRows) return;
                }


                // if (task1.Status == TaskStatus.Faulted)
                // {
                // throw task1.Exception.Flatten();
                // }
                // if (task1.Wait(300))
                // {
                // break;
                // }
                if (fExit) return;
            }
        }

        private async Task Writechar()
        {
            var ch = '-';
            var cur = DateTime.Now;
            _terminal.Dispatcher.Invoke(() =>
            {
                _terminal.SetCellCharacter(_rowIndex, _colIndex, ch, ConsoleColor.White, ConsoleColor.Black);
                Debug.WriteLine($"{_rowIndex},{_colIndex}");
                _colIndex++;
                if (_colIndex == _terminal.NumColumns)
                {
                    _colIndex = 0;
                    _rowIndex++;
                    if (_rowIndex == _terminal.NumRows) return;
                }

                _terminal.SetCellCharacter(_rowIndex, _colIndex, ch, ConsoleColor.White, ConsoleColor.Black);
                Debug.WriteLine($"{_rowIndex},{_colIndex}");
                _colIndex++;
                if (_colIndex == _terminal.NumColumns)
                {
                    _colIndex = 0;
                    _rowIndex++;
                    if (_rowIndex == _terminal.NumRows) return;
                }
            }, DispatcherPriority.Render);

            var span = DateTime.Now - cur;
            Debug.WriteLine(span);
            await Writechar();
        }


        private WpfTerminalControl CreateWpfTerminalControl(out Window w)
        {
            var xx = new WpfTerminalControl(); 
            xx.HorizontalAlignment = HorizontalAlignment.Left;
            xx.VerticalAlignment = VerticalAlignment.Top;

            xx.DebugCb = _testOutputHelper.WriteLine;
            xx.BorderBrush = Brushes.Red;
            xx.BorderThickness = new Thickness(10);
            var ow = xx.CellHeight;
            var oh = xx.CellWidth;
            xx.FontSize = 20;
            Debug.WriteLine(xx.CellWidth);
            Debug.WriteLine(xx.CellHeight);
            w = CreateWindow();
            var grid = new Grid();

            w.Content = xx;

            return xx;
        }

        [WpfFact]
        public void Test4()
        {
            var term = CreateWpfTerminalControl(out var w);
            w.ShowActivated = true;

            _terminal = term;
            var wClosed = false;
            w.Closed += (sender, args) => { wClosed = true; };
            w.KeyDown += (sender, args) =>
            {
                var xx = new TerminalWriter(term);
                var file = new StreamReader(@"C:\data\logs\client2\8612.json");
                try
                {
                    while (file.EndOfStream == false)
                    {
                        var s = file.ReadLine();
                        xx.WriteLine(s);
                        DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(xx._written);
                    throw;
                }
            };
            w.Show();
        }

        [WpfFact]
        public void Test5()
        {
            var term = CreateWpfTerminalControl(out var w);
            w.ShowActivated = true;

            _terminal = term;
            var wClosed = false;
            w.Closed += (sender, args) => { wClosed = true; };
            w.Loaded += (sender, args) =>
            {
                var xx = new TerminalWriter(term);
                var file = new StreamReader(@"C:\data\logs\client2\8612.json");
                try
                {
                    while (file.EndOfStream == false)
                    {
                        var s = file.ReadLine();
                        xx.WriteLine(s);
                        DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(xx._written);
                    throw;
                }
            };
            w.Show();
        }

        [WpfFact]
        public void Test6()
        {
            var term = CreateWpfTerminalControl(out var w);
            term.FontFamily = new FontFamily("Lucida console");
            term.NumRows = 10;
            term.NumColumns = 20;
                
            ITerminalInterface xxi = new TerminalInterface(term);
            w.ShowActivated = true;
            // w.ShowDialog();
            _terminal = term;
            var wClosed = false;
            w.Closed += (sender, args) => { wClosed = true; };
            for (var i = 0; i < 25; i++)
            {
                var s = i.ToString("D3");
                for (int j = 0; j < s.Length; j++)
                {
                    xxi.SetCellCharacter(i, j, s[j]);
                }

            }

            w.Show();
        }

        [WpfFact]
        public void Test8()
        {
            var term = CreateWpfTerminalControl(out var w);
            term.FontFamily = new FontFamily("Lucida console");
            term.NumRows = 10;
            term.NumColumns = 30;

            ITerminalInterface xxi = new TerminalInterface(term);
            w.ShowActivated = true;
            // w.ShowDialog();
            _terminal = term;
            var wClosed = false;
            w.Closed += (sender, args) => { wClosed = true; };
            var xxiNumRows = xxi.NumRows;
            var xxiNumColumns = xxi.NumColumns;
            _testOutputHelper.WriteLine($"Numrows is {xxiNumRows}");
            _testOutputHelper.WriteLine($"cols is {xxiNumColumns}");
            for (var i = 0; i < xxiNumRows; i++)
            {
                var s = i.ToString("D3");
             
                for (int j = 0; j < xxiNumColumns; j++)
                {
                    try
                    {
                        xxi.SetCellCharacter(i, j, 'a');
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }

            }

            w.Show();
        }
        [WpfFact]
        public void Test7()
        {
            var term = CreateWpfTerminalControl(out var w);
            term.NumRows = 20;
            term.NumColumns = 20;

            ITerminalInterface xxi = new TerminalInterface(term);
            w.ShowActivated = true;
            // w.ShowDialog();
            _terminal = term;
            var wClosed = false;
            w.Closed += (sender, args) => { wClosed = true; };
            // for (var i = 0; i < 25; i++)
            // {
                // var s = i.ToString("D3");
                // for (int j = 0; j < s.Length; j++)
                // {
                    // xxi.SetCellCharacter(i, j, s[j]);
                // }

            // }

            w.Show();
        }
        [WpfFact]
        public void Test9()
        {
            var term = CreateWpfTerminalControl(out var w);
            term.FontFamily = new FontFamily("Lucida console");
            term.NumRows = 10;
            term.NumColumns = 30;

            ITerminalInterface xxi = new TerminalInterface(term);
            w.ShowActivated = true;
             w.Show();
            _terminal = term;
            var wClosed = false;
            w.Closed += (sender, args) => { wClosed = true; };
            var xxiNumRows = xxi.NumRows;
            var xxiNumColumns = xxi.NumColumns;
            _testOutputHelper.WriteLine($"Numrows is {xxiNumRows}");
            _testOutputHelper.WriteLine($"cols is {xxiNumColumns}");
            for (var i = 0; i < xxiNumRows; i++)
            {
                var s = i.ToString("D3");

                for (int j = 0; j < xxiNumColumns; j++)
                {
                    try
                    {
                        xxi.SetCellCharacter(i, j, 'a');
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    DoEvents();
                }

            }

            Thread.Sleep(10000);
        }
    }

    public class TerminalWriter : TextWriter
    {
        private readonly WpfTerminalControl _term;
        public int _written;

        public TerminalWriter(WpfTerminalControl term)
        {
            _term = term;
        }

        /// <inheritdoc />
        public override void Write(char value)
        {
            _written++;
            _term.Dispatcher.Invoke(() => { _term.Write(value); });
        }

        /// <inheritdoc />
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}