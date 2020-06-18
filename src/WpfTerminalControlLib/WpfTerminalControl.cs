using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NLog;
using Terminal1;
using WpfCustomControlLibrary1.Annotations;

namespace WpfTerminalControlLib
{
    public class WpfTerminalControl : Control, INotifyPropertyChanged
    {
        public static readonly DependencyProperty CharUnderCursorProperty = DependencyProperty.Register(
            "CharUnderCursor", typeof(char), typeof(WpfTerminalControl),
            new PropertyMetadata(default(char)/*, OnCharUnderCursorChanged*/));

           public static readonly DependencyProperty AutoResizeProperty = DependencyProperty.Register(
            "AutoResize", typeof(bool), typeof(WpfTerminalControl),
            new PropertyMetadata(true, OnAutoResizeChanged));

        public static readonly DependencyProperty CursorBrushProperty = DependencyProperty.Register(
            "CursorBrush", typeof(Brush), typeof(WpfTerminalControl), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty InputOnlyProperty = DependencyProperty.Register(
            "InputOnly", typeof(bool), typeof(WpfTerminalControl), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty WindowTitleProperty = DependencyProperty.Register(
            "WindowTitle", typeof(string), typeof(WpfTerminalControl),
            new PropertyMetadata(default(string), OnWindowTitleChanged));

        public static readonly DependencyProperty ViewXProperty = DependencyProperty.Register(
            "ViewX", typeof(int), typeof(WpfTerminalControl), new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.AffectsRender,  OnViewXChanged));

        public static readonly DependencyProperty ViewYProperty = DependencyProperty.Register(
            "ViewY", typeof(int), typeof(WpfTerminalControl), new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.AffectsRender|FrameworkPropertyMetadataOptions.AffectsArrange|FrameworkPropertyMetadataOptions.AffectsMeasure,  OnViewYChanged));

        public static readonly DependencyProperty CursorRowProperty = DependencyProperty.Register(
            "CursorRow", typeof(int), typeof(WpfTerminalControl),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.AffectsRender,
                OnCursorRowChanged, CoerceCursorRow));

        private static object CoerceCursorRow(DependencyObject d, object basevalue)
        {
            // int row = (int) basevalue;
            // WpfTerminalControl w = d;
            // if (w.ViewY + w.NumRows <= row)
            // {

            // }
            return basevalue;
        }

        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
            "BackgroundColor", typeof(ConsoleColor), typeof(WpfTerminalControl),
            new PropertyMetadata(ConsoleColor.Black, OnBackgroundColorChanged));

        public static readonly DependencyProperty VisibleRowCountProperty = DependencyProperty.Register(
            "VisibleRowCount", typeof(int), typeof(WpfTerminalControl),
            new PropertyMetadata(default(int), VisibleRowCountChanged));

        public static readonly DependencyProperty LineModeProperty = DependencyProperty.Register(
            "LineMode", typeof(bool), typeof(WpfTerminalControl),
            new PropertyMetadata(default(bool), PropertyChangedCallback));

        public static readonly DependencyProperty CursorColumnProperty = DependencyProperty.Register(
            "CursorColumn", typeof(int), typeof(WpfTerminalControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender,
                OnCursorColumnChanged, CoerceCursorColumn));

        [Obsolete()]
        public char CharUnderCursor
        {
            get { return (char) GetValue(CharUnderCursorProperty); }
            set { SetValue(CharUnderCursorProperty, value); }
        }

    /*
    private static void OnCharUnderCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnCharUnderCursorChanged((char) e.OldValue, (char) e.NewValue);
        }

        protected virtual void OnCharUnderCursorChanged(char oldValue, char newValue)
        {
        }
*/

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            UpdateStates(true);
        }

        private void UpdateStates(bool useTransitions)
        {
            if (IsFocused)
                VisualStateManager.GoToState(this, "Focused", useTransitions);
            else
                VisualStateManager.GoToState(this, "Unfocused", useTransitions);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            UpdateStates(true);
        }

        // Determine if a geometry within the visual was hit.
        private static Drawing HitTestGeometryInVisual([NotNull] Visual visual, Point pt)
        {
            if (visual == null) throw new ArgumentNullException(nameof(visual));
            // Retrieve the group of drawings for the visual.
            var drawingGroup = VisualTreeHelper.GetDrawing(visual);
            return EnumDrawingGroup(drawingGroup, pt);
        }

        // Enumerate the drawings in the DrawingGroup.
        private static Drawing EnumDrawingGroup(DrawingGroup drawingGroup, Point pt)
        {
            var drawingCollection = drawingGroup.Children;

            // Enumerate the drawings in the DrawingCollection.
            foreach (var drawing in drawingCollection)
                // If the drawing is a DrawingGroup, call the function recursively.if (drawing.GetType() == typeof(DrawingGroup))
                switch (drawing)
                {
                    case DrawingGroup drawingGroup1:
                        var o = EnumDrawingGroup(drawingGroup1, pt);
                        if (o != null)
                            return o;

                        break;
                    case GeometryDrawing geometryDrawing:
                        break;
                    case GlyphRunDrawing glyphRunDrawing:
                        if (glyphRunDrawing.Bounds.Contains(pt)) return glyphRunDrawing;
                        break;
                    case ImageDrawing imageDrawing:
                        break;
                    case VideoDrawing videoDrawing:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(drawing));
                }

            return null;
        }

