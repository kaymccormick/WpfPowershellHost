using System.Windows;
using NLog;

namespace Terminal1
{
    public class WpfInputLine : WpfTerminalControl
    {
        static WpfInputLine()
        {
            NumRowsProperty.OverrideMetadata(typeof(WpfInputLine),
                new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure));
            LineModeProperty.OverrideMetadata(typeof(WpfInputLine),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));
            InputOnlyProperty.OverrideMetadata(typeof(WpfInputLine),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));
            NumReservedColumnsProperty.OverrideMetadata(typeof(WpfInputLine), new PropertyMetadata(0));
        }

        /// <inheritdoc />
        public WpfInputLine()
        {
            Logger = LogManager.CreateNullLogger();
        }

        public void ProvideInput(string getText)
        {
            EchoInput(getText);
        }
    }
}