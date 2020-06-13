using System;
using System.Windows.Threading;

namespace WpfCustomControlLibrary1
{
    class TerminalInterface2 : ITerminalInterface
    {
        /// <inheritdoc />
        public Dispatcher Dispatcher { get; }

        /// <inheritdoc />
        public string WindowTitle { get; set; }

        /// <inheritdoc />
        public int NumColumns { get; set; }

        /// <inheritdoc />
        public int NumRows { get; set; }

        /// <inheritdoc />
        public int ViewX { get; set; }

        /// <inheritdoc />
        public int ViewY { get; set; }

        /// <inheritdoc />
        public ConsoleColor ForegroundColor { get; set; }

        /// <inheritdoc />
        public int CursorSize { get; set; }

        /// <inheritdoc />
        public int CursorColumn { get; set; }

        /// <inheritdoc />
        public int CursorRow { get; set; }

        /// <inheritdoc />
        public ConsoleColor BackgroundColor { get; set; }

        /// <inheritdoc />
        public object Keys { get; set; }

        /// <inheritdoc />
        public void WriteLine()
        {
        }

        /// <inheritdoc />
        public void SetCellCharacter(in int i, in int i1, in char fillCharacter, ConsoleColor? white = null,
            ConsoleColor? black = null, bool b = false, bool b1 = true)
        {
        }

        /// <inheritdoc />
        public char ReadKey(int options)
        {
            return '\0';
        }
    }
}