#if ROWDGMODE
 public bool SetCellCharacter(int row, int col, char char1, ConsoleColor? foregroundColor = null,
            ConsoleColor? backgroundColor = null, bool expandRow = false, bool advanceCursor = true)
        {
            if (NumColumns == -1 || NumRows == -1)
                return false;
            if (Rect1 != null)
            {
                var dr1 = HitTestGeometryInVisual(Rect1, new Point(col * CellWidth, row * CellHeight));
                if (dr1 != null)
                {
                }
            }
            else
            {
            }

            //if (col < 0 || col >= NumColumns) throw new ArgumentOutOfRangeException($"{col} >= NumColumns{NumColumns} || < 0", nameof(col));
            // if (row >= _buffer2.Count)
            // throw new InvalidControlState($"buffer length is {_buffer2.Count} _col is {col}");

            var bRow = TerminalRowToBufferRow(row);
#if ROWDBMODE
            if (bRow >= BufferRowCount)
                AddRowsToBuffer(bRow + 1);
#else
            if (bRow >= BufferRowCount)
                AddRowsToBuffer2(bRow + 1);
#endif

            var rowL = _buffer2[bRow];

            if (!backgroundColor.HasValue) backgroundColor = BackgroundColor;

            if (!foregroundColor.HasValue) foregroundColor = ForegroundColor;


            var bCol = TerminalColToBufferCol(col);
            if (rowL.Count < NumColumns) rowL.AddRange(Enumerable.Repeat('\0', NumColumns - rowL.Count));


            if (rowL.Count <= bCol)
            {
                if (!expandRow)
                    return false;
                while (rowL.Count <= col - 1) rowL.Add('\0');

                rowL.Add(char1);
            }
            else
            {
                rowL[bCol] = char1;
            }
#if ROWDBMODE
            DrawingGroup rowDrawingGroup = null;
            try
            {
                rowDrawingGroup = GetRowDrawingGroup(row, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("row is " + row);
                throw;
            }

            if (rowDrawingGroup.Children.Count <= col)
                while (rowDrawingGroup.Children.Count <= col)
                    rowDrawingGroup.Children.Add(new DrawingGroup());
            var drawingGroup = (DrawingGroup) rowDrawingGroup.Children[col];

            var dc = drawingGroup.Open();
            var cellOrigin = GetCellOrigin(row, col);
            _debug($"cell origin is {cellOrigin.X:N2} {cellOrigin.Y:N2}");
            dc.DrawRectangle(colors[(int) backgroundColor], null,
                new Rect(cellOrigin, new Size(xadvance, yadvance)));
            var x1 = new FormattedText(char1.ToString(), Thread.CurrentThread.CurrentUICulture,
                FlowDirection.LeftToRight, _typeface, (double) GetValue(FontSizeProperty), colors[(int) foregroundColor],
                _pixelsPerDip);
            dc.DrawText(x1, cellOrigin);
            Logger.Trace($"Drawtext {char1} at {cellOrigin}");
            Trace.WriteLine($"{cellOrigin}");
            dc.Close();
            Logger.Trace($"{DrawingGroup.Bounds}");
#endif

            //drawingGroup.Freeze();
            if (advanceCursor)
            {
                CursorColumn = col + 1;
                CheckCursorForOverrun();
            }

            return true;
        }


#else
        public bool SetCellCharacter(int row, int col, char char1, ConsoleColor? foregroundColor = null,
            ConsoleColor? backgroundColor = null, bool expandRow = false, bool advanceCursor = true)
        {
            if (NumColumns == -1 || NumRows == -1)
                return false;
            if (_rect != null)
            {
                var dr1 = HitTestGeometryInVisual(_rect, new Point(col * CellWidth, row * CellHeight));
                if (dr1 != null)
                {
                }
            }
            else
            {
            }

            //if (col < 0 || col >= NumColumns) throw new ArgumentOutOfRangeException($"{col} >= NumColumns{NumColumns} || < 0", nameof(col));
            // if (row >= _buffer2.Count)
            // throw new InvalidControlState($"buffer length is {_buffer2.Count} _col is {col}");

            var bRow = TerminalRowToBufferRow(row);
#if ROWDBMODE
            if (bRow >= BufferRowCount)
                AddRowsToBuffer(bRow + 1);
#else
            if (bRow >= BufferRowCount)
                AddRowsToBuffer2(bRow + 1);
#endif

            var rowL = _buffer2[bRow];

            if (!backgroundColor.HasValue) backgroundColor = BackgroundColor;

            if (!foregroundColor.HasValue) foregroundColor = ForegroundColor;


            var bCol = TerminalColToBufferCol(col);
            if (rowL.Count < NumColumns) rowL.AddRange(Enumerable.Repeat('\0', NumColumns - rowL.Count));


            if (rowL.Count <= bCol)
            {
                if (!expandRow)
                    return false;
                while (rowL.Count <= col - 1) rowL.Add('\0');

                rowL.Add(char1);
            }
            else
            {
                rowL[bCol] = char1;
            }

            var cellOrigin = GetCellOrigin(row, col);
            _debug($"cell origin is {cellOrigin.X:N2} {cellOrigin.Y:N2}");


            if (_drawingContext == null)
            {
                _drawingContext = DrawingGroup.Append();
            }
                _drawingContext.DrawRectangle(colors[(int) backgroundColor], null,
                    new Rect(cellOrigin, new Size(xadvance, yadvance)));
                
            var x1 = new FormattedText(char1.ToString(), Thread.CurrentThread.CurrentUICulture,
                    FlowDirection.LeftToRight, Typeface, (double) GetValue(FontSizeProperty),
                    colors[(int) foregroundColor],
                    _pixelsPerDip);
                _drawingContext.DrawText(x1, cellOrigin);
                Logger.Trace($"Drawtext {char1} at {cellOrigin}");
                Trace.WriteLine($"{cellOrigin}");
                _drawingContext.Close();
                _drawingContext = null;

                // if (Translate != null)
                // {
                    // Translate.X = DrawingGroup.Bounds.Left;
                    // CalcTransY();
                    // TranslateX = Translate.X;
                    // TranslateY = Translate.Y;
                // }
            


            // _drawingContext = DrawingGroup.Append();
            Logger.Trace($"{DrawingGroup.Bounds}");


            //drawingGroup.Freeze();
            if (advanceCursor)
            {
                CursorColumn = col + 1;
                CheckCursorForOverrun();
            }

            return true;
        }

        private void CalcTransY()
        {
            Translate.Y = -1* ViewY * CellHeight;//-1 * NumRows * CellHeight + DrawingGroup.Bounds.Bottom;
        }
#endif

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            VScrollBar = (ScrollBar) GetTemplateChild("VScrollBar");
            if (VScrollBar != null) VScrollBar.Minimum = 0;
            Border = GetTemplateChild("Bd") as Border;
            Translate = GetTemplateChild("Translate") as TranslateTransform;
            //Translate = new TranslateTransform();
            DrawingGroup.Transform = Translate;
            Brush1 = (DrawingBrush) GetTemplateChild("DrawingBrush");
            Brush2 = (DrawingBrush) GetTemplateChild("DrawingBrush2");
            Minibrush = (DrawingBrush) GetTemplateChild("MiniBrush");
            Zoombrush = (DrawingBrush) GetTemplateChild("ZoomBrush");
            Popup1 = (Popup) GetTemplateChild("Magnifier");
            if (Minibrush != null) Minibrush.Drawing = DrawingGroup;
            Zoombrush.Drawing = DrawingGroup;
            // _brush.Viewport = new Rect(0, 0, NumColumns * CellWidth, NumRows * CellHeight);
            // _brush.ViewportUnits = BrushMappingMode.Absolute;

            // _brush.Viewbox = new Rect(0, 0, NumColumns * CellWidth, NumRows * CellHeight);
             // _brush.ViewboxUnits = BrushMappingMode.Absolute;

            SetBinding(BrushViewportXProperty, new Binding("Viewport.X") {Source = Brush1});
            SetBinding(BrushViewportYProperty, new Binding("Viewport.Y") {Source = Brush1});
            SetBinding(BrushViewportWidthProperty, new Binding("Viewport.Width") {Source = Brush1});
            SetBinding(BrushViewportHeightProperty, new Binding("Viewport.Height") {Source = Brush1});

            if (Brush1 != null) Brush1.Drawing = DrawingGroup;
            MiniRect = (Rectangle) GetTemplateChild("MiniRect");
            // Popup1.MouseEnter += PopupOnMouseEnter;
            // Popup1.MouseLeave += PopupOnMouseLeave;

            Rect1 = (Rectangle) GetTemplateChild("Rect");
            if(NumRows!=-1){
            var height = NumRows * CellHeight;
            if (Rect1 != null) Rect1.Height = height;
            }

            if (NumColumns != -1)
            {
                var width = NumColumns * CellWidth;
                Logger.Info($"{width}");
                if (Rect1 != null) Rect1.Width = width;
            }


            Rect2 = (Rectangle) GetTemplateChild("Rect2");
            ColLabel = (Rectangle) GetTemplateChild("ColLabel");
          
            ColLabel2 = (Rectangle)GetTemplateChild("ColLabel2");
            DG0 = (DrawingGroup) GetTemplateChild("ColLabel1DrawingGroup");
            ColLabel2DrawingGroup = (DrawingGroup)GetTemplateChild("ColLabel2DrawingGroup");
            ColLabel2Transform = (TranslateTransform) GetTemplateChild("ColLabel2Transform");
            DG2 = (DrawingGroup) GetTemplateChild("DG2");


            if (ColLabel2 != null && NumColumns != -1)
            {
                DrawColLabel2(NumColumns);
            }
            if (ColLabel != null && NumColumns != -1)
            {
                DrawColLabel1(NumColumns);
            }
#if ROWDGMODE
            NewMethod();
#endif
            UiInitialized = true;
            CoerceValue(CursorColumnProperty);
            CoerceValue(CursorRowProperty);
            UpdateStates(true);
        }

        public Rectangle ColLabel2 { get; set; }

        public TranslateTransform ColLabel2Transform { get; set; }

        private DrawingGroup DG0
        {
            get { return _dg0; }
            set
            {
                if (Equals(value, _dg0)) return;
                _dg0 = value;
                OnPropertyChanged();
            }
        }

        private Rectangle ColLabel
        {
            get { return _colLabel; }
            set
            {
                if (Equals(value, _colLabel)) return;
                _colLabel = value;
                OnPropertyChanged();
            }
        }

        private DrawingGroup DG2
        {
            get { return _dg2; }
            set
            {
                if (Equals(value, _dg2)) return;
                _dg2 = value;
                if (NumRows == -1) return;
                var dc = _dg2.Open();
                var solidColorBrush = new SolidColorBrush(Colors.Gray) {Opacity = .5};
                dc.DrawRectangle(solidColorBrush, null, new Rect(new Point(0, 0), new Size(1, 1 / NumRows)));
                dc.DrawRectangle(Brushes.Azure, null, new Rect(new Point(0, 1 / NumRows), new Size(1, 1 / NumRows)));
                dc.Close();
            }
        }

        private Rectangle Rect2
        {
            get { return _rect2; }
            set
            {
                if (Equals(value, _rect2)) return;
                _rect2 = value;

                OnPropertyChanged();
            }
        }

        private void MiniRectOnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(MiniRect);
            if (p.X < 0 || p.Y < 0 || p.X >= MiniRect.ActualWidth || p.Y >= MiniRect.ActualHeight)
            {
                MiniRect.ReleaseMouseCapture();
                Popup1.IsOpen = false;
                return;
            }

            var xr = p.X / MiniRect.ActualWidth;
            var yr = p.Y / MiniRect.ActualHeight;
            var height = 24 / DrawingGroup.Bounds.Height * 8;
            var width = 32 / DrawingGroup.Bounds.Width * 8;
            Zoombrush.Viewport = new Rect(0, 0, 320, 240); //xr, yr, width, height);

            Zoombrush.ViewportUnits = BrushMappingMode.Absolute;
            Zoombrush.Viewbox = new Rect(xr, yr, width, height);
            Debug.WriteLine(Zoombrush.Viewport);
            e.Handled = true;
        }

        private void MiniRectOnMouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(MiniRect);
            var xr = p.X / MiniRect.ActualWidth;
            var yr = p.Y / MiniRect.ActualHeight;
            var height = 24 / DrawingGroup.Bounds.Height * 8;
            var width = 32 / DrawingGroup.Bounds.Width * 8;
            Zoombrush.Viewport = new Rect(0, 0, 320, 240); //xr, yr, width, height);

            Zoombrush.ViewportUnits = BrushMappingMode.Absolute;
            Zoombrush.Viewbox = new Rect(xr, yr, width, height);
            Debug.WriteLine(Zoombrush.Viewport);
        }

        private void MiniRectOnMouseLeave(object sender, MouseEventArgs e)
        {
            // _popup.IsOpen= false;
        }

        private void MinirectOnMouseEnter(object sender, MouseEventArgs e)
        {
            Popup1.StaysOpen = true;
            if (!Popup1.IsOpen)

            {
                MiniRect.CaptureMouse();
                var p = e.GetPosition(MiniRect);
                var r = new Rect(p.X - Popup1.ActualWidth / 2, p.Y - Popup1.ActualHeight / 2, Popup1.ActualWidth, 10);
                Popup1.PlacementRectangle = r;
                Popup1.Placement = PlacementMode.Bottom;
                Popup1.IsOpen = true;
            }
        }

        private void AddRowsToBuffer2(in int newNumRows)
        {
            var j = _buffer2.Count;
            var origin = GetCellOrigin(j, 0);
            while (_buffer2.Count < newNumRows)

            {
                MakeRow(_buffer2, j);

                var lineno = $"{j:D4}";
                for (var i = 0; i < Math.Min(NumReservedColumns - 1, NumColumns); i++)
                    _buffer2[j][i] = lineno[i];

                j++;
                origin.Y += yadvance;
                origin.X = NumReservedColumns * CellWidth;
            }

            BufferRowCount = _buffer2.Count;
            if (VScrollBar != null) VScrollBar.Maximum = BufferRowCount - NumRows / 2;
            _debug("BufferRow count is " + BufferRowCount);
        }

        private void MakeRow(List<List<char>> buffer2, int i)
        {
            buffer2.Add(Enumerable.Range(0, NumColumns)
                .Select(n => '\0').ToList());
        }
