using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using NLog;
using PowerShellShared;
using Terminal1;

namespace WpfTerminalControlLib
{
    [ DefaultProperty ( "Shell" ) ]
    //[ TypeDescriptionProvider ( typeof ( WrappedPowerShellProvider ) ) ]
    // [TypeConverter(typeof(WrappedPowerShellTypeConverter))]
    // [ValueSerializer(typeof(WrappedSerializer))]
    [ContentProperty("Terminal")]
    public class WrappedPowerShell : ContentControl
    {

        private static Logger Logger = LogManager.GetCurrentClassLogger();
        public string CallerFilePath { get ; }

        public int CallerLineNumber { get ; }

        /// <summary>Signals the object that initialization is starting.</summary>

        [ Category ( "PowerShell" ) ]
        public static readonly DependencyProperty ShellProperty ;

        public static readonly RoutedEvent ShellChangedEvent ;
        private IAsyncResult _r1;


        public WpfTerminalControl Terminal { get; set; }
        static WrappedPowerShell ( )
        {
            ShellChangedEvent = EventManager.RegisterRoutedEvent (
                                                                  "ShellChanged"
                                                                , RoutingStrategy.Bubble
                                                                , typeof (
                                                                      RoutedPropertyChangedEventHandler
                                                                      < PowerShell > )
                                                                , typeof ( WrappedPowerShell )
                                                                 ) ;

            ShellProperty = DependencyProperty.Register (
                                                         "Shell"
                                                       , typeof ( PowerShell )
                                                       , typeof ( WrappedPowerShell )
                                                       , new FrameworkPropertyMetadata (
                                                                                        null
                                                                                      , FrameworkPropertyMetadataOptions
                                                                                           .None
                                                                                      , new
                                                                                            PropertyChangedCallback (
                                                                                                                     OnShellChanged
                                                                                                                    )
                                                                                      , new
                                                                                            CoerceValueCallback (
                                                                                                                 CoerceShellValue
                                                                                                                )
                                                                                      , true
                                                                                      , UpdateSourceTrigger
                                                                                           .PropertyChanged
                                                                                       )
                                                        ) ;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Control" /> class. </summary>

        
        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Control" /> class. </summary>
        /// <param name="drawing"></param>
        public WrappedPowerShell ( )
        {
            
            CreateDateTime = DateTime.Now ;
        }

        public event RoutedEventHandler ShellChanged
        {
            add => AddHandler ( ShellChangedEvent , value ) ;
            remove => RemoveHandler ( ShellChangedEvent , value ) ;
        }

        private static object CoerceShellValue ( DependencyObject d , object basevalue )
        {
            return basevalue ;
        }

        private static void OnShellChanged (
            DependencyObject                   d
          , DependencyPropertyChangedEventArgs e
        )
        {
            var ev = new RoutedPropertyChangedEventArgs < PowerShell > (
                                                                        ( PowerShell ) e.OldValue
                                                                      , ( PowerShell ) e.NewValue
                                                                      , ShellChangedEvent
                                                                       ) ;
            var owner = ( WrappedPowerShell ) d ;
            owner.RaiseEvent ( ev ) ;
        }

        public DateTime CreateDateTime { get ; set ; }
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public Host Host { get ; set ; }

        public FlowDocument OutputDocument { get ; set ; }

        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public Runspace Runspace { get ; set ; }

        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public PowerShell Shell
        {
            get => ( PowerShell ) GetValue ( ShellProperty ) ;
            set => SetValue ( ShellProperty , ( PowerShell ) value ) ;
        }


        public override void EndInit ( )
        {
            Host           = new Host (Terminal) ;
            Terminal.TextEntryComplete +=TerminalOnTextEntryComplete;
            InitialSessionState iss = InitialSessionState.CreateDefault();
            SessionStateVariableEntry var1 = new
                SessionStateVariableEntry("test1",
                    "MyVar1",
                    "Initial session state MyVar1 test");
            iss.Variables.Add(var1);

            SessionStateVariableEntry var2 = new
                SessionStateVariableEntry("test2",
                    "MyVar2",
                    "Initial session state MyVar2 test");
            iss.Variables.Add(var2);

            Runspace = RunspaceFactory.CreateRunspace ( Host ,iss) ;
            
            foreach (var sessionStateCommandEntry in iss.Commands)
            {
                if(sessionStateCommandEntry.CommandType == CommandTypes.Cmdlet)
                Debug.WriteLine(sessionStateCommandEntry.Name);
                
            }
            Runspace.Open();
            Host.Runspace = Runspace ;
            
            //Host.DebuggerEnabled = true ;
            if ( Shell == null )
            {
                Debug.WriteLine("creating powershell from wrappedpowershell");
                Shell = PowerShell.Create ( ) ;
                
                Shell.Runspace = Runspace;
            }
        }

        private void TerminalOnTextEntryComplete(object sender, TextEntryCompleteArgs e)
        {
            Host.HostUI.WriteLine("");
            Debug.WriteLine(Execute(e.Text));
            Debug.WriteLine("Back from execute");

        }


        /// <summary>
        /// Basic script execution routine - any runtime exceptions are
        /// caught and passed back into the engine to display.
        /// </summary>
        /// <param name="cmd">The parameter is not used.</param>
        public async Task Execute ( string cmd )
        {
            try
            {
                // execute the command with no input...
                await executeHelper ( cmd , null ) ;
            }
            catch ( RuntimeException rte )
            {
                ReportException ( rte ) ;
            }
        }

        /// <summary>
        /// A helper class that builds and executes a pipeline that writes to the
        /// default output path. Any exceptions that are thrown are just passed to
        /// the caller. Since all output goes to the default outter, this method()
        /// won't return anything.
        /// </summary>
        /// <param name="cmd">The script to run</param>
        /// <param name="input">Any input arguments to pass to the script. If null
        /// then nothing is passed in.</param>
        private async Task executeHelper ( string cmd , object input )
        {
            // Just ignore empty command lines...
            if ( string.IsNullOrEmpty ( cmd ) )
            {
                return ;
            }


            // Create a pipeline for this execution - place the result in the currentPowerShell
            // instance variable so it is available to be stopped.
            try
            {
                if (Shell != null)
                {
                    Shell.Commands.Clear();

                    Shell.AddScript(cmd);
                    Logger.Info("Coommand is : " + cmd);
  
                    //Host.UI.WriteLine(cmd);
                    // Now add the default outputter to the end of the pipe and indicate
                    // that it should handle both output and errors from the previous
                    // commands. This will result in the output being written using the PSHost
                    // and PSHostUserInterface classes instead of returning objects to the hosting
                    // application.
                    Shell.AddCommand("out-default");
                    Shell.Commands.Commands[0]
                         .MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                    // If there was any input specified, pass it in, otherwise just
                    // execute the pipeline.
#if NET472
                                        Collection<PSObject> results = null;
                    if (input != null)
                    {
                        results = Shell.Invoke(new object[] { input });
                    }
                    else
                    {
                        _r1 = Shell.BeginInvoke<object>(null, new PSInvocationSettings(){}, Callback, Shell);
                    }

#else
                    Collection<PSObject> results = null;
                    if (input != null)
                    {
                        results = Shell.Invoke(new object[] {input});
                    }
                    else
                    {
                        
                        
                        var r2 = await Shell.InvokeAsync();
                        if (r2 != null)
                            foreach (var psObject in r2)
                            {
                                Debug.Write(psObject);
                            }
                    }
#endif
                }
            }catch
                
            {}
            finally
            {
            }
        }

        private void Callback(IAsyncResult ar)
        {
            var shell = (PowerShell) ar.AsyncState;
            var r2 = shell.EndInvoke(ar);

            foreach (var psObject in r2)
            {
                Debug.Write(psObject);
            }
        }

        /// <summary>
        /// An exception occurred that we want to display
        /// using the display formatter. To do this we run
        /// a second pipeline passing in the error record.
        /// The runtime will bind this to the $input variable
        /// which is why $input is being piped to out-string.
        /// We then call WriteErrorLine to make sure the error
        /// gets displayed in the correct error color.
        /// </summary>
        /// <param name="e">The exception to display</param>
        private void ReportException ( Exception e )
        {
            if ( e != null )
            {
                object error ;
                var icer = e as IContainsErrorRecord ;
                if ( icer != null )
                {
                    error = icer.ErrorRecord ;
                }
                else
                {
                    error = ( object ) new ErrorRecord (
                                                        e
                                                      , "Host.ReportException"
                                                      , ErrorCategory.NotSpecified
                                                      , null
                                                       ) ;
                }

                try
                {
                    Shell.Commands.Clear();
                    Shell.AddScript ( "$input" ).AddCommand ( "out-string" ) ;

                    // Don't merge errors, this function will swallow errors.
                    Collection < PSObject > result ;
                    var inputCollection = new PSDataCollection < object > ( ) ;
                    inputCollection.Add ( error ) ;
                    inputCollection.Complete ( ) ; 
                    result = Shell.Invoke ( inputCollection ) ;

                    if ( result.Count > 0 )
                    {
                        var str = result[ 0 ].BaseObject as string ;
                        if ( ! string.IsNullOrEmpty ( str ) )
                        {
                            // out-string adds \r\n, we remove it
                            Host.UI.WriteErrorLine ( str.Substring ( 0 , str.Length - 2 ) ) ;
                        }
                    }
                }
                finally
                {
                }
            }
        }
    }


