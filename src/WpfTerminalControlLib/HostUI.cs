using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NLog;
using Terminal1;
using WpfCustomControlLibrary1;
using WpfTerminalControlLib;
using ReadKeyOptions = System.Management.Automation.Host.ReadKeyOptions;

// ReSharper disable once CheckNamespace
namespace PowerShellShared
{
    public class HostUI : PSHostUserInterface
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly WpfTerminalControl _wpfTerminalControl;
        private readonly RawUI myRawUi;
        private readonly TerminalInterface _terminalInterface;

        public override PSHostRawUserInterface RawUI
        {
            get { return myRawUi; }
        }

        public HostUI(WpfTerminalControl wpfTerminalControl)
        {
            if (wpfTerminalControl == null) throw new ArgumentNullException(nameof(wpfTerminalControl));
            _wpfTerminalControl = wpfTerminalControl;
            _terminalInterface = MakeView(wpfTerminalControl);

            // WriteDebugLine("this is debug");
            myRawUi = new RawUI(_terminalInterface);
        }

        private TerminalInterface MakeView(WpfTerminalControl o)
        {
            return new TerminalInterface(o);
        }

        public override string ReadLine()
        {
            var ch = new List<char>();
            for (;;)
            {
                var i = RawUI.ReadKey(ReadKeyOptions.IncludeKeyDown);
                if (i.Character == '\r') break;
                ch.Add(i.Character);
            }

            return string.Join("", ch);
        }

        /// <inheritdoc />
        public override void WriteLine()
        {
            Logger.Debug(nameof(WriteLine));
            myRawUi.WriteLine();
        }

        /// <inheritdoc />
        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            Logger.Debug(nameof(WriteLine) + $" {foregroundColor} {backgroundColor} {value}");
            Write(foregroundColor, backgroundColor, value);
            WriteLine();
        }

        /// <inheritdoc />
        public override void WriteInformation(InformationRecord record)
        {
            _terminalInterface.Dispatcher.Invoke(() =>
            {
                _terminalInterface.WriteInformation(new MyInformationRecord(record.MessageData, record.Source,
                    record.TimeGenerated, record.Tags.ToList(), record.User, record.Computer, record.ProcessId,
                    record.NativeThreadId, record.ManagedThreadId));
            });
        }

        public override SecureString ReadLineAsSecureString()
        {
            return new SecureString();
        }

        public override void Write(string value)
        {
            Logger.Debug(nameof(Write) + $" {value}");
            _terminalInterface.Dispatcher.Invoke(() => { _terminalInterface.Write(value); });
            // if (string.IsNullOrEmpty(value)) return;
            // var a = myRawUi.NewBufferCellArray(new[] {value}, CurForegroundColor.Value, CurBackgroundColor.Value);
            // myRawUi.SetBufferContents(new Coordinates(myRawUi.CursorPosition.X, myRawUi.CursorPosition.Y),
            // a);
        }

        public ConsoleColor? CurBackgroundColor { get; set; } = ConsoleColor.White;

        public ConsoleColor? CurForegroundColor { get; set; } = ConsoleColor.Black;

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            Debug.WriteLine("Writing with fg color of " + foregroundColor);
            var a = myRawUi.NewBufferCellArray(new[] {value}, foregroundColor, backgroundColor);
            myRawUi.SetBufferContents(new Coordinates(myRawUi.CursorPosition.X, myRawUi.CursorPosition.Y),
                a);
            CurForegroundColor = foregroundColor;
            CurBackgroundColor = backgroundColor;
        }

        public override void WriteLine(string value)
        {
            _wpfTerminalControl.Dispatcher.Invoke(() => { _terminalInterface.WriteLine(value); });
        }

        public override void WriteErrorLine(string value)
        {
                _wpfTerminalControl.Dispatcher.Invoke(() => { _terminalInterface.WriteErrorLine(value); });
        }

        public override void WriteDebugLine(string message)
        {
            _wpfTerminalControl.Dispatcher.Invoke(() => { _terminalInterface.WriteDebugLine(message); });
            // var a = myRawUi.NewBufferCellArray(new[] {message}, CurForegroundColor.Value, CurBackgroundColor.Value);
            // myRawUi.SetBufferContents(new Coordinates(myRawUi.CursorPosition.X, myRawUi.CursorPosition.Y),
            //     a);
            // myRawUi.WriteLine();
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            var x = new MyRecord()
            {
                Activity = record.Activity, ActivityId = record.ActivityId,
                CurrentOperation = record.CurrentOperation
            };

            x.ParentActivityId = record.ParentActivityId;
            x.PercentComplete = record.PercentComplete;
            x.RecordType = (int) record.RecordType;
            x.SecondsRemaining = record.SecondsRemaining;

            x.StatusDescription = record.StatusDescription;
            _terminalInterface.Dispatcher.Invoke(() => { _terminalInterface.WriteProgress(sourceId, x); });
        }

        public override void WriteVerboseLine(string message)
        {
            _terminalInterface.Dispatcher.Invoke(() => { _terminalInterface.WriteVerboseLine(message); });
            // var a = myRawUi.NewBufferCellArray(new[] { message }, CurForegroundColor.Value, CurBackgroundColor.Value);
            // myRawUi.SetBufferContents(new Coordinates(myRawUi.CursorPosition.X, myRawUi.CursorPosition.Y),
            // a);
            // myRawUi.WriteLine();
        }

        public override void WriteWarningLine(string message)
        {
            _terminalInterface.Dispatcher.Invoke(() => { _terminalInterface.WriteWarningLine(message); });
            //
            // var a = myRawUi.NewBufferCellArray(new[] { message }, CurForegroundColor.Value, CurBackgroundColor.Value);
            // myRawUi.SetBufferContents(new Coordinates(myRawUi.CursorPosition.X, myRawUi.CursorPosition.Y),
            //     a);
            // myRawUi.CursorPosition = new Coordinates(0, myRawUi.CursorPosition.Y + 1);
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message,
            Collection<FieldDescription> descriptions)
        {
            var d = descriptions.Select(x =>
            {
                var myFieldDescription = new MyFieldDescription(x.DefaultValue, x.HelpMessage, x.IsMandatory, x.Label,
                    x.Name, x.ParameterAssemblyFullName, x.ParameterTypeFullName, x.ParameterTypeName);
                foreach (var argAttribute in x.Attributes) myFieldDescription.Attributes.Add(argAttribute);

                return myFieldDescription;
            }).ToList();

            var psObjects = new Dictionary<string, PSObject>();
            var dd = _wpfTerminalControl.Dispatcher.Invoke(() => _wpfTerminalControl.Prompt(caption, message, d));
            foreach (DictionaryEntry o in dd) psObjects[(string) o.Key] = new PSObject(o.Value);
            return psObjects;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName,
            string targetName)
        {
            var cred = _wpfTerminalControl.Dispatcher.Invoke(() =>
                _wpfTerminalControl.PromptForCredential(caption, message, userName, targetName));
            return new PSCredential("", new SecureString());
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName,
            string targetName,
            PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return new PSCredential("", new SecureString());
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices,
            int defaultChoice)
        {
            return 1;
        }

        public bool SupportsVirtualTerminal
        {
            get { return true; }
        }
    }
}