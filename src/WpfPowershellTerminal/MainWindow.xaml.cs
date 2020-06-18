using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using NLog;
using Terminal1;
using WpfTerminalControlLib;

namespace WpfPowerShellTerminal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SshFunctionality ssh = new SshFunctionality();
        private bool inDebugMode;
     
        private Popup _popup;
        Logger Logger = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            InitializeComponent();
            Terminal.Focus();
            TraceSource xx = new TraceSource("TerminalTraceSource");
            Terminal.ProgressEvent += (sender, args) =>
            {
                if(ProgressBar.Visibility != Visibility.Visible)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    
                }
                ProgressBar.Value = args.Record.PercentComplete;
                if(args.Record.RecordType == 1)
                {
                
                }
                MainStatus.Text = args.Record.StatusDescription;
            };
            Terminal.DebugCb = DebugCb;
            Terminal.ExecuteCommandComplete += TerminalOnExecuteCommandComplete;
            if (Shell.InitialSessionState != null)
            {
                Shell.InitialSessionState.Commands.Add(new SessionStateCmdletEntry("my-command", typeof(MyCommandCmdlet),
                    ""));
            }

            Shell.InitialSessionStateChanged += (sender, args) =>
            {
                if (args.NewValue != null)
                    args.NewValue.Commands.Add(new SessionStateCmdletEntry("my-command", typeof(MyCommandCmdlet),
                        ""));
            };
            Shell.CoerceValue(WrappedPowerShell.InitialSessionStateProperty);
            Shell.CoerceValue(WrappedPowerShell.RunspaceProperty);

            Input.TextEntryComplete += OnInputOnTextEntryComplete;
            // Te???rminal.TextEntryComplete += (object sender, TextEntryCompleteArgs args) =>
            // {
                // Terminal.WriteLine();
                // Debug.WriteLine(args.Text);
                // Shell.ExecuteAsync(args.Text);
                // Debug.WriteLine("Back from execute");
            // };


        }

        private void TerminalOnExecuteCommandComplete(object sender, EventArgs e)
        {
            FSLocation.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }

        public static readonly DependencyProperty IsExecutingProperty = DependencyProperty.Register(
            "IsExecuting", typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool), OnIsExecutingChanged));

        public bool IsExecuting
        {
            get { return (bool) GetValue(IsExecutingProperty); }
            set { SetValue(IsExecutingProperty, value); }
        }

        private static void OnIsExecutingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainWindow) d).OnIsExecutingChanged((bool) e.OldValue, (bool) e.NewValue);
        }



        protected virtual void OnIsExecutingChanged(bool oldValue, bool newValue)
        {
        }

        private async void OnInputOnTextEntryComplete(object sender, TextEntryCompleteArgs args)
        {
            Debug.WriteLine(args.Text);
            IsExecuting = true;
            Debug.WriteLine("Calling executeasync");

            await Shell.ExecuteAsync(args.Text).ConfigureAwait(true);
            Debug.WriteLine("Back from execute2");
            TerminalOnExecuteCommandComplete(null, null);
            IsExecuting = false;

        }

        private void DebugCb(string obj)
        {
            Debug.WriteLine(obj);
            Trace.WriteLine(obj);
            Logger.Debug(obj);
        }

        /// <inheritdoc />
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.LeftAlt)
            {

            }
            if (e.Key == Key.LeftShift )
            {
                if (_popup == null)
                {
                    _popup = new Popup();
                    Border b = new Border()
                    {
                        Padding = new Thickness(5), Background = Brushes.Azure, BorderBrush = Brushes.Red,
                        BorderThickness = new Thickness(3)
                    };
                    ScrollViewer vv = new ScrollViewer();

                    StackPanel pp = new StackPanel();
                    vv.Content = pp;
                    b.Child = vv;
                    pp.SetValue(TextElement.FontFamilyProperty, new FontFamily("Courier New"));
                    foreach (var textBlock in Terminal._buffer2.Select(l => new TextBlock {Text = string.Join("", l.Select(cc=>cc=='\0'?' ':cc))}))
                    {
                        pp.Children.Add(textBlock);
                    }

                    _popup.Child = b;

                    _popup.PlacementTarget = Input;
                    _popup.Placement = PlacementMode.Top;
                    _popup.IsOpen = true;
                }
                else
                {
                    _popup.IsOpen = false;
                    _popup = null;
                }
            }
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.S)
                {
                    Terminal.DumpBuffercontents();
                } else if (e.Key == Key.A)
                {
                    inDebugMode = true;
                    Terminal.EnterDebugMode();
                    Terminal.Focusable = true;
                    Terminal.Focus();
                }

            } else if (inDebugMode && e.Key == Key.Escape)
            {
                Input.Focus();
                Terminal.ExitDebugMode();
                Terminal.Focusable = false;
            }
        }

        private void PasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                Input.ProvideInput(Clipboard.GetText());
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // var transparencyConverter = new TransparencyConverter(this);
            // transparencyConverter.MakeTransparent();
        }
    }

    public class MyCommandCmdlet : PSCmdlet
    {
        /// <inheritdoc />
        protected override void EndProcessing()
        {
            base.EndProcessing();
            WriteWarning("test");
        }
    }


    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32_DllName)]
        private static extern int GetConsoleOutputCP();

        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        public static void Show()
        {
            //#if DEBUG
            if (!HasConsole)
            {
                AllocConsole();
                InvalidateOutAndError();
            }
            //#endif
        }

        /// <summary>
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.
        /// </summary>
        public static void Hide()
        {
            //#if DEBUG
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
            //#endif
        }

        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        static void InvalidateOutAndError()
        {
            Type type = typeof(System.Console);

            System.Reflection.FieldInfo _out = type.GetField("_out",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            System.Reflection.FieldInfo _error = type.GetField("_error",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            System.Reflection.MethodInfo _InitializeStdOutError = type.GetMethod("InitializeStdOutError",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            Debug.Assert(_out != null);
            Debug.Assert(_error != null);

            Debug.Assert(_InitializeStdOutError != null);

            _out.SetValue(null, null);
            _error.SetValue(null, null);

            _InitializeStdOutError.Invoke(null, new object[] { true });
        }

        static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
    }

    class TransparencyConverter
    {
        private readonly Window _window;

        public TransparencyConverter(Window window)
        {
            _window = window;
        }

        public void MakeTransparent()
        {
            var mainWindowPtr = new WindowInteropHelper(_window).Handle;
            var mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            if (mainWindowSrc != null)
                if (mainWindowSrc.CompositionTarget != null)
                    mainWindowSrc.CompositionTarget.BackgroundColor = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);

            var margins = new Margins
            {
                cxLeftWidth = 0,
                cxRightWidth = Convert.ToInt32(_window.Width) * Convert.ToInt32(_window.Width),
                cyTopHeight = 0,
                cyBottomHeight = Convert.ToInt32(_window.Height) * Convert.ToInt32(_window.Height)
            };

            if (mainWindowSrc != null) DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Margins
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);
    }
}