    public class WrappedSerializer : ValueSerializer
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ValueSerializer" /> class.</summary>
        /// 
        public WrappedSerializer ( ) { }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <summary>When overridden in a derived class, determines whether the specified object can be converted into a <see cref="T:System.String" />.</summary>
        /// <param name="value">The object to evaluate for conversion.</param>
        /// <param name="context">Context information that is used for conversion.</param>
        /// <returns>
        /// <see langword="true" /> if the <paramref name="value" /> can be converted into a <see cref="T:System.String" />; otherwise, <see langword="false." /></returns>
        public override bool CanConvertToString ( object value , IValueSerializerContext context )
        {
            return true ;
        }

        /// <summary>When overridden in a derived class, determines whether the specified <see cref="T:System.String" /> can be converted to an instance of the type that the implementation of <see cref="T:System.Windows.Markup.ValueSerializer" /> supports.</summary>
        /// <param name="value">The string to evaluate for conversion.</param>
        /// <param name="context">Context information that is used for conversion.</param>
        /// <returns>
        /// <see langword="true" /> if the value can be converted; otherwise, <see langword="false" />.</returns>
        public override bool CanConvertFromString ( string value , IValueSerializerContext context )
        {
            return true ;
        }

        /// <summary>When overridden in a derived class, converts the specified object to a <see cref="T:System.String" />.</summary>
        /// <param name="value">The object to convert into a string.</param>
        /// <param name="context">Context information that is used for conversion.</param>
        /// <returns>A string representation of the specified object.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="value" /> cannot be converted.</exception>
        public override string ConvertToString ( object value , IValueSerializerContext context )
        {
            var w = ( WrappedPowerShell ) value ;
            var s = context.GetValueSerializerFor ( w.OutputDocument.GetType ( ) ) ;
            if ( s != null
                 && s.CanConvertToString ( w.OutputDocument , context ) )
            {
                Debug.WriteLine ( s.ConvertToString ( w.OutputDocument , context ) ) ;
            }

            return "derp" ;
        }

