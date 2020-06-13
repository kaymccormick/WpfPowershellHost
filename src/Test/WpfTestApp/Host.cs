using System;
using System.Collections.Generic;
using System.Diagnostics ;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces ;
using System.Text;
using System.Threading.Tasks;
using Terminal1;
using WpfTerminalControlLib;


namespace PowerShellShared
{
    public class Host : PSHost, IHostSupportsInteractiveSession
    {
        private bool _isRunspacePushed;
        private Runspace _runspace;
        private Stack<Runspace> _runspaces = new Stack<Runspace>();
        private HostUI _hostUI;
        private readonly PSHostUserInterface _uI;
        private CultureInfo _CurrentCulture;
        private CultureInfo _CurrentUICulture;
        private readonly PSObject _privateData;

        public Host(WpfTerminalControl uicontrol )
        {
            if (uicontrol == null) throw new ArgumentNullException(nameof(uicontrol));
            this.InstanceId = Guid.NewGuid();
            this.HostUI = new HostUI(uicontrol);
            _uI = this.HostUI;
            this.Name = "Host1";
            this.Version = Version.Parse("0.1");
            _CurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture ??
                             System.Threading.Thread.CurrentThread.CurrentUICulture ?? CultureInfo.InstalledUICulture;
            _CurrentUICulture = _CurrentCulture;
        }

        public HostUI HostUI
        {
            get { return _hostUI ; }
            set => _hostUI = value;
        }

        public override void SetShouldExit(int exitCode)
        {  
            UI.WriteDebugLine("exit");
        }

        public override void EnterNestedPrompt()
        {
            Debug.WriteLine("NEsted prompt");
        }

        public override void ExitNestedPrompt()
        {
            Debug.WriteLine("NEsted prompt");
        }

        public override void NotifyBeginApplication()
        {
            // UI.WriteDebugLine("Begin application.");
            Debug.WriteLine ( "Begin application" ) ;
        }

        public override void NotifyEndApplication()
        {
            Debug.WriteLine("NEsted prompt");
        }

        public override CultureInfo CurrentCulture => _CurrentCulture;
        public override CultureInfo CurrentUICulture => _CurrentUICulture;
        public override Guid InstanceId { get; }
        public override string Name { get; }
        public override PSHostUserInterface UI
        {
            get
            {
                Debug.WriteLine ( "Request for user interface" ) ;
                return _uI ;
            }
        }

        public override Version Version { get; }

        /// <inheritdoc />
        public override PSObject PrivateData
        {
            get { return _privateData; }
        }

        #region Implementation of IHostSupportsInteractiveSession
        public void PushRunspace(Runspace runspace)
        {
            Debug.WriteLine(nameof(PushRunspace));
            _runspaces.Push(runspace);
            _runspace = runspace;
        }

        public void PopRunspace()
        {
            _runspaces.Pop();
        }

        public bool IsRunspacePushed { get { return _isRunspacePushed; } set { _isRunspacePushed = value; } }

        public Runspace Runspace
        {
            get
            {
                Debug.WriteLine ( "Request for runspace" ) ;
                return _runspace ;
            }
            set
            {
                _runspace = value ;
            }
        }
        #endregion
    }
}