#if ROWDGMMODE
        private void AddRowsToBuffer(in int newNumRows)
        {
            if (!RowDgMode)
            {
                AddRowsToBuffer2(newNumRows);
                return;
            }

            while (_groups.Count < newNumRows) _gr oups.Add(null);
            Logger.Info("AddRowsToBuffer " + newNumRows);
            var j = _buffer2.Count;
            var origin = GetCellOrigin(j, 0);
            while (_buffer2.Count < newNumRows)

            {
                _buffer2.Add(Enumerable.Range(0, NumColumns).Select(n => '\0').ToList());

                var lineno = $"{j:D4}";
                for (var i = 0; i < NumReservedColumns - 1; i++) _buffer2[j][i] = lineno[i];


                var g1 = new DrawingGroup();
                // var gg0 = new DrawingGroup();
                // var x0 = new FormattedText(lineno,
                // Thread.CurrentThread.CurrentUICulture,
                // FlowDirection.LeftToRight, _typeface, FontSize, Brushes.Black,
                // _pixelsPerDip);
                // var dc0 = gg0.Open();
                // var cellOrigin = GetCellOrigin(j, 0);
                // cellOrigin.X = 0;
                // dc0.DrawText(x0, cellOrigin);
                // dc0.Close();
                // g1.Children.Add(gg0);
                for (var i = 0; i < NumColumns; i++)
                {
                    var gg = new DrawingGroup();
                    var x1 = new FormattedText(_buffer2[j][i].ToString(),
                        Thread.CurrentThread.CurrentUICulture,
                        FlowDirection.LeftToRight, _typeface, FontSize, Brushes.Black,
                        _pixelsPerDip);
                    var dc_ = gg.Open();
                    dc_.DrawText(x1, origin);

                    dc_.Close();
                    origin.X += xadvance;
                    g1.Children.Add(gg);
                }


                Debug.WriteLine($"{DrawingGroup.Children.Count}");
                Debug.WriteLine($"index {DrawingGroup.Children.Count} for row {j}");
                AddDrawingGroupChild(g1, j);

                j++;
                origin.Y += yadvance;
                origin.X = 0; //NumReservedColumns * CellWidth;
            }

            BufferRowCount = _buffer2.Count;
            if (VScrollBar != null) VScrollBar.Maximum = BufferRowCount - NumRows / 2;
            Debug.WriteLine("BufferRoWcount is " + BufferRowCount);
        }
        private void AddDrawingGroupChild(DrawingGroup g1, int? row)
        {
            if (!RowDgMode)
                return;
            _debug($"Adding drawing group for row {row} {g1}");
            if (row.HasValue) _groups[row.Value] = g1;

            DrawingGroup.Children.Add(g1);
        }

        private void NewMethod()
        {
            return;
            var fontSize = FontSize;
            var gli = _glyphTypeface.CharacterToGlyphMap['p'];
            yadvance = _glyphTypeface.AdvanceHeights[gli] * fontSize;
            xadvance = _glyphTypeface.AdvanceWidths[gli] * fontSize;
            _yd2 = _glyphTypeface.DistancesFromHorizontalBaselineToBlackBoxBottom[gli] * fontSize;

            CellHeight = yadvance;
            CellWidth = xadvance;

            _debug($"Adding rows to buffer. {NumRows}");
            AddRowsToBuffer(NumRows);

            var dispatcherTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 300), DispatcherPriority.Input,
                Callbaack, Dispatcher);
            dispatcherTimer.Start();

            var width = NumColumns * CellWidth;
            var height = NumRows * CellHeight;
            if (_rect != null)
            {
                _rect.Width = width;
                _rect.Height = height;
            }
        
            // DrawingGroup.Children.Clear();
            // DrawingGroup.Children.Add(new GeometryDrawing(Background, null,
            // new RectangleGeometry(new Rect(0, 0, width, height))));

            // _rowDrawingGroupStartIndnex = DrawingGroup.Children.Count;
            // double y = 0;
            // foreach (var line in _buffer2)
            // {
            //     double x = 0;
            //     var g = new DrawingGroup();
            //
            //     foreach (var c in line)
            //     {
            //         var gg = new DrawingGroup();
            //         var x1 = new FormattedText(c.ToString(), Thread.CurrentThread.CurrentUICulture,
            //             FlowDirection.LeftToRight, _typeface, (double)GetValue(FontSizeProperty), Brushes.Black,
            //             VisualTreeHelper.GetDpi(this).PixelsPerDip);
            //         var dc_ = gg.Open();
            //         dc_.DrawText(x1, new Point(x, y));
            //         dc_.Close();
            //         x += xadvance;
            //         g.Children.Add(gg);
            //     }
            //
            //
            //     y += yadvance;
            //     DrawingGroup.Children.Add(g);
            // }

            if (_brush != null) _brush.Drawing = DrawingGroup;
        }
#endif

        private void CommonInit()
        {
            var fontSize = FontSize;
            var gli = _glyphTypeface.CharacterToGlyphMap['p'];
            yadvance = _glyphTypeface.AdvanceHeights[gli] * fontSize;
            xadvance = _glyphTypeface.AdvanceWidths[gli] * fontSize;
            _yd2 = _glyphTypeface.DistancesFromHorizontalBaselineToBlackBoxBottom[gli] * fontSize;

            CellHeight = yadvance;
            CellWidth = xadvance;

            if (Brush1 != null) Brush1.Drawing = DrawingGroup;
        }

#if DEBUGz
        protected Logger Logger = LogManager.GetCurrentClassLogger();
#else
        protected Logger Logger = LogManager.CreateNullLogger();
