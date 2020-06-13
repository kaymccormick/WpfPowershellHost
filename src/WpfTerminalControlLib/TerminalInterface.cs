using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Terminal1;
using WpfCustomControlLibrary1.Annotations;
using WpfTerminalControlLib;

namespace WpfCustomControlLibrary1
{
    public class TerminalInterface : FrameworkElement, ITerminalInterface
    {
        private WpfTerminalControl wpfTerminalControl;
        private int _numColumns;
        private int _numRows;
        private int _viewX;
        private int _viewY;
        private int _cursorSize;
        private int _cursorColumn;
        private int _cursorRow;
        private ConsoleColor _backgroundColor;
        private int columnOffset = 5;
        private int rowOffset = 0;
        private int _numColumns1;
        private int _numRows1;

        public static readonly DependencyProperty NumRowsProperty =
            TerminalCharacteristics.NumRowsProperty.AddOwner(typeof(TerminalInterface));


        public static readonly DependencyProperty NumColumnsProperty =
            TerminalCharacteristics.NumColumnsProperty.AddOwner(typeof(TerminalInterface));


        public TerminalInterface(WpfTerminalControl wpfTerminalControl)
        {
            this.wpfTerminalControl = wpfTerminalControl;
            if(wpfTerminalControl.NumColumns != -1)
            {
                NumColumns = wpfTerminalControl.NumColumns - columnOffset;
            }
            if (wpfTerminalControl.NumRows != -1)
            {
                NumRows = wpfTerminalControl.NumRows - rowOffset;
            }

            TerminalCharacteristics.AddNumColumnsChangedEventHandler(wpfTerminalControl, Target2);
            TerminalCharacteristics.AddNumRowsChangedEventHandler(wpfTerminalControl, Target);
        }

        private void Target2(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            SetValue(NumColumnsProperty, e.NewValue - columnOffset);
        }

        private void Target(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            SetValue(NumRowsProperty, e.NewValue - rowOffset);
            //RaiseEvent(new RoutedPropertyChangedEventArgs<int>(e.OldValue - rowOffset, e.NewValue - rowOffset, NumRowsChangedEvent));
        }


        /// <inheritdoc />
        public object Keys { get; set; }

        /// <inheritdoc />
        public char ReadKey(int options)
        {
            return '\0';
        }


        /// <inheritdoc />
        public string WindowTitle
        {
            get { return wpfTerminalControl.WindowTitle; }
            set { wpfTerminalControl.WindowTitle = value; }
        }

        /// <inheritdoc />
        public int NumColumns_
        {
            get { return wpfTerminalControl.NumColumns - wpfTerminalControl.NumReservedColumns; }
            set { wpfTerminalControl.NumColumns = wpfTerminalControl.NumReservedColumns + value; }
        }

        public int NumColumns
        {
            get { return (int) GetValue(NumColumnsProperty); }
            set { SetValue(NumColumnsProperty, value); }
        }


        public int NumRows
        {
            get { return (int) GetValue(NumRowsProperty); }
            set { SetValue(NumRowsProperty, value); }
        }

        /// <inheritdoc />
        public int NumRows_
        {
            get { return wpfTerminalControl.NumRows; }
            set { wpfTerminalControl.NumRows = value; }
        }

        /// <inheritdoc />
        public int ViewX
        {
            get { return _viewX; }
            set { _viewX = value; }
        }

        /// <inheritdoc />
        public int ViewY
        {
            get { return _viewY; }
            set { _viewY = value; }
        }

        /// <inheritdoc />
        public ConsoleColor ForegroundColor { get; set; }

        /// <inheritdoc />
        public int CursorSize { get; set; }

        /// <inheritdoc />
        public int CursorRow
        {
            get { return wpfTerminalControl.CursorRow - rowOffset; }
            set { wpfTerminalControl.CursorRow = value + rowOffset; }
        }

        /// <inheritdoc />
        public int CursorColumn
        {
            get { return wpfTerminalControl.CursorColumn - columnOffset; }
            set
            {
                _cursorColumn = value;
                wpfTerminalControl.CursorColumn = value + columnOffset;
            }
        }


        /// <inheritdoc />
        public ConsoleColor BackgroundColor

        {
            get { return wpfTerminalControl.BackgroundColor; }
            set
            {
                _backgroundColor = value;
                wpfTerminalControl.BackgroundColor = value;
            }
        }

        /// <inheritdoc />
        public void WriteLine()
        {
            wpfTerminalControl.WriteLine();
        }

        /// <inheritdoc />
        public void SetCellCharacter(in int i, in int i1, in char fillCharacter, ConsoleColor? white = null,
            ConsoleColor? black = null, bool b = false,
            bool b1 = true)
        {
            if (NumColumns == -1 || NumRows == -1)
                return;
            wpfTerminalControl.SetCellCharacter(TranslateRow(i), TranslateColumn(i1), fillCharacter, white, black, b,
                b1);
        }

        private int TranslateColumn(in int i1)
        {
            return i1 + columnOffset;
        }

        private int TranslateRow(in int i)
        {
            return i + rowOffset;
        }

        /// <inheritdoc />
        Dispatcher ITerminalInterface.Dispatcher
        {
            get { return wpfTerminalControl.Dispatcher; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void WriteLine(string value)
        {
            wpfTerminalControl.WriteLine(value);
        }

        public void WriteErrorLine(string value)
        {
            WriteLine(value);

        }

        public void WriteDebugLine(string message)
        {
            WriteLine(message);

        }

        public void WriteInformation(MyInformationRecord myInformationRecord)
        {

        }

        public void WriteVerboseLine(string message)
        {
            WriteLine(message);
        }

        public void WriteProgress(long sourceId, MyRecord myRecord)
        {
            WriteLine(myRecord.CurrentOperation);
        }

        public void WriteWarningLine(string message)
        {
            WriteLine(message);
        }

        public void Write(string value)
        {
            wpfTerminalControl.Write(value);
        }
    }
}