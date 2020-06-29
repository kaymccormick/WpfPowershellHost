using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using NLog;
using Terminal1;
using WpfCustomControlLibrary1;
using WpfTerminalControlLib;
using ReadKeyOptions = System.Management.Automation.Host.ReadKeyOptions;
using Size = System.Management.Automation.Host.Size;

namespace PowerShellShared
{
    internal class RawUI : PSHostRawUserInterface
    {
#if DEBUGz
        protected static Logger Logger = LogManager.GetCurrentClassLogger();
#else
        protected static Logger Logger = LogManager.CreateNullLogger();
#endif

        private ConsoleBuffer buf;
        private readonly Size _maxWindowSize = new Size(0, 0);
        private readonly Size _maxPhysicalWindowSize = new Size(0, 0);


        private string _windowTitle;

        private Coordinates _windowPosition;
        private ConsoleColor _foregroundColor;
        private int _cursorSize = DefaultCursorSize;
        public const int DefaultCursorSize = 100;
        private ConsoleColor _backgroundColor;
        private float pixelsPerDip = 1;

        private int? numRows;
        private int? numCols;
        private int? _reportedRows;
        private int? _reportedCols;

        public RawUI(TerminalInterface terminalInterface)
        {
            
            TerminalCharacteristics.AddNumColumnsChangedEventHandler(terminalInterface,
                TerminalInterfaceOnNumColumnsChanged);
            TerminalCharacteristics.AddNumRowsChangedEventHandler(terminalInterface, TerminalInterfaceOnNumRowsChanged);
            TermInterface = terminalInterface;
            var rows = TerminalCharacteristics.GetNumRows(terminalInterface);
            if (rows != -1)
            {
                numRows = rows;
            }
            var cols= TerminalCharacteristics.GetNumColumns(terminalInterface);
            if (cols!= -1)
            {
                numCols= cols;
            }


            if(numCols.HasValue && numRows.HasValue)
            buf = new ConsoleBuffer {Buf = NewBufferCellArray(new Size(numCols.Value, numRows.Value), new BufferCell())};
        }

        private void TerminalInterfaceOnNumColumnsChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            if (numRows.HasValue && e.NewValue != -1 && numRows.Value >= 0)
                buf = new ConsoleBuffer
                    {Buf = NewBufferCellArray(new Size(e.NewValue, numRows.Value), new BufferCell())};
            numCols = e.NewValue;
        }

        private void TerminalInterfaceOnNumRowsChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            if (numCols.HasValue && e.NewValue < 0 && numCols.Value >=0)
                buf = new ConsoleBuffer
                    {Buf = NewBufferCellArray(new Size(numCols.Value, e.NewValue), new BufferCell())};
            numRows = e.NewValue;
        }

        public WpfTerminalControl Control { get; set; }

        /// <summary>
        /// Reads a key stroke from the keyboard device, blocking until a keystroke is typed.
        /// Either one of ReadKeyOptions.IncludeKeyDown and ReadKeyOptions.IncludeKeyUp or both must be specified.
        /// </summary>
        /// <param name="options">
        /// A bit mask of the options to be used to read the keyboard. Constants defined by
        /// <see cref="T:System.Management.Automation.Host.ReadKeyOptions" />
        /// </param>
        /// <returns>
        /// Key stroke depending on the value of <paramref name="options" />.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// Neither ReadKeyOptions.IncludeKeyDown nor ReadKeyOptions.IncludeKeyUp is specified.
        /// </exception>
        /// <example>
        ///     <MSH>
        ///         $option = [System.Management.Automation.Host.ReadKeyOptions]"IncludeKeyDown";
        ///         $host.UI.RawUI.ReadKey($option)
        ///     </MSH>
        /// </example>
        /// <seealso cref="T:System.Management.Automation.Host.ReadKeyOptions" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.ReadKey" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.ReadKey(System.Management.Automation.Host.ReadKeyOptions)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.FlushInputBuffer" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.KeyAvailable" />
        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            var ch = TermInterface.ReadKey((int) options);
            return new KeyInfo() {Character = ch, KeyDown = true};
        }

        /// <summary>
        /// Resets the keyboard input buffer.
        /// </summary>
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.ReadKey" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.ReadKey(System.Management.Automation.Host.ReadKeyOptions)" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.KeyAvailable" />
        public override void FlushInputBuffer()
        {
        }

        /// <summary>
        /// Copies the <see cref="T:System.Manageent.Automation.Host.BufferCell" /> array into the screen buffer at the
        /// given origin, clipping such that cells in the array that would fall outside the screen buffer are ignored.
        /// </summary>
        /// <param name="origin">
        /// The top left corner of the rectangular screen area to which <paramref name="contents" /> is copied.
        /// </param>
        /// <param name="contents">
        /// A rectangle of <see cref="T:System.Management.Automation.Host.BufferCell" /> objects to be copied to the
        /// screen buffer.
        /// </param>
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.Int32,System.Int32,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.Management.Automation.Host.Size,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.String[],System.ConsoleColor,System.ConsoleColor)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(System.Char)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(System.String)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.GetBufferContents(System.Management.Automation.Host.Rectangle)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.ScrollBufferContents(System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.Coordinates,System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.BufferCell)" />
        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            Trace.WriteLine($"set buffer {contents.Length}");
            Logger.Debug($"SetBufferContents {origin} ({contents.Length})");
            var length = contents.GetLength(1);
            var g = new ushort[length];
            var advanceWidths = new double[length];
            Debug.WriteLine(nameof(SetBufferContents));
            var k = 0;
            
            
            var l = contents.GetLength(0);
            try
            {
                for (var i = origin.Y; k < l; i++, k++)
                {
                    var z = 0;
                    var bound = contents.GetLength(1);
                    for (var j = origin.X; z < bound; j++, z++)
                        try
                        {
                            var bufferCell = contents[k, z];
                            //buf.Buf[i, j] = bufferCell;

                            var bufferCellCharacter = bufferCell.Character;
                            var bufferCellBackgroundColor = bufferCell.BackgroundColor;
                            var bufferCellForegroundColor = bufferCell.ForegroundColor;
                            TermInterface.Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    var xx = bound;
                                    TermInterface.SetCellCharacter(i, j, bufferCellCharacter, bufferCellForegroundColor,
                                        bufferCellBackgroundColor, false, true);
                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }


        /// <summary>
        /// Copies a given character to all of the character cells in the screen buffer with the indicated colors.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle on the screen buffer to which <paramref name="fill" /> is copied.
        /// If all elements are -1, the entire screen buffer will be copied with <paramref name="fill" />.
        /// </param>
        /// <param name="fill">
        /// The character and attributes used to fill <paramref name="rectangle" />.
        /// </param>
        /// <remarks>
        /// Provided for clearing regions -- less chatty than passing an array of cells.
        /// </remarks>
        /// <example>
        ///     <snippet Code="C#">
        ///         using System;
        ///         using System.Management.Automation;
        ///         using System.Management.Automation.Host;
        ///         namespace Microsoft.Samples.MSH.Cmdlet
        ///         {
        ///             [Cmdlet("Clear","Screen")]
        ///             public class ClearScreen : PSCmdlet
        ///             {
        ///                 protected override void BeginProcessing()
        ///                 {
        ///                     Host.UI.RawUI.SetBufferContents(new Rectangle(-1, -1, -1, -1),
        ///                         new BufferCell(' ', Host.UI.RawUI.ForegroundColor, Host.UI.RawUI.BackgroundColor))
        ///                 }
        ///             }
        ///         }
        ///     </snippet>
        /// </example>
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.Int32,System.Int32,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.Management.Automation.Host.Size,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.String[],System.ConsoleColor,System.ConsoleColor)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(System.Char)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(System.String)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(System.Management.Automation.Host.Coordinates,System.Management.Automation.Host.BufferCell[0:,0:])" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.GetBufferContents(System.Management.Automation.Host.Rectangle)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.ScrollBufferContents(System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.Coordinates,System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.BufferCell)" />
        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            
            for (var j = rectangle.Top; j < rectangle.Bottom; j++)
            for (var i = rectangle.Left; i < rectangle.Right; i++)
            {
                buf.Buf[j, i] = fill;
                TermInterface.Dispatcher.Invoke(() =>
                    TermInterface.SetCellCharacter(j, i, fill.Character, ConsoleColor.White, ConsoleColor.Black, false,
                        true));
            }
        }

        /// <summary>
        /// Extracts a rectangular region of the screen buffer.
        /// </summary>
        /// <param name="rectangle">
        /// The rectangle on the screen buffer to extract.
        /// </param>
        /// <returns>
        /// An array of <see cref="T:System.Management.Automation.Host.BufferCell" /> objects extracted from
        /// the rectangular region of the screen buffer specified by <paramref name="rectangle" />
        /// </returns>
        /// <remarks>
        /// If the rectangle is completely outside of the screen buffer, a BufferCell array of zero rows and column will be
        /// returned.
        /// If the rectangle is partially outside of the screen buffer, the area where the screen buffer and rectangle overlap
        /// will be read and returned. The size of the returned array is the same as that of r. Each BufferCell in the
        /// non-overlapping area of this array is set as follows:
        /// Character is the space (' ')
        /// ForegroundColor to the current foreground color, given by the ForegroundColor property of this class.
        /// BackgroundColor to the current background color, given by the BackgroundColor property of this class.
        /// The resulting array is organized in row-major order for performance reasons.  The screen buffer, however, is
        /// organized in column-major order -- e.g. you specify the column index first, then the row index second, as in (x, y).
        /// This means that a cell at screen buffer position (x, y) is in the array element [y, x].
        /// </remarks>
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.Int32,System.Int32,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.Management.Automation.Host.Size,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.String[],System.ConsoleColor,System.ConsoleColor)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(System.Char)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(System.String)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(System.Management.Automation.Host.Coordinates,System.Management.Automation.Host.BufferCell[0:,0:])" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.ScrollBufferContents(System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.Coordinates,System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.BufferCell)" />
        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            Debug.WriteLine("get buffer");
            var bufferContents = new BufferCell[rectangle.Right = rectangle.Left
                , rectangle.Bottom - rectangle.Top];
            var y = 0;
            for (var j = rectangle.Top; j < rectangle.Bottom; j++, y++)
            {
                var x = 0;
                for (var i = rectangle.Left; i < rectangle.Right; i++, x++) bufferContents[y, x] = buf.Buf[j, i];
            }

            return bufferContents;
        }

        /// <summary>
        /// Scroll a region of the screen buffer.
        /// </summary>
        /// <param name="source">
        /// Indicates the region of the screen to be scrolled.
        /// </param>
        /// <param name="destination">
        /// Indicates the upper left coordinates of the region of the screen to receive the source region contents.  The target
        /// region is the same size as the source region.
        /// </param>
        /// <param name="clip">
        /// Indicates the region of the screen to include in the operation.  If a cell would be changed by the operation but
        /// does not fall within the clip region, it will be unchanged.
        /// </param>
        /// <param name="fill">
        /// The character and attributes to be used to fill any cells within the intersection of the source rectangle and
        /// clipping rectangle that are left "empty" by the move.
        /// </param>
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.Int32,System.Int32,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.Management.Automation.Host.Size,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(System.String[],System.ConsoleColor,System.ConsoleColor)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(System.Char)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(System.String)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(System.Management.Automation.Host.Coordinates,System.Management.Automation.Host.BufferCell[0:,0:])" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.GetBufferContents(System.Management.Automation.Host.Rectangle)" />
        public override void ScrollBufferContents(
            Rectangle source
            , Coordinates destination
            , Rectangle clip
            , BufferCell fill
        )
        {
            Debug.WriteLine("scroll");
        }

        /// <summary>
        /// Gets or sets the color used to render the background behind characters on the screen buffer.  Each character cell in
        /// the screen buffer can have a separate background color.
        /// </summary>
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.ForegroundColor" />
        public override ConsoleColor BackgroundColor
        {
            get { return TermInterface.Dispatcher.Invoke(() => TermInterface.BackgroundColor); }
            set { TermInterface.Dispatcher.Invoke(() => TermInterface.BackgroundColor = value); }
        }

        /// <summary>
        /// Gets or sets the current size of the screen buffer, measured in character cells.
        /// </summary>
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxPhysicalWindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.WindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.CursorPosition" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxWindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.WindowPosition" />
        public override Size BufferSize
        {
            get
            {
                _reportedCols = numCols.GetValueOrDefault();
                _reportedRows = numRows.GetValueOrDefault();
                return new Size(numCols.GetValueOrDefault(), numRows.GetValueOrDefault());
                //return TermInterface.Dispatcher.Invoke(() => { return new Size(TermInterface.NumColumns, TermInterface.NumRows); }); }
            }
            set
            {
                throw new NotSupportedException();
                TermInterface.Dispatcher.Invoke(() =>
                {
                    TermInterface.NumColumns = value.Width;
                    TermInterface.NumRows = value.Height;
                });
            }
        }

        /// <summary>
        /// Gets or sets the cursor position in the screen buffer.  The view window always adjusts it's location over the screen
        /// buffer such that the cursor is always visible.
        /// </summary>
        /// <remarks>
        /// To write to the screen buffer without updating the cursor position, use
        /// <see cref="M:System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.BufferCell)" /> or
        /// <see cref="M:System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(System.Management.Automation.Host.Coordinates,System.Management.Automation.Host.BufferCell[0:,0:])" />
        /// </remarks>
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxPhysicalWindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.WindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.WindowPosition" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxWindowSize" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(System.Management.Automation.Host.Rectangle,System.Management.Automation.Host.BufferCell)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(System.Management.Automation.Host.Coordinates,System.Management.Automation.Host.BufferCell[0:,0:])" />
        public override Coordinates CursorPosition
        {
            get
            {
                return TermInterface.Dispatcher.Invoke(() =>
                {
                    return new Coordinates(TermInterface.CursorColumn, TermInterface.CursorRow);
                });
            }
            set
            {
                TermInterface.Dispatcher.Invoke(() =>
                {
                    TermInterface.CursorColumn = value.X;
                    TermInterface.CursorRow = value.Y;
                });
            }
        }

        /// <summary>
        /// Gets or sets the cursor size as a percentage 0..100.
        /// </summary>
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.CursorPosition" />
        public override int CursorSize
        {
            get { return TermInterface.Dispatcher.Invoke(() => TermInterface.CursorSize); }
            set { TermInterface.Dispatcher.Invoke(() => TermInterface.CursorSize = value); }
        }

        /// <summary>
        /// Gets or sets the color used to render characters on the screen buffer. Each character cell in the screen buffer can
        /// have a separate foreground color.
        /// </summary>
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.BackgroundColor" />
        public override ConsoleColor ForegroundColor
        {
            get { return TermInterface.Dispatcher.Invoke(() => TermInterface.ForegroundColor); }
            set { TermInterface.Dispatcher.Invoke(() => TermInterface.ForegroundColor = value); }
        }

        /// <summary>
        /// A non-blocking call to examine if a keystroke is waiting in the input buffer.
        /// </summary>
        /// <value>
        /// True if a keystroke is waiting in the input buffer, false if not.
        /// </value>
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.ReadKey" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.ReadKey(System.Management.Automation.Host.ReadKeyOptions)" />
        /// <seealso cref="M:System.Management.Automation.Host.PSHostRawUserInterface.FlushInputBuffer" />
        public override bool KeyAvailable
        {
            get { return false; } // !TermInterface.Keys.IsEmpty;
        }

        /// <summary>
        /// Gets the largest window possible for the current font and display hardware, ignoring the current buffer dimensions.  In
        /// other words, the dimensions of the largest window that could be rendered in the current display, if the buffer was
        /// at least as large.
        /// </summary>
        /// <remarks>
        /// To resize the window to this dimension, use <see cref="P:System.Management.Automation.Host.PSHostRawUserInterface.BufferSize" />
        /// to first check and, if necessary, adjust, the screen buffer size.
        /// </remarks>
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxWindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.BufferSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.CursorPosition" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.WindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.WindowPosition" />
        public override Size MaxPhysicalWindowSize
        {
            get { return _maxPhysicalWindowSize; }
        }

        /// <summary>
        /// Gets the size of the largest window possible for the current buffer, current font, and current display hardware.
        /// The view window cannot be larger than the screen buffer or the current display (the display the window is rendered on).
        /// </summary>
        /// <value>
        /// The largest dimensions the window can be resized to without resizing the screen buffer.
        /// </value>
        /// <remarks>
        /// Always returns a value less than or equal to
        /// <see cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxPhysicalWindowSize" />.
        /// </remarks>
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxPhysicalWindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.BufferSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.CursorPosition" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.WindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.WindowPosition" />
        public override Size MaxWindowSize
        {
            get { return _maxWindowSize; }
        }

        /// <summary>
        /// Gets or sets position of the view window relative to the screen buffer, in characters. (0,0) is the upper left of the screen
        /// buffer.
        /// </summary>
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxPhysicalWindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.WindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.CursorPosition" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxWindowSize" />
        public override Coordinates WindowPosition
        {
            get
            {
                return TermInterface.Dispatcher.Invoke(() =>
                {
                    return new Coordinates(TermInterface.ViewX, TermInterface.ViewY);
                });
            }
            set
            {
                Debug.WriteLine("Setting window position");
                TermInterface.Dispatcher.Invoke(() =>
                {
                    TermInterface.ViewX = value.X;
                    TermInterface.ViewY = value.Y;
                });
            }
        }

        /// <summary>
        /// Gets or sets the current view window size, measured in character cells.  The window size cannot be larger than the
        /// dimensions returned by <see cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxPhysicalWindowSize" />.
        /// </summary>
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxPhysicalWindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.BufferSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.CursorPosition" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.MaxWindowSize" />
        /// <seealso cref="P:System.Management.Automation.Host.PSHostRawUserInterface.WindowPosition" />
        public override Size WindowSize
        {
            get
            {
                return new Size(numCols.GetValueOrDefault(), numRows.GetValueOrDefault());
                return TermInterface.Dispatcher.Invoke(() =>
                {
                    return new Size(TermInterface.NumColumns, TermInterface.NumRows);
                });
            }
            set
            {
                throw new NotSupportedException();
                TermInterface.Dispatcher.Invoke(() =>
                {
                    TermInterface.NumColumns = value.Width;
                    TermInterface.NumRows = value.Height;
                });
            }
        }

        /// <summary>
        /// Gets or sets the titlebar text of the current view window.
        /// </summary>
        public override string WindowTitle
        {
            get { return TermInterface.Dispatcher.Invoke(() => { return TermInterface.WindowTitle; }); }
            set { TermInterface.Dispatcher.Invoke(() => { TermInterface.WindowTitle = value; }); }
        }

        public ITerminalInterface TermInterface { get; set; }

        public void WriteLine()
        {
            TermInterface.Dispatcher.Invoke(() => TermInterface.WriteLine());
        }
    }

    internal struct ConsoleBuffer
    {
        public BufferCell[,] Buf;
    }
}