#endif
        private readonly Brush[] colors =
        {
            Brushes.Black, Brushes.DarkBlue, Brushes.DarkGreen, Brushes.DarkCyan, Brushes.DarkRed, Brushes.DarkMagenta,
            new SolidColorBrush(Color.FromArgb(0, 0x80, 0x80, 0)),
            Brushes.Gray, Brushes.DarkGray, Brushes.Blue, Brushes.Lime, Brushes.Cyan, Brushes.Red, Brushes.Magenta,
            Brushes.Yellow, Brushes.White
        };

        public readonly List<List<char>> _buffer2 = new List<List<char>>();
        private ScrollBar _vScrollBar;

        private DrawingBrush _brush;
        private Rectangle _rect;

        private GlyphTypeface _glyphTypeface;
        private double yadvance;
        private double xadvance;

        private TranslateTransform Translate
        {
            get { return _translate; }
            set
            {
                if (Equals(value, _translate)) return;
                _translate = value;
                OnPropertyChanged();
            }
        }

     
        private static object CoerceCursorColumn(DependencyObject d, object basevalue)
        {
            var t = (WpfTerminalControl) d;
            t._debug($"{nameof(CoerceCursorColumn)}");
            if (t.DebugMode) return basevalue;

            if ((int) basevalue < t.NumReservedColumns) return t.NumReservedColumns;

            return basevalue;
        }

        public int CursorSize { get; set; }

        public static readonly DependencyProperty NumReservedColumnsProperty = DependencyProperty.Register(
            "NumReservedColumns", typeof(int), typeof(WpfTerminalControl),
            new PropertyMetadata(5, OnNumReservedColumnsChanged));

        public int NumReservedColumns
        {
            get { return (int) GetValue(NumReservedColumnsProperty); }
            set { SetValue(NumReservedColumnsProperty, value); }
        }

        private static void OnNumReservedColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnNumReservedColumnsChanged((int) e.OldValue, (int) e.NewValue);
        }

        protected virtual void OnNumReservedColumnsChanged(int oldValue, int newValue)
        {
        }

        private readonly double _pixelsPerDip;

        private static FontFamily _consoleFontFamily;
        public static FontFamily ConsoleFontFamily
        {
            get
            {
                if (_consoleFontFamily == null) _consoleFontFamily = new FontFamily("Courier New");

                return _consoleFontFamily;
            }
        }

        static WpfTerminalControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfTerminalControl),
                new FrameworkPropertyMetadata(typeof(WpfTerminalControl)));
            FontSizeProperty.OverrideMetadata(typeof(WpfTerminalControl),
                new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.None, FontSizeChanged));
            FontFamilyProperty.OverrideMetadata(typeof(WpfTerminalControl),
                new FrameworkPropertyMetadata(ConsoleFontFamily, FrameworkPropertyMetadataOptions.None,
                    OnFontFamilyChanged));
        }

        public static readonly DependencyProperty NumRowsProperty =
            TerminalCharacteristics.NumRowsProperty.AddOwner(typeof(WpfTerminalControl),
                new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.None, OnNumRowsChanged,
                    CoerceNumRows));

        public static readonly DependencyProperty NumColumnsProperty =
            TerminalCharacteristics.NumColumnsProperty.AddOwner(typeof(WpfTerminalControl),
                new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.None, OnNumColumnsChanged));

        private static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnFontFamilyChanged((FontFamily) e.OldValue,
                (FontFamily) e.NewValue);
        }

        private void OnFontFamilyChanged(FontFamily eOldValue, FontFamily eNewValue)
        {
            Logger.Info(nameof(OnFontFamilyChanged));
            Typeface = new Typeface(eNewValue, FontStyle, FontWeight, FontStretch);

            if (!Typeface.TryGetGlyphTypeface(out _glyphTypeface))
                throw new InvalidControlState("Unable to get glyph typeface");
            var fontSize = FontSize;
            var gli = _glyphTypeface.CharacterToGlyphMap['p'];
            yadvance = _glyphTypeface.AdvanceHeights[gli] * fontSize;
            xadvance = _glyphTypeface.AdvanceWidths[gli] * fontSize;
            _yd2 = _glyphTypeface.DistancesFromHorizontalBaselineToBlackBoxBottom[gli] * fontSize;
            CellHeight = yadvance;
            CellWidth = xadvance;
#if ROWDGMODE
            if (NumRows > 0 && NumColumns > 0)
                Redraw();
#endif
        }

        public WpfTerminalControl()
        {
            Typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            if (!Typeface.TryGetGlyphTypeface(out _glyphTypeface))
                throw new InvalidControlState("Unable to get glyph typeface");

            _drawingGroup = new DrawingGroup();

#if ROWDGMODE
            if (RowDgMode)
            {
                ResetDrawingGroup();
                NewMethod();
            }
            else
#endif
            {
                CommonInit();
                _drawingContext = DrawingGroup.Open();
            }
            Focusable = true;

            _pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        }

#if ROWDBMODE
        public static bool RowDgMode { get; set; } = true;
#else
        public static bool RowDgMode { get; set; } = false;
#endif

#if ROWDBMODE
        private void ResetDrawingGroup()
        {
            for (var i = 0; i < _groups.Count; i++) _groups[i] = null;
            DrawingGroup.Children.Clear();
            DrawingGroup.Children.Add(new GeometryDrawing(Background, null,
                new RectangleGeometry(new Rect(0, 0, 1, 1))));
        }
#endif
        private static void OnAutoResizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnAutoResizeChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnAutoResizeChanged(bool oldValue, bool newValue)
        {
        }

        private static object CoerceNumRows(DependencyObject d, object basevalue)
        {
            if ((bool) d.GetValue(LineModeProperty)) return 1;

            return basevalue;
        }

        private static void OnNumRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnNumRowsChanged((int) e.OldValue, (int) e.NewValue);
        }

        protected virtual void OnNumRowsChanged(int eOldValue, int eNewValue)
        {
            // var ev = new RoutedPropertyChangedEventArgs<int>(eOldValue, eNewValue, NumRowsChangedEvent);
            // RaiseEvent(ev);

            Logger.Debug($"NumRows = {eNewValue} ");
            if (eNewValue == -1) return;

            if (NumColumns != -1)
            {
                if (Brush2 != null)
                {
                    Brush2.Viewport = new Rect(0, 0, 1.0 / NumColumns, 1.0
                                                                       / eNewValue);
                    Brush2.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
                }
            }

            if (eNewValue > eOldValue && NumColumns != -1)
            {
#if ROWDBMODE
                AddRowsToBuffer(eNewValue);
#else
                AddRowsToBuffer2(eNewValue);
#endif
            }

            // if (_brush != null)
            // {
            // _brush.Viewport = new Rect(0, 0, NumColumns * CellWidth, eNewValue * CellHeight);
            // _brush.ViewportUnits = BrushMappingMode.Absolute;
            // _brush.Viewbox = new Rect(0, 0, NumColumns * CellWidth, eNewValue * CellHeight);
            // _brush.ViewboxUnits = BrushMappingMode.Absolute;
            // }

            // var dc = DG2.Open();
            // var solidColorBrush = new SolidColorBrush(Colors.Gray) { Opacity = .5 };
            // dc.DrawRectangle(solidColorBrush, null, new Rect(new Point(0, 0), new Size(1, 0.5)));
            // dc.DrawRectangle(Brushes.Azure, null, new Rect(new Point(0, 0.5), new Size(1, 0.5)));
            // dc.Close();

            var height = eNewValue * CellHeight;
            if (Rect1 != null) Rect1.Height = height;
#if ROWDGMODE
            Redraw();
#endif
        }

        public bool AutoResize
        {
            get { return (bool) GetValue(AutoResizeProperty); }
            set { SetValue(AutoResizeProperty, value); }
        }

        public int NumRows
        {
            get { return (int) GetValue(NumRowsProperty); }
            set { SetValue(NumRowsProperty, value); }
        }

        public Brush CursorBrush
        {
            get { return (Brush) GetValue(CursorBrushProperty); }
            set { SetValue(CursorBrushProperty, value); }
        }

        private static void OnNumColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnNumColumnsChanged((int) e.OldValue, (int) e.NewValue);
        }

        protected virtual void OnNumColumnsChanged(int eOldValue, int eNewValue)
        {
            // if (_brush != null)
            // {
            // _brush.Viewport = new Rect(0, 0, eNewValue * CellWidth, eNewValue * CellHeight);
            // _brush.ViewportUnits = BrushMappingMode.Absolute;
            // _brush.Viewbox = new Rect(0, 0, eNewValue * CellWidth, eNewValue * CellHeight);
            // _brush.ViewboxUnits = BrushMappingMode.Absolute;
            if (eNewValue == -1)
                return;

            if (NumRows != -1)
            {
                if (Brush2 != null)
                {
                    Brush2.Viewport = new Rect(0, 0, 1.0 / eNewValue, 1.0 / NumRows);
                    Brush2.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
                }
            }

            DrawColLabel1(eNewValue);


            DrawColLabel2(eNewValue);

            var width = eNewValue * CellWidth;
            Logger.Info($"{width}");
            if (Rect1 != null) Rect1.Width = width;
        }

        private void DrawColLabel1(int eNewValue)
        {
            if (DG0 != null)
            {
                var dc = DG0.Open();
                var origin = new Point(0, 0);


                /* If font size computed relative to each individual line is too small (defined here as less than 16pt), increase font size
                to 28pt. */

                for (var i = 0; i < eNewValue; i++)
                {
                    var textToFormat = i.ToString("D2")[0].ToString();
                    var tt = new FormattedText(textToFormat, CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        Typeface, FontSize, Brushes.Black, _pixelsPerDip);
                    tt.SetFontWeight(FontWeights.ExtraBold);
                    dc.DrawText(tt, origin);
                    origin.X += xadvance;
                }

                origin.X = 0;
                origin.Y += yadvance;
                for (var i = 0; i < eNewValue; i++)
                {
                    var tt = new FormattedText(i.ToString("D2")[1].ToString(), CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        Typeface, FontSize, Brushes.Black, _pixelsPerDip);
                    tt.SetFontWeight(FontWeights.ExtraBold);
                    dc.DrawText(tt, origin);
                    origin.X += xadvance;
                }

                dc.Close();
            }
        }

        private void DrawColLabel2(int eNewValue)
        {
            if (ColLabel2DrawingGroup != null)
            {
                var dc = ColLabel2DrawingGroup.Open();
                var origin = new Point(0, 0);

                var fontSize = FontSize * (xadvance / yadvance);
                /* If font size computed relative to each individual line is too small (defined here as less than 16pt), increase font size
                to 28pt. */
                if (fontSize < 16) fontSize = 28;
                var gli = _glyphTypeface.CharacterToGlyphMap['x'];
                var xadvance1 = _glyphTypeface.AdvanceWidths[gli] * fontSize;
                var yadvance1 = _glyphTypeface.AdvanceHeights[gli] * fontSize;

                for (var i = 0; i < eNewValue; i++)
                {
                    var tt = new FormattedText(i.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        Typeface, fontSize, Brushes.Black, _pixelsPerDip);
                    tt.SetFontWeight(FontWeights.ExtraBold);
                    dc.DrawText(tt, origin);

                    //origin.X += tt.Width;


                    if (yadvance1 > CellWidth)
                    {
                        var a = yadvance * 2;
                        if (CellWidth * 2 - a < 5)
                            origin.Y += a;
                        else
                            origin.Y += a + yadvance1;

                        // while (a < CellWidth * 2)
                        // {
                        // a += 
                        // }
                        // while()
                        // origin.Y += CellWidth / (yadvance );
                        i++;
                    }
                    else
                    {
                        origin.Y += CellWidth;
                    }
                }

                dc.Close();
            }
        }

        public DrawingGroup ColLabel2DrawingGroup { get; set; }

        public static readonly DependencyProperty RectWidthProperty = DependencyProperty.Register(
            "RectWidth", typeof(float), typeof(WpfTerminalControl),
            new PropertyMetadata(default(float), OnRectWidthChanged));

        public float RectWidth
        {
            get { return (float) GetValue(RectWidthProperty); }
            set { SetValue(RectWidthProperty, value); }
        }

        private static void OnRectWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnRectWidthChanged((float) e.OldValue, (float) e.NewValue);
        }


        protected virtual void OnRectWidthChanged(float oldValue, float newValue)
        {
        }

        public static readonly DependencyProperty RectHeightProperty = DependencyProperty.Register(
            "RectHeight", typeof(float), typeof(WpfTerminalControl),
            new PropertyMetadata(default(float), OnRectHeightChanged));

        public float RectHeight
        {
            get { return (float) GetValue(RectHeightProperty); }
            set { SetValue(RectHeightProperty, value); }
        }
        private static void OnRectHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnRectHeightChanged((float) e.OldValue, (float) e.NewValue);
        }

        protected virtual void OnRectHeightChanged(float oldValue, float newValue)
        {
        }

        public int NumColumns
        {
            get { return (int) GetValue(NumColumnsProperty); }
            set { SetValue(NumColumnsProperty, value); }
        }

        private static void FontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnFontSizeChanged((double) e.OldValue, (double) e.NewValue);
        }

        protected virtual void OnFontSizeChanged(double oldValue, double newValue)
        {
            var gli = _glyphTypeface.CharacterToGlyphMap['x'];
            xadvance = CellWidth = _glyphTypeface.AdvanceWidths[gli] * newValue;
            yadvance = CellHeight = _glyphTypeface.AdvanceHeights[gli] * newValue;

            _yd2 = _glyphTypeface.DistancesFromHorizontalBaselineToBlackBoxBottom[gli] * newValue;
            if (Brush2 != null)
            {
                // Brush2.Viewport = new Rect(0, 0, CellWidth * 2, CellHeight * 2);
                // Brush2.ViewportUnits = BrushMappingMode.Absolute;
            }

#if ROWDBMODE
            if (NumRows > 0 && NumColumns > 0)
                Redraw();
#endif
            Debug.WriteLine($"New Cell size is {new Size(CellWidth, CellHeight)}");
        }
