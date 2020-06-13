using System;
using System.Windows.Threading;

namespace WpfCustomControlLibrary1
{
    public interface ITerminalInterface 
    {
        Dispatcher Dispatcher { get; }
        string WindowTitle { get; set; }
        int NumColumns { get; set; }
        int NumRows { get; set; }
        int ViewX { get; set; }
        int ViewY { get; set; }
        ConsoleColor ForegroundColor { get; set; }
        int CursorSize { get; set; }
        int CursorColumn { get; set; }
        int CursorRow { get; set; }
        ConsoleColor BackgroundColor { get; set; }
        object Keys { get; set; }
        void WriteLine();
        void SetCellCharacter(in int i, in int i1, in char fillCharacter, ConsoleColor? white = null, ConsoleColor? black=null, bool b = false, bool b1=true);
        char ReadKey(int options);
    }
}