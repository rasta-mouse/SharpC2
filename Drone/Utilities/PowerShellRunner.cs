using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;

namespace Drone.Utilities;

public class PowerShellRunner : IDisposable
{
    private readonly PSHost _host;
    private readonly Runspace _rs;
    private readonly Pipeline _pipeline;

    public PowerShellRunner()
    {
        _host = new CustomPSHost();

        var state = InitialSessionState.CreateDefault();
        state.AuthorizationManager = null;
        state.LanguageMode = PSLanguageMode.FullLanguage;

        _rs = RunspaceFactory.CreateRunspace(_host, state);
        _rs.Open();
        _pipeline = _rs.CreatePipeline();
    }

    public void ImportScript(string script)
    {
        _pipeline.Commands.AddScript(script);
    }

    public string Invoke(string command)
    {
        _pipeline.Commands.AddScript(command);
        _pipeline.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
        _pipeline.Commands.Add("out-default");

        _pipeline.Invoke();

        return ((CustomPSHostUserInterface) _host.UI).Output;
    }

    public void Dispose()
    {
        _pipeline.Dispose();
        _rs.Dispose();
    }

    private class CustomPSHost : PSHost
    {
        private Guid _hostId = Guid.NewGuid();
        private CustomPSHostUserInterface _ui = new();
        public override Guid InstanceId => _hostId;
        public override string Name => "ConsoleHost";
        public override Version Version => new(1, 0);
        public override PSHostUserInterface UI => _ui;
        public override CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;
        public override CultureInfo CurrentUICulture => Thread.CurrentThread.CurrentUICulture;

        public override void EnterNestedPrompt()
            => throw new NotImplementedException(
                "EnterNestedPrompt is not implemented.  The script is asking for input, which is a problem since there's no console.  Make sure the script can execute without prompting the user for input.");

        public override void ExitNestedPrompt()
            => throw new NotImplementedException(
                "ExitNestedPrompt is not implemented.  The script is asking for input, which is a problem since there's no console.  Make sure the script can execute without prompting the user for input.");

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override void SetShouldExit(int exitCode)
        {
        }
    }

    private class CustomPSHostUserInterface : PSHostUserInterface
    {
        // Replace StringBuilder with whatever your preferred output method is (e.g. a socket or a named pipe)
        private StringBuilder _sb;
        private CustomPSRHostRawUserInterface _rawUi = new();

        public CustomPSHostUserInterface() => _sb = new StringBuilder();

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
            => _sb.Append(value);

        public override void WriteLine()
            => _sb.Append("\n");

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
            => _sb.Append(value + "\n");

        public override void Write(string value) => _sb.Append(value);
        public override void WriteDebugLine(string message) => _sb.AppendLine("DEBUG: " + message);
        public override void WriteErrorLine(string value) => _sb.AppendLine("ERROR: " + value);
        public override void WriteLine(string value) => _sb.AppendLine(value);
        public override void WriteVerboseLine(string message) => _sb.AppendLine("VERBOSE: " + message);
        public override void WriteWarningLine(string message) => _sb.AppendLine("WARNING: " + message);

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
        }

        public string Output => _sb.ToString();

        public override Dictionary<string, PSObject> Prompt(string caption, string message,
            System.Collections.ObjectModel.Collection<FieldDescription> descriptions)
            => throw new NotImplementedException();

        public override int PromptForChoice(string caption, string message,
            System.Collections.ObjectModel.Collection<ChoiceDescription> choices, int defaultChoice)
            => throw new NotImplementedException();

        public override PSCredential PromptForCredential(string caption, string message, string userName,
            string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
            => throw new NotImplementedException();

        public override PSCredential PromptForCredential(string caption, string message, string userName,
            string targetName)
            => throw new NotImplementedException();

        public override PSHostRawUserInterface RawUI => _rawUi;

        public override string ReadLine() => throw new NotImplementedException();

        public override System.Security.SecureString ReadLineAsSecureString()
            => throw new NotImplementedException();
    }

    private class CustomPSRHostRawUserInterface : PSHostRawUserInterface
    {
        private Size _windowSize = new() {Width = 120, Height = 100};
        private Coordinates _cursorPosition = new() {X = 0, Y = 0};

        private int _cursorSize = 1;
        private ConsoleColor _foregroundColor = ConsoleColor.White;
        private ConsoleColor _backgroundColor = ConsoleColor.Black;

        private Size _maxPhysicalWindowSize = new()
        {
            Width = int.MaxValue,
            Height = int.MaxValue
        };

        private Size _maxWindowSize = new() {Width = 100, Height = 100};
        private Size _bufferSize = new() {Width = 100, Height = 1000};
        private Coordinates _windowPosition = new() {X = 0, Y = 0};
        private string _windowTitle = "";

        public override ConsoleColor BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public override Size BufferSize
        {
            get => _bufferSize;
            set => _bufferSize = value;
        }

        public override Coordinates CursorPosition
        {
            get => _cursorPosition;
            set => _cursorPosition = value;
        }

        public override int CursorSize
        {
            get => _cursorSize;
            set => _cursorSize = value;
        }

        public override void FlushInputBuffer() => throw new NotImplementedException();

        public override ConsoleColor ForegroundColor
        {
            get => _foregroundColor;
            set => _foregroundColor = value;
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
            => throw new NotImplementedException();

        public override bool KeyAvailable => throw new NotImplementedException();

        public override Size MaxPhysicalWindowSize => _maxPhysicalWindowSize;

        public override Size MaxWindowSize => _maxWindowSize;

        public override KeyInfo ReadKey(ReadKeyOptions options) => throw new NotImplementedException();

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip,
            BufferCell fill) => throw new NotImplementedException();

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
            => throw new NotImplementedException();

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
            => throw new NotImplementedException();

        public override Coordinates WindowPosition
        {
            get => _windowPosition;
            set => _windowPosition = value;
        }

        public override Size WindowSize
        {
            get => _windowSize;
            set => _windowSize = value;
        }

        public override string WindowTitle
        {
            get => _windowTitle;
            set => _windowTitle = value;
        }
    }
}