#if ROWDBMODE
        private void Redraw()
        {
            if (!RowDgMode)
                return;
            Logger.Info("Redraw");
            ResetDrawingGroup();

            var origin = GetCellOrigin(ViewY, ViewX);
            for (var i = ViewY; i < ViewY + NumRows; i++)
            {
                var startx = origin.X;
                var g1 = new DrawingGroup();
                var chars = _buffer2[i];

                for (var j = ViewX; j < ViewX + NumColumns; j++)
                {
                    var gg = new DrawingGroup();
                    if (chars.Count <= j)
                        break;
                    var x1 = new FormattedText(chars[j].ToString(),
                        Thread.CurrentThread.CurrentUICulture,
                        FlowDirection.LeftToRight, _typeface, FontSize, Brushes.Black, //fixme
                        _pixelsPerDip);
                    var dc_ = gg.Open();
                    dc_.DrawText(x1, origin);
                    dc_.Close();
                    origin.X += xadvance;
                    g1.Children.Add(gg);
                }

                AddDrawingGroupChild(g1, i);
                origin.Y += yadvance;
                origin.X = startx;
            }
        }
#endif
        public bool InputOnly
        {
            get { return (bool) GetValue(InputOnlyProperty); }
            set { SetValue(InputOnlyProperty, value); }
        }

        public static readonly DependencyProperty BrushViewportXProperty = DependencyProperty.Register(
            "BrushViewportX", typeof(double), typeof(WpfTerminalControl),
            new PropertyMetadata(default(double), OnBrushViewportXChanged));

        public double BrushViewportX
        {
            get { return (double) GetValue(BrushViewportXProperty); }
            set { SetValue(BrushViewportXProperty, value); }
        }

        private static void OnBrushViewportXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnBrushViewportXChanged((double) e.OldValue, (double) e.NewValue);
        }

        protected virtual void OnBrushViewportXChanged(double oldValue, double newValue)
        {
        }

        public static readonly DependencyProperty BrushViewportYProperty = DependencyProperty.Register(
            "BrushViewportY", typeof(double), typeof(WpfTerminalControl),
            new PropertyMetadata(default(double), OnBrushViewportYChanged));

        public double BrushViewportY
        {
            get { return (double) GetValue(BrushViewportYProperty); }
            set { SetValue(BrushViewportYProperty, value); }
        }

        private static void OnBrushViewportYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnBrushViewportYChanged((double) e.OldValue, (double) e.NewValue);
        }

        public static readonly DependencyProperty BrushViewportWidthProperty = DependencyProperty.Register(
            "BrushViewportWidth", typeof(double), typeof(WpfTerminalControl),
            new PropertyMetadata(default(double), OnBrushViewportWidthChanged));

        public double BrushViewportWidth
        {
            get { return (double) GetValue(BrushViewportWidthProperty); }
            set { SetValue(BrushViewportWidthProperty, value); }
        }

        private static void OnBrushViewportWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnBrushViewportWidthChanged((double) e.OldValue, (double) e.NewValue);
        }

        protected virtual void OnBrushViewportWidthChanged(double oldValue, double newValue)
        {
        }

        protected virtual void OnBrushViewportYChanged(double oldValue, double newValue)
        {
        }

        public static readonly DependencyProperty BrushViewportHeightProperty = DependencyProperty.Register(
            "BrushViewportHeight", typeof(double), typeof(WpfTerminalControl),
            new PropertyMetadata(default(double), OnBrushViewportHeightChanged));

        public double BrushViewportHeight
        {
            get { return (double) GetValue(BrushViewportHeightProperty); }
            set { SetValue(BrushViewportHeightProperty, value); }
        }

        private static void OnBrushViewportHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnBrushViewportHeightChanged((double) e.OldValue, (double) e.NewValue);
        }

        protected virtual void OnBrushViewportHeightChanged(double oldValue, double newValue)
        {
        }

        private bool UiInitialized { get; set; }
        public Action<string> DebugCb { get; set; }

        public DrawingGroup DrawingGroup
        {
            get { return _drawingGroup; }
            set { _drawingGroup = value; }
        }


        private void _debug(string s)
        {
            DebugCb?.Invoke(s);
            Debug.WriteLine(s);
            Logger.Debug(s);
        }


        private void Callbaack(object sender, EventArgs e)
        {
            if (!ReadKeys.IsEmpty)
                if (ReadKeys.TryDequeue(out var result))
                    _readKeys = (ReadKeyOptions) result;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var measureOverride = InternalMeasure(constraint, out var c, out var r);
            return measureOverride;
        }

        private Size InternalMeasure(Size constraint, out int numColumns, out int numRows)
        {
            var excessWidth = 0.0;
            VScrollBar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            MiniRect.Measure(constraint);
            excessWidth += VScrollBar.DesiredSize.Width + BorderThickness.Left + BorderThickness.Right;// (DiagnosticsEnabled ? MiniRect.DesiredSize.Width: 0);
            ColLabel.Measure(constraint);
            ColLabel2.Measure(constraint);
            var excessHeight = BorderThickness.Top + BorderThickness.Bottom + ColLabel.DesiredSize.Height+ ColLabel2.DesiredSize.Height;
            if (Border != null)
            {
                excessHeight += Border.BorderThickness.Top + Border.BorderThickness.Bottom;
                excessWidth += Border.BorderThickness.Left + Border.BorderThickness.Right;
            }

            Logger.Info("Input " + constraint);
            numColumns = -1;
            numRows = -1;
            if (!AutoResize)
                return new Size(NumColumns * CellWidth + excessWidth, NumRows * CellHeight + excessHeight + _yd2);

            if (!double.IsPositiveInfinity(constraint.Width) && CellWidth != 0)
            {
                if (excessWidth > constraint.Width)
                    constraint.Width = 1;
                else
                    constraint.Width -= excessWidth;
                var w = constraint.Width / CellWidth;
                numColumns = (int) w;
                if (numColumns == 0)
                    numColumns = 1;

                Logger.Info("NumColumns would be " + numColumns);
            }
            else
            {
                if (NumColumns != -1)
                    constraint.Width = NumColumns * CellWidth + excessWidth;
                else
                    constraint.Width = 320;
            }


            if (!double.IsPositiveInfinity(constraint.Height) && CellHeight != 0)
            {
                var h = (constraint.Height - excessHeight) / CellHeight;
                numRows = (int) h;
            }
            else
            {
                if (NumRows != -1)
                    constraint.Height = NumRows * CellHeight + excessHeight;
                else
                    constraint.Height = 240;
            }

            Logger.Info("Resturning " + constraint);
            return constraint;
        }

        public Rectangle ColLabel2on { get; set; }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            _debug($"{nameof(ArrangeOverride)}({arrangeBounds}");
            var sz = InternalMeasure(arrangeBounds, out var cols, out var rows);
            if (AutoResize)
            {
                NumRows = rows;
                NumColumns = cols;
            }

            (GetTemplateChild("Border") as Border)?.Arrange(new Rect(arrangeBounds));
            (GetTemplateChild("Grid1") as Border)?.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        /// <inheritdoc />
        public override void EndInit()
        {
            base.EndInit();
        }

        public double CellHeight
        {
            get { return _cellHeight; }
            set { _cellHeight = value; }
        }

        public double CellWidth
        {
            get { return _cellWidth; }
            set { _cellWidth = value; }
        }

        public int ViewY
        {
            get { return (int) GetValue(ViewYProperty); }
            set { SetValue(ViewYProperty, value); }
        }

        private static void OnViewYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnViewYChanged((int) e.OldValue, (int) e.NewValue);
        }


        public int CursorRow
        {
            get { return (int) GetValue(CursorRowProperty); }
            set { SetValue(CursorRowProperty, value); }
        }


        public int CursorColumn
        {
            get { return (int) GetValue(CursorColumnProperty); }
            set { SetValue(CursorColumnProperty, value); }
        }

        public static readonly DependencyProperty ForegroundColorProperty = DependencyProperty.Register(
            "ForegroundColor", typeof(ConsoleColor), typeof(WpfTerminalControl),
            new PropertyMetadata(ConsoleColor.Black, OnForegroundColorChanged));

        public ConsoleColor ForegroundColor
        {
            get { return (ConsoleColor) GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }


        private ReadKeyOptions? _readKeys;

        public ConsoleColor BackgroundColor
        {
            get { return (ConsoleColor) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        private static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnBackgroundColorChanged((ConsoleColor) e.OldValue, (ConsoleColor) e.NewValue);
        }


        private void OnBackgroundColorChanged(ConsoleColor oldValue, ConsoleColor newValue)
        {
            _debug("Background now " + newValue);
        }


        private static void OnForegroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnForegroundColorChanged((ConsoleColor) e.OldValue, (ConsoleColor) e.NewValue);
        }


        private void OnForegroundColorChanged(ConsoleColor oldValue, ConsoleColor newValue)
        {
            _debug("Foreground now " + newValue);
        }


        private static void OnCursorColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnCursorColumnChanged((int) e.OldValue, (int) e.NewValue);
        }


        protected virtual void OnCursorColumnChanged(int oldValue, int newValue)
        {
            _debug("Cursor column now " + newValue);
            //_curOrigin.X = newValue * CellWidth;
            CharUnderCursor = newValue < NumColumns ? GetCharUnderCursor(CursorRow, newValue) : '\0';
        }

        private char GetCharUnderCursor(in int cursorRow, in int cursorCol)
        {
            if (_buffer2.Count <= ViewY + cursorRow) return '\0'; //throw new InvalidOperationException();

            var chars = _buffer2[ViewY + cursorRow];
            if (chars.Count <= ViewX + cursorCol)
                return '\0';
            //throw new InvalidOperationException($"{chars.Count} <= {ViewX} + {cursorCol} ({NumColumns})");
            return chars[ViewX + cursorCol];
        }

        private static void OnCursorRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnCursorRowChanged((int) e.OldValue, (int) e.NewValue);
        }


        protected virtual void OnCursorRowChanged(int oldValue, int newValue)
        {
            //_curOrigin.Y = newValue * CellHeight;
            if (ViewY + NumRows <= newValue)
            {
                ViewY = newValue - NumRows + 1;
            }
            Debug.WriteLine("Row: " + newValue);
            CharUnderCursor = newValue < NumRows ? GetCharUnderCursor(newValue, CursorColumn) : '\0';
        }


        private void OnViewYChanged(int oldValue, int newValue)
        {
            // if (Translate != null)
            // {
                
                // Translate.Y = -1 * DrawingGroup.Bounds.Top + ViewY * CellHeight;
                // Translate.Y = (NumRows * CellHeight - DrawingGroup.Bounds.Height);
                // CalcTransY();
                // TranslateY = Translate.Y;
                
                // _debug("View Y = " + newValue);
                // _debug("Translate y is " + Translate.Y);
            // }

            if (_brush != null)
            {
            _brush.Viewbox = new Rect(  ViewX * CellWidth,  newValue * CellHeight,  NumColumns * CellWidth, NumRows * CellHeight);
            _brush.ViewportUnits = BrushMappingMode.Absolute;
            _brush.Viewport = new Rect(0, 0, NumColumns * CellWidth, (NumRows) * CellHeight);
            _brush.ViewboxUnits = BrushMappingMode.Absolute;
            }
        }

        public int ViewX
        {
            get { return (int) GetValue(ViewXProperty); }
            set { SetValue(ViewXProperty, value); }
        }

        private static void OnViewXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnViewXChanged((int) e.OldValue, (int) e.NewValue);
        }

        public static readonly DependencyProperty TranslateXProperty = DependencyProperty.Register(
            "TranslateX", typeof(double), typeof(WpfTerminalControl),
            new PropertyMetadata(default(double), OnTranslateXChanged));

        public double TranslateX
        {
            get { return (double) GetValue(TranslateXProperty); }
            set { SetValue(TranslateXProperty, value); }
        }

        public static readonly DependencyProperty TranslateYProperty = DependencyProperty.Register(
            "TranslateY", typeof(double), typeof(WpfTerminalControl),
            new PropertyMetadata(default(double), OnTranslateYChanged));

        private Border _bd;
        private int _preDebugCol;
        private int _preDebugRow;
