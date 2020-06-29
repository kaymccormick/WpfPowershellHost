using System;
using System.IO;
using System.Text;

namespace WpfTerminalControlLib
{
    class Writer1 : TextWriter
    {
        public override Encoding Encoding { get; } = Encoding.UTF8;

    }
    public class MyConsoleWriter : TextWriter
    {
        public WpfTerminalControl ConsoleTerm { get; }

        public MyConsoleWriter(WpfTerminalControl consoleTerm)
        {
            ConsoleTerm = consoleTerm;
            MemoryStream s = new MemoryStream();
        }

        public override Encoding Encoding { get; } = Encoding.UTF8;

        public override void WriteLine(string value)
        {
            ConsoleTerm.Dispatcher.InvokeAsync(() => { ConsoleTerm.WriteLine(DateTime.Now.ToString() + ": " + value); });
        }
    }
}