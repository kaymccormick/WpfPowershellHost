using System;

namespace Terminal1
{
    [Flags]
    public enum ReadKeyOptions
    {
        /// <summary>
        /// 
        /// Allow Ctrl-C to be processed as a keystroke, as opposed to causing a break event.
        /// 
        /// </summary>
        AllowCtrlC = 1,

        /// <summary>
        /// 
        /// Include key down events.  Either one of IncludeKeyDown and IncludeKeyUp or both must be specified.
        /// 
        /// </summary>
        IncludeKeyDown = 4,

        /// <summary>
        /// 
        /// Include key up events.  Either one of IncludeKeyDown and IncludeKeyUp or both must be specified.
        /// 
        /// </summary>
        IncludeKeyUp = 8,

        /// <summary>
        /// 
        /// Do not display the character for the key in the window when pressed.
        /// 
        /// </summary>
        NoEcho = 2
    }
}