        /// <summary>When overridden in a derived class, converts a <see cref="T:System.String" /> to an instance of the type that the implementation of <see cref="T:System.Windows.Markup.ValueSerializer" /> supports.</summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="context">Context information that is used for conversion.</param>
        /// <returns>A new instance of the type that the implementation of <see cref="T:System.Windows.Markup.ValueSerializer" /> supports based on the supplied <paramref name="value" />.</returns>
        /// <exception cref="T:System.NotSupportedException">
        /// <paramref name="value" /> cannot be converted.</exception>
        public override object ConvertFromString ( string value , IValueSerializerContext context )
        {
            return new WrappedPowerShell (  ) ;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString ( ) { return $"〖" + GetType ( ) + "〗" ; }
    }

    public class WrappedPowerShellProvider : TypeDescriptionProvider
    {
        private static TypeDescriptionProvider defaultTypeProvider =
            TypeDescriptor.GetProvider ( typeof ( WrappedPowerShell ) ) ;

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.TypeDescriptionProvider" /> class.</summary>
        public WrappedPowerShellProvider ( ) : base ( defaultTypeProvider ) { }


        /// <summary>Gets a custom type descriptor for the given type and object.</summary>
        /// <param name="objectType">The type of object for which to retrieve the type descriptor.</param>
        /// <param name="instance">An instance of the type. Can be <see langword="null" /> if no instance was passed to the <see cref="T:System.ComponentModel.TypeDescriptor" />.</param>
        /// <returns>An <see cref="T:System.ComponentModel.ICustomTypeDescriptor" /> that can provide metadata for the type.</returns>
        public override ICustomTypeDescriptor GetTypeDescriptor (
            Type   objectType
          , object instance
        )
        {
            var defaultDescriptor = base.GetTypeDescriptor ( objectType , instance ) ;

            return null;
        }

        //     return new WrappedPowerShellCustomTypeDescriptor();
        // }
        // return base.GetTypeDescriptor ( objectType , instance ) ;
    }
}