#if ROWDGMODE
        private List<DrawingGroup> _groups = new List<DrawingGroup>();
#endif
        private double _yd2;

        public double TranslateY
        {
            get { return (double) GetValue(TranslateYProperty); }
            set { SetValue(TranslateYProperty, value); }
        }

        private static void OnTranslateYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnTranslateYChanged((double) e.OldValue, (double) e.NewValue);
        }


        protected virtual void OnTranslateYChanged(double oldValue, double newValue)
        {
        }

        private static void OnTranslateXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnTranslateXChanged((double) e.OldValue, (double) e.NewValue);
        }


        protected virtual void OnTranslateXChanged(double oldValue, double newValue)
        {
        }

        private static void VisibleRowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnVisibleRowCountChanged((int) e.OldValue, (int) e.NewValue);
        }

        private void OnVisibleRowCountChanged(int eOldValue, int eNewValue)
        {
            _debug("Visible row count changed to " + eNewValue);
        }

        public int VisibleRowCount
        {
            get { return (int) GetValue(VisibleRowCountProperty); }
            set { SetValue(VisibleRowCountProperty, value); }
        }

        private void OnViewXChanged(int oldValue, int newValue)
        {
            if (DrawingGroup.Bounds.IsEmpty)
            {
                Translate.X = -1 * newValue * CellWidth;
            }
            else
            {
                Translate.X = -1 * newValue * CellWidth + DrawingGroup.Bounds.Left;
            }

            // Translate.X = DrawingGroup.Bounds.Left - (newValue - VisibleRowCount) * xadvance;
            TranslateX = Translate.X;
            ColLabel2Transform.X = Translate.X;

            // if (_brush != null)
            // {
            // _brush.Viewport = new Rect(newValue * CellWidth, ViewY * CellHeight, NumColumns * CellWidth, NumRows * CellHeight);
            // _brush.ViewportUnits = BrushMappingMode.Absolute;
            // _brush.Viewbox = new Rect(0, 0, NumColumns * CellWidth, (ViewY + NumRows) * CellHeight);
            // _brush.ViewboxUnits = BrushMappingMode.Absolute;
            // }

            _debug("View X = " + newValue);
            _debug("Translate X is " + Translate.X);
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            if (e.Handled) return;
            if (InputOnly == true && LineMode)
            {
                try
                {
                    EchoInput(e.Text);
                }
                catch
                {
                }

                e.Handled = true;
                return;
            }

            // if (!_readKeys.HasValue)
            // {
            // SystemSounds.Beep.Play();
            // return;
            // }


            e.Handled = true;
            EchoInput(e.Text);
            return;

            // origin.X = CursorColumn * xadvance;
            // origin.Y = CursorRow * yadvance;

            Debug.WriteLine("enqueuing " + e.Text);
            Input.Enqueue(e.Text);
            // foreach (var Character in e.Text)
            // {
            // Keys.Enqueue(Character);
            // }
            return;
#if false
            _buffer[CursorRow][CursorColumn] = Character;
                Debug.WriteLine($"Seting _buffer[{CursorRow}][{CursorColumn}] to ''{Character}'");
                var x1 =
 new FormattedText(Character.ToString(), Thread.CurrentThread.CurrentUICulture, FlowDirection.LeftToRight, _typeface, 20, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                Debug.WriteLine(x1.Width);
                Debug.WriteLine(x1.Height);
                Debug.WriteLine(x1.LineHeight);


                DrawingGroup xxx = new DrawingGroup();
                var dc = xxx.Open();
                dc.DrawText(x1, origin);
                dc.Close();
                origin.X += xadvance;
                var drawingCollection = ((DrawingGroup)_group.Children[CursorRow + 1]).Children;
                Debug.WriteLine("Line " + CursorRow + " has " + drawingCollection.Count + " children.");
                var old =
                    drawingCollection[CursorColumn];
                Debug.WriteLine(old.Bounds);
                drawingCollection[CursorColumn] = xxx;
                Debug.WriteLine(xxx.Bounds);
                CursorColumn++;
#endif


            Translate.X = DrawingGroup.Bounds.Left;
            Translate.Y = DrawingGroup.Bounds.Top;
            InvalidateVisual();
        }

        protected virtual void EchoInput(string eText)
        {
            foreach (var ch in eText)
            {
                //fixme
                SetCellCharacter(CursorRow, CursorColumn, ch, ConsoleColor.Black, ConsoleColor.White, true, false);
                CursorColumn++;

                if (CursorColumn - ViewX >= NumColumns) ViewX += CursorColumn - ViewX - NumColumns;
            }
        }


        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnLineModeChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        private void OnLineModeChanged(bool eOldValue, bool eNewValue)
        {
            CoerceValue(NumColumnsProperty);
            CoerceValue(NumRowsProperty);
        }

        public bool LineMode
        {
            get { return (bool) GetValue(LineModeProperty); }
            set { SetValue(LineModeProperty, value); }
        }

        public ConcurrentQueue<string> Input { get; set; } = new ConcurrentQueue<string>();
        public event TextEntryCompleteHandler TextEntryComplete;
        public event EventHandler ExecuteCommandComplete;

        /// <inheritdoc />
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (!e.Handled)
                if (DebugMode)
                {
                    switch (e.Key)
                    {
                        case Key.Escape:
                            DebugMode = false;
                            break;
                        case Key.Up:
                            CursorRow--;
                            break;
                        case Key.Down:
                            CursorRow++;
                            break;
                        case Key.Left:
                            CursorColumn--;
                            break;
                        case Key.Right:
                            CursorColumn++;
                            break;
                    }

                    e.Handled = true;
                }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                //if (LineMode && InputOnly)
                e.Handled = true;
                switch (e.Key)
                {
                    case Key.D:
                        if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
                            Debug.WriteLine(GetLineText(CursorRow));
                        else
                            e.Handled = false;
                        break;
                    case Key.Back:
                    {
                        if (CursorColumn > 0)
                        {
                            CursorColumn--;
                            SetCellCharacter(CursorRow, CursorColumn, '\0', ConsoleColor.White, ConsoleColor.Black,
                                false, false);
                        }


                        return;
                    }
                    case Key.Return:
                    {
                        var t = GetLineText(CursorRow);
                        TextEntryComplete?.Invoke(this, new TextEntryCompleteArgs() {Text = t});

                        return;
                    }
                    default:
                        e.Handled = false;
                        break;
                }

                return;
                if (!_readKeys.HasValue)
                    return;
                switch (e.Key)
                {
                    case Key.Left:
                        CursorColumn--;
                        InvalidateVisual();
                        break;
                    case Key.Return:

                        Input.Enqueue("\r");
                        if ((_readKeys.Value & ReadKeyOptions.NoEcho) == 0)
                        {
                            CursorRow++;
                            CursorColumn = 0;
                        }

                        break;

                    default:
                        return;
                }

                e.Handled = true;
            }
        }

        //fixme
        private string GetLineText(int row)
        {
            if (_buffer2.Count <= row)
                return String.Empty;
            var sb = new StringBuilder();
            var chars = _buffer2[row];
            for (var i = NumReservedColumns; i < NumColumns && chars[i] != '\0'; i++)
                if (i < chars.Count)
                {
                    var ch = chars[i];
                    sb.Append(ch);
                }

            return sb.ToString();
        }

        public ConcurrentQueue<char> Keys { get; set; } = new ConcurrentQueue<char>();


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var p = Rect1.TranslatePoint(new Point(0, 0), this);
            var rectangle = new Rect((CursorColumn - ViewX) * xadvance + p.X, p.Y + (CursorRow - ViewY) * yadvance, xadvance,
                yadvance);
            Debug.WriteLine(rectangle);
            drawingContext.DrawRectangle(CursorBrush, null,
                rectangle);
        }

        private int BufferRowCount { get; set; }


        private int TerminalColToBufferCol(in int col)
        {
            return col;
        }

        private int TerminalRowToBufferRow(in int row)
        {
            return row;
        }

        private void CheckCursorForOverrun()
        {
            //viewx
            if (CursorColumn >= NumColumns)
            {
                CursorColumn = NumReservedColumns;
                CursorRow++;
            }

            if (CursorRow >= ViewY + NumRows)
            {
                if (CursorRow == ViewY + NumRows)
                {
                    ViewY++;

#if ROWDGMODE
AddRowsToBuffer(CursorRow + 1);
#else
                    AddRowsToBuffer2(CursorRow + 1);
#endif
                }
                else
                {
                    var viewY = ViewY;
                    var y = CursorRow - NumRows - ViewY + 1;
                    ViewY = y;
#if ROWDGMODE
AddRowsToBuffer(CursorRow + 1);
#else
                    AddRowsToBuffer2(CursorRow + 1);
#endif
                }
            }
        }
