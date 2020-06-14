using System.Windows;
using WpfTerminalControlLib;

namespace Terminal1
{
    public static class TerminalCharacteristics
    {

        public static readonly DependencyProperty NumRowsProperty = DependencyProperty.RegisterAttached(
            "NumRows", typeof(int), typeof(TerminalCharacteristics),
            new FrameworkPropertyMetadata(-1,
                FrameworkPropertyMetadataOptions.AffectsMeasure ,
                new PropertyChangedCallback(OnNumRowsChanged_)));

        private static void OnNumRowsChanged_(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((UIElement) d).RaiseEvent(
                new RoutedPropertyChangedEventArgs<int>((int) e.OldValue, (int) e.NewValue, NumRowsChangedEvent));
        }

        public static readonly DependencyProperty NumColumnsProperty = DependencyProperty.RegisterAttached(
            "NumColumns", typeof(int), typeof(TerminalCharacteristics),
            new FrameworkPropertyMetadata(-1,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnNumColumnsChanged));

        private static void OnNumColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((UIElement) d).RaiseEvent(new RoutedPropertyChangedEventArgs<int>((int) e.OldValue, (int) e.NewValue,
                NumColumnsChangedEvent));
        }

        public static readonly RoutedEvent NumRowsChangedEvent = EventManager.RegisterRoutedEvent(
            "NumRowsChanged"
            , RoutingStrategy.Bubble
            , typeof(
                RoutedPropertyChangedEventHandler
                <int>)
            , typeof(WpfTerminalControl)
        );

        public static readonly RoutedEvent NumColumnsChangedEvent = EventManager.RegisterRoutedEvent(
            "NumColumnsChanged"
            , RoutingStrategy.Bubble
            , typeof(
                RoutedPropertyChangedEventHandler
                <int>)
            , typeof(WpfTerminalControl)
        );

        public static void AddNumColumnsChangedEventHandler(
            DependencyObject d
            , RoutedPropertyChangedEventHandler<int> handler
        )
        {
            switch (d)
            {
                case UIElement uie:
                    uie.AddHandler(NumColumnsChangedEvent, handler);
                    break;
                case ContentElement ce:
                    ce.AddHandler(NumColumnsChangedEvent, handler);
                    break;
            }
        }

        public static void AddNumRowsChangedEventHandler(
            DependencyObject d
            , RoutedPropertyChangedEventHandler<int> handler
        )
        {
            switch (d)
            {
                case UIElement uie:
                    uie.AddHandler(NumRowsChangedEvent, handler);
                    break;
                case ContentElement ce:
                    ce.AddHandler(NumRowsChangedEvent, handler);
                    break;
            }
        }

        public static int GetNumRows(DependencyObject d)
        {
            return (int) d.GetValue(NumRowsProperty);
        }

        public static void SetNumRows(DependencyObject d, int numRows)
        {
            d.SetValue(NumRowsProperty, numRows);
        }

        public static int GetNumColumns(DependencyObject d)
        {
            return (int) d.GetValue(NumColumnsProperty);
        }

        public static void SetNumColumns(DependencyObject d, int numCols)
        {
            d.SetValue(NumColumnsProperty, numCols);
        }
    }
}