#if ROWDGMODE
        private DrawingGroup GetRowDrawingGroup(int row, bool b)
        {
            if (DrawingGroup.Children.Count <= row + 1)
            {
                if (b)
                    while (DrawingGroup.Children.Count <= row + 1)
                        DrawingGroup.Children.Add(new DrawingGroup());
                else
                    throw new InvalidControlState("No drawing groups for rows.");
            }

            var groupChild = DrawingGroup.Children[row + 1];
            if (groupChild is DrawingGroup dg) return dg;
            throw new InvalidControlState($"{groupChild} is not a drawing group");
        }
#endif
        private Point GetCellOrigin(int row, int col)
        {
            var numReservedColumns = col * CellWidth;
            Logger.Info($"{numReservedColumns}");
            return new Point(numReservedColumns, row * CellHeight);
        }

        public char ReadKey(int options)
        {
            ReadKeys.Enqueue(options);
            for (;;)
            {
                if (Input.TryDequeue(out var ch)) return ch[0];

                Thread.Sleep(300);
            }
        }

        public ConcurrentQueue<int> ReadKeys { get; set; } = new ConcurrentQueue<int>();


        public string WindowTitle
        {
            get { return (string) GetValue(WindowTitleProperty); }
            set { SetValue(WindowTitleProperty, value); }
        }

        private static void OnWindowTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnWindowTitleChanged((string) e.OldValue, (string) e.NewValue);
        }


        protected virtual void OnWindowTitleChanged(string oldValue, string newValue)
        {
        }

        public void WriteLine()
        {
            CursorColumn = 0;
            CursorRow++;
        }


        public event ProgressEventHandler ProgressEvent;

        public void WriteProgress(in long sourceId, MyRecord myRecord)
        {
            ProgressEvent?.Invoke(this, new ProgressEventArgs() {SourceId = sourceId, Record = myRecord});
        }

        public Cred PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return new Cred();
        }

        public void WriteInformation(MyInformationRecord myInformationRecord)
        {
            InfoRecords.Add(myInformationRecord);
        }

        public ObservableCollection<MyInformationRecord> InfoRecords { get; set; } =
            new ObservableCollection<MyInformationRecord>();

        public IDictionary Prompt(string caption, string message, IEnumerable<MyFieldDescription> myFieldDescriptions)
        {
            var returnv = new Dictionary<string, object>();
            var w = new Window();
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Auto});
            g.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Auto});
            g.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
            var rowIndex = 0;
            var textBlock = new TextBlock {Text = message, TextWrapping = TextWrapping.WrapWithOverflow};
            g.Children.Add(textBlock);
            Grid.SetRow(textBlock, rowIndex);
            Grid.SetColumnSpan(textBlock, 2);
            rowIndex++;
            foreach (var myFieldDescription in myFieldDescriptions)
            {
                var label = new TextBlock {Text = myFieldDescription.Label};
                var inputBox = new TextBox() {Width = 200};
                inputBox.TextChanged += (sender, args) => { returnv[myFieldDescription.Name] = inputBox.Text; };
                g.Children.Add(label);
                g.Children.Add(inputBox);
                Grid.SetColumn(label, 0);
                Grid.SetRow(label, rowIndex);
                Grid.SetColumn(inputBox, 1);
                Grid.SetRow(inputBox, rowIndex);
                g.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
                rowIndex++;
            }

            w.MinWidth = 400;
            w.Height = 400;
            w.Content = g;
            w.Title = caption;
            var uiElement = new WrapPanel();
            var element = new Button {Content = "Ok"};
            element.Click += (sender, args) => w.Close();
            uiElement.Children.Add(element);
            g.Children.Add(uiElement);
            g.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Auto});
            Grid.SetRow(uiElement, rowIndex);
            w.ShowDialog();
            return returnv;
        }

        public CellCharacterInfo GetCellCharacter(int row, int column)
        {
            return new CellCharacterInfo {Character = _buffer2[row][column]};
        }

        public void Write(in char value)
        {
            SetCellCharacter(CursorRow, CursorColumn, value, ForegroundColor, BackgroundColor);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void DumpBuffercontents()
        {
            foreach (var l in _buffer2) _debug(string.Join("", l));
        }

        public void EnterDebugMode()
        {
            DebugMode = true;
            _preDebugRow = CursorRow;
            _preDebugCol = CursorColumn;
        }

        private bool DebugMode { get; set; }

        public void ExitDebugMode()
        {
            DebugMode = false;
            CursorRow = _preDebugRow;
            CursorColumn = _preDebugCol;
        }

        public void WriteErrorLine(string value)
        {
            foreach (var ch in value) Write(ch);

            WriteLine();
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
            VScrollBar.Value += e.Delta / 5;
            //ViewY += e.Delta;
        }

        public static readonly DependencyProperty DiagnosticsEnabledProperty = DependencyProperty.Register(
            "DiagnosticsEnabled", typeof(bool), typeof(WpfTerminalControl),
            new PropertyMetadata(default(bool), OnDiagnosticsEnabledChanged));

        private Rectangle _minirect;
        private DrawingBrush _minibrush;
        private Popup _popup;
        private DrawingBrush _zoombrush;
        private DrawingContext _drawingContext;

        // More recent addition for optimization
        private Point _curOrigin;
        private double _cellHeight;
        private double _cellWidth;
        private TranslateTransform _translate;
        private Rectangle _rect2;
        private DrawingGroup _dg2;
        private DrawingBrush _brush2;
        private DrawingGroup _dg0;
        private Rectangle _colLabel;
        private DrawingGroup _drawingGroup;
        private Typeface _typeface;

        public bool DiagnosticsEnabled
        {
            get { return (bool) GetValue(DiagnosticsEnabledProperty); }
            set { SetValue(DiagnosticsEnabledProperty, value); }
        }

        private ScrollBar VScrollBar
        {
            get { return _vScrollBar; }
            set
            {
                if (Equals(value, _vScrollBar)) return;
                _vScrollBar = value;
                if (_vScrollBar != null)
                    _vScrollBar.SetBinding(RangeBase.ValueProperty,
                        new Binding(nameof(ViewY)) {Source = this, Mode = BindingMode.TwoWay});
                OnPropertyChanged();
            }
        }

        private Border Border
        {
            get { return _bd; }
            set
            {
                if (Equals(value, _bd)) return;
                _bd = value;
                OnPropertyChanged();
            }
        }

        private Rectangle MiniRect
        {
            get { return _minirect; }
            set
            {
                if (Equals(value, _minirect)) return;
                if (_minirect != null)
                {
                    _minirect.MouseEnter -= MinirectOnMouseEnter;
                    _minirect.MouseLeave -= MiniRectOnMouseLeave;
                    _minirect.PreviewMouseMove -= MiniRectOnPreviewMouseMove;
                }

                _minirect = value;
                if (_minirect != null)
                {
                    _minirect.MouseEnter += MinirectOnMouseEnter;
                    _minirect.MouseLeave += MiniRectOnMouseLeave;
                    _minirect.PreviewMouseMove += MiniRectOnPreviewMouseMove;
                }

                OnPropertyChanged();
            }
        }

        private DrawingBrush Brush2
        {
            get { return _brush2; }
            set
            {
                if (Equals(value, _brush2)) return;
                _brush2 = value;
                OnPropertyChanged();
            }
        }

        private DrawingBrush Brush1
        {
            get { return _brush; }
            set { _brush = value; }
        }

        private DrawingBrush Zoombrush
        {
            get { return _zoombrush; }
            set { _zoombrush = value; }
        }

        private Popup Popup1
        {
            get { return _popup; }
            set { _popup = value; }
        }

        private DrawingBrush Minibrush
        {
            get { return _minibrush; }
            set { _minibrush = value; }
        }

        private Rectangle Rect1
        {
            get { return _rect; }
            set
            {
                _rect = value;
                SetBinding(RectWidthProperty, new Binding("ActualWidth") {Source = _rect});
                SetBinding(RectHeightProperty, new Binding("ActualHeight") {Source = _rect});
            }
        }

        public Typeface Typeface
        {
            get { return _typeface; }
            set { _typeface = value; }
        }

        private static void OnDiagnosticsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfTerminalControl) d).OnDiagnosticsEnabledChanged((bool) e.OldValue, (bool) e.NewValue);
        }


        protected virtual void OnDiagnosticsEnabledChanged(bool oldValue, bool newValue)
        {
        }

        public void WriteLine(string s)
        {
            Write(s, true);
        }

        public void WriteDebugLine(string message)
        {
            WriteLine(message);
        }

        public void WriteVerboseLine(string message)
        {
            WriteLine(message);
        }

        public void WriteWarningLine(string message)
        {
            WriteLine(message);
        }

        public void Write(string s, bool doWriteNewLine = false)
        {
            var newCursorColumn = CursorColumn;
            var newCursorRow = CursorRow;
            var fNewRow = false;

            if (!String.IsNullOrEmpty(s))
            {
                var foreground = colors[(int) ForegroundColor];

                while (newCursorColumn + s.Length > NumColumns)
                {
                    var length = NumColumns - newCursorColumn;
                    var s1 = s.Substring(0, length);
                    var text1 = new FormattedText(s1, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                        Typeface,
                        (double) GetValue(FontSizeProperty), foreground,
                        _pixelsPerDip);

                    if (_drawingContext == null)
                    {
                        _drawingContext = DrawingGroup.Append();
                    }

                        _drawingContext.DrawText(text1, _curOrigin);
                    
                    newCursorColumn = NumReservedColumns;
                    _curOrigin.X = newCursorColumn * CellWidth;
                    _curOrigin.Y += CellHeight;
                    s = s.Substring(length);
                    fNewRow = true;
                    newCursorRow++;
                    if (ViewY + CursorRow == NumRows) ViewY++;
                }

                var text = new FormattedText(s, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface,
                    (double) GetValue(FontSizeProperty), foreground,
                    _pixelsPerDip);

                if (_drawingContext == null)
                {
                    _drawingContext = DrawingGroup.Append();
                }

                    _drawingContext.DrawText(text, _curOrigin);
            }

            if (_drawingContext != null) _drawingContext.Close();
            _drawingContext = null;
            if (doWriteNewLine)
            {
                _curOrigin.Y += CellHeight;
                fNewRow = true;
                newCursorRow++;
            }

            if (fNewRow)
            {
                CursorRow = newCursorRow;
                newCursorColumn = NumReservedColumns;
                _curOrigin.X = newCursorColumn * CellWidth;
                CursorColumn = newCursorColumn;
            }


            Brush1.Drawing = DrawingGroup;
            InvalidateVisual();
            var message =
                $"( {DrawingGroup.Bounds.X:N2}, {DrawingGroup.Bounds.Y:N2} ) - ({DrawingGroup.Bounds.Right:N2}, {DrawingGroup.Bounds.Bottom:N2} )";
            Debug.WriteLine(message);
            // if (newCursorRow == ViewY + NumRows)
            // {
                // ViewY += 1;
            // }

            // if (Translate != null)
            // {
                // Translate.X = DrawingGroup.Bounds.Left;
                // 0 until end
             // CalcTransY();
                // Translate.Y = DrawingGroup.Bounds.Bottom;
                // TranslateX = Translate.X;
                // TranslateY = Translate.Y;
            // }

            //_drawingContext = DrawingGroup.Append();
        }
    }

    public delegate void ProgressEventHandler(object sender, ProgressEventArgs args);

    public delegate void TextEntryCompleteHandler(object sender, TextEntryCompleteArgs e);
}