using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace MovMux;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private string? _videoPath;
    private string? _audio1Path;
    private string? _audio2Path;
    private string? _audio1Title;
    private string? _audio2Title;
    private string? _outputPath;

    private string? _videoStartSecondsText;
    private string? _audio1StartSecondsText;
    private string? _audio2StartSecondsText;

    private string? _commandText;

    private bool _audio1IsAnalog;
    private bool _audio1IsDigital;
    private bool _audio1Is48k;
    private bool _audio1Is441k;

    private bool _audio2IsAnalog;
    private bool _audio2IsDigital;
    private bool _audio2Is48k;
    private bool _audio2Is441k;

    private string? _ffmpegLog;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        UpdateAudio1Title();
        UpdateAudio2Title();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? FfmpegLog
    {
        get => _ffmpegLog;
        private set => SetField(ref _ffmpegLog, value);
    }

    public string? VideoPath
    {
        get => _videoPath;
        set => SetField(ref _videoPath, value);
    }

    public string? Audio1Path
    {
        get => _audio1Path;
        set => SetField(ref _audio1Path, value);
    }

    public string? Audio2Path
    {
        get => _audio2Path;
        set => SetField(ref _audio2Path, value);
    }

    public string? Audio1Title
    {
        get => _audio1Title;
        private set => SetField(ref _audio1Title, value);
    }

    public string? Audio2Title
    {
        get => _audio2Title;
        private set => SetField(ref _audio2Title, value);
    }

    public string? OutputPath
    {
        get => _outputPath;
        set => SetField(ref _outputPath, value);
    }

    public string? VideoStartSecondsText
    {
        get => _videoStartSecondsText;
        set => SetField(ref _videoStartSecondsText, value);
    }

    public string? Audio1StartSecondsText
    {
        get => _audio1StartSecondsText;
        set => SetField(ref _audio1StartSecondsText, value);
    }

    public string? Audio2StartSecondsText
    {
        get => _audio2StartSecondsText;
        set => SetField(ref _audio2StartSecondsText, value);
    }

    public string? CommandText
    {
        get => _commandText;
        set => SetField(ref _commandText, value);
    }

    public bool Audio1IsAnalog
    {
        get => _audio1IsAnalog;
        set
        {
            if (!SetField(ref _audio1IsAnalog, value))
            {
                return;
            }

            if (value && Audio1IsDigital)
            {
                _audio1IsDigital = false;
                OnPropertyChanged(nameof(Audio1IsDigital));
            }

            UpdateAudio1Title();
        }
    }

    public bool Audio1IsDigital
    {
        get => _audio1IsDigital;
        set
        {
            if (!SetField(ref _audio1IsDigital, value))
            {
                return;
            }

            if (value && Audio1IsAnalog)
            {
                _audio1IsAnalog = false;
                OnPropertyChanged(nameof(Audio1IsAnalog));
            }

            UpdateAudio1Title();
        }
    }

    public bool Audio1Is48k
    {
        get => _audio1Is48k;
        set
        {
            if (!SetField(ref _audio1Is48k, value))
            {
                return;
            }

            if (value && Audio1Is441k)
            {
                _audio1Is441k = false;
                OnPropertyChanged(nameof(Audio1Is441k));
            }

            UpdateAudio1Title();
        }
    }

    public bool Audio1Is441k
    {
        get => _audio1Is441k;
        set
        {
            if (!SetField(ref _audio1Is441k, value))
            {
                return;
            }

            if (value && Audio1Is48k)
            {
                _audio1Is48k = false;
                OnPropertyChanged(nameof(Audio1Is48k));
            }

            UpdateAudio1Title();
        }
    }

    public bool Audio2IsAnalog
    {
        get => _audio2IsAnalog;
        set
        {
            if (!SetField(ref _audio2IsAnalog, value))
            {
                return;
            }

            if (value && Audio2IsDigital)
            {
                _audio2IsDigital = false;
                OnPropertyChanged(nameof(Audio2IsDigital));
            }

            UpdateAudio2Title();
        }
    }

    public bool Audio2IsDigital
    {
        get => _audio2IsDigital;
        set
        {
            if (!SetField(ref _audio2IsDigital, value))
            {
                return;
            }

            if (value && Audio2IsAnalog)
            {
                _audio2IsAnalog = false;
                OnPropertyChanged(nameof(Audio2IsAnalog));
            }

            UpdateAudio2Title();
        }
    }

    public bool Audio2Is48k
    {
        get => _audio2Is48k;
        set
        {
            if (!SetField(ref _audio2Is48k, value))
            {
                return;
            }

            if (value && Audio2Is441k)
            {
                _audio2Is441k = false;
                OnPropertyChanged(nameof(Audio2Is441k));
            }

            UpdateAudio2Title();
        }
    }

    public bool Audio2Is441k
    {
        get => _audio2Is441k;
        set
        {
            if (!SetField(ref _audio2Is441k, value))
            {
                return;
            }

            if (value && Audio2Is48k)
            {
                _audio2Is48k = false;
                OnPropertyChanged(nameof(Audio2Is48k));
            }

            UpdateAudio2Title();
        }
    }

    private void PathTextBox_PreviewDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void VideoPathTextBox_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetSingleDroppedFile(e, out var filePath))
        {
            return;
        }

        VideoPath = filePath;

        if (string.IsNullOrWhiteSpace(OutputPath) && !string.IsNullOrWhiteSpace(VideoPath))
        {
            OutputPath = GetDefaultOutputPath(VideoPath!);
        }
    }

    private void Audio1PathTextBox_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetSingleDroppedFile(e, out var filePath))
        {
            return;
        }

        Audio1Path = filePath;
    }

    private void Audio2PathTextBox_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetSingleDroppedFile(e, out var filePath))
        {
            return;
        }

        Audio2Path = filePath;
    }

    private void OutputPathTextBox_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetSingleDroppedFile(e, out var filePath))
        {
            return;
        }

        OutputPath = filePath;
    }

    private static bool TryGetSingleDroppedFile(DragEventArgs e, out string? filePath)
    {
        filePath = null;

        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return false;
        }

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths || paths.Length == 0)
        {
            return false;
        }

        string candidate = paths[0];
        if (string.IsNullOrWhiteSpace(candidate) || !File.Exists(candidate))
        {
            return false;
        }

        filePath = candidate;
        return true;
    }

    private void BrowseVideo_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "MOV (*.mov)|*.mov|All files (*.*)|*.*",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == true)
        {
            VideoPath = dialog.FileName;

            if (string.IsNullOrWhiteSpace(OutputPath) && !string.IsNullOrWhiteSpace(VideoPath))
            {
                OutputPath = GetDefaultOutputPath(VideoPath!);
            }
        }
    }

    private void BrowseAudio1_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Audio (*.wav;*.flac;*.m4a;*.aac;*.mp3)|*.wav;*.flac;*.m4a;*.aac;*.mp3|All files (*.*)|*.*",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == true)
        {
            Audio1Path = dialog.FileName;
        }
    }

    private void BrowseAudio2_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Audio (*.wav;*.flac;*.m4a;*.aac;*.mp3)|*.wav;*.flac;*.m4a;*.aac;*.mp3|All files (*.*)|*.*",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == true)
        {
            Audio2Path = dialog.FileName;
        }
    }

    private void BrowseOutput_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "MOV (*.mov)|*.mov|All files (*.*)|*.*",
            AddExtension = true,
            DefaultExt = ".mov",
            FileName = Path.GetFileName(OutputPath) ?? "out.mov",
        };

        if (dialog.ShowDialog(this) == true)
        {
            OutputPath = dialog.FileName;
        }
    }

    private void GenerateCommand_Click(object sender, RoutedEventArgs e)
    {
        CommandText = BuildFfmpegCommandLine(out var errorMessage);
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            MessageBox.Show(this, errorMessage, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        FfmpegLog = CommandText;
    }

    private void CopyCommand_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CommandText))
        {
            CommandText = BuildFfmpegCommandLine(out _);
        }

        if (!string.IsNullOrWhiteSpace(CommandText))
        {
            Clipboard.SetText(CommandText);
            FfmpegLog = CommandText;
        }
    }

    private void RunFfmpeg_Click(object sender, RoutedEventArgs e)
    {
        var commandLine = BuildFfmpegCommandLine(out var errorMessage);
        CommandText = commandLine;

        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            MessageBox.Show(this, errorMessage, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        FfmpegLog = commandLine;

        string ffmpegArgs = commandLine.StartsWith("ffmpeg ", StringComparison.OrdinalIgnoreCase)
            ? commandLine.Substring("ffmpeg ".Length)
            : commandLine;

        try
        {
            StartInNewCmd(ffmpegArgs);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                "Failed to start cmd.exe.\n\n" + ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static void StartInNewCmd(string ffmpegArgs)
    {
        // Keep it simple: let cmd parse the already-quoted ffmpeg arguments.
        // No extra quoting/escaping of quotes here.
        string cmdLine = "ffmpeg " + ffmpegArgs;

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/k " + cmdLine,
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory,
        });
    }

    private string BuildFfmpegCommandLine(out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(VideoPath) || !File.Exists(VideoPath))
        {
            errorMessage = "Select a valid video input.";
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(Audio1Path) || !File.Exists(Audio1Path))
        {
            errorMessage = "Select a valid Audio 1 input.";
            return string.Empty;
        }

        bool hasAudio2 = !string.IsNullOrWhiteSpace(Audio2Path) && File.Exists(Audio2Path);

        if (string.IsNullOrWhiteSpace(OutputPath) && !string.IsNullOrWhiteSpace(VideoPath))
        {
            OutputPath = GetDefaultOutputPath(VideoPath!);
        }

        if (!TryParseOptionalSeconds(VideoStartSecondsText, out var vss, out var vssError))
        {
            errorMessage = vssError;
            return string.Empty;
        }

        if (!TryParseOptionalSeconds(Audio1StartSecondsText, out var a1ss, out var a1ssError))
        {
            errorMessage = a1ssError;
            return string.Empty;
        }

        if (!TryParseOptionalSeconds(Audio2StartSecondsText, out var a2ss, out var a2ssError))
        {
            errorMessage = a2ssError;
            return string.Empty;
        }

        var args = new StringBuilder();
        args.Append("ffmpeg ");

        AppendInput(args, vss, VideoPath);
        AppendInput(args, a1ss, Audio1Path);

        if (hasAudio2)
        {
            AppendInput(args, a2ss, Audio2Path!);
        }

        args.Append("-map 0:v -map 1:a ");
        if (hasAudio2)
        {
            args.Append("-map 2:a ");
        }

        args.Append("-c copy ");

        if (hasAudio2)
        {
            args.Append("-copyinkf ");

            args.Append("-metadata:s:a:0 title=").Append(QuoteArg(Audio1Title ?? string.Empty)).Append(' ');
            args.Append("-metadata:s:a:0 handler_name=").Append(QuoteArg(Audio1Title ?? string.Empty)).Append(' ');

            args.Append("-metadata:s:a:1 title=").Append(QuoteArg(Audio2Title ?? string.Empty)).Append(' ');
            args.Append("-metadata:s:a:1 handler_name=").Append(QuoteArg(Audio2Title ?? string.Empty)).Append(' ');

            args.Append("-movflags +use_metadata_tags+faststart ");
        }
        else
        {
            args.Append("-movflags +faststart ");
        }

        args.Append("-avoid_negative_ts make_zero -shortest ");
        args.Append(QuoteArg(OutputPath!));

        return args.ToString().Trim();
    }

    private static void AppendInput(StringBuilder args, double? ssSeconds, string path)
    {
        if (ssSeconds is not null)
        {
            args.Append("-ss ").Append(FormatSeconds(ssSeconds.Value)).Append(' ');
        }

        args.Append("-i ").Append(QuoteArg(path)).Append(' ');
    }

    private static bool TryParseOptionalSeconds(string? text, out double? seconds, out string? error)
    {
        seconds = null;
        error = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        string t = text.Trim();

        if (!double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out double s) &&
            !double.TryParse(t, NumberStyles.Float, CultureInfo.CurrentCulture, out s))
        {
            error = "The -ss value must be a number (e.g. 2 or 2.5) or blank.";
            return false;
        }

        if (s < 0)
        {
            error = "The -ss value must be non-negative.";
            return false;
        }

        if (s == 0)
        {
            return true;
        }

        seconds = s;
        return true;
    }

    private static string FormatSeconds(double seconds)
        => seconds.ToString("0.###", CultureInfo.InvariantCulture);

    private static string QuoteArg(string value)
    {
        string v = value.Replace("\"", "\\\"", StringComparison.Ordinal);
        return "\"" + v + "\"";
    }

    private static string GetDefaultOutputPath(string videoPath)
    {
        string dir = Path.GetDirectoryName(videoPath) ?? string.Empty;
        string name = Path.GetFileNameWithoutExtension(videoPath);
        return Path.Combine(dir, name + "_mux.mov");
    }

    private void UpdateAudio1Title()
        => Audio1Title = BuildTitle(Audio1IsAnalog, Audio1IsDigital, Audio1Is48k, Audio1Is441k);

    private void UpdateAudio2Title()
        => Audio2Title = BuildTitle(Audio2IsAnalog, Audio2IsDigital, Audio2Is48k, Audio2Is441k);

    private static string BuildTitle(bool isAnalog, bool isDigital, bool is48k, bool is441k)
    {
        string source = isAnalog ? "Analog" : isDigital ? "Digital" : string.Empty;
        string rate = is48k ? "48 kHz" : is441k ? "44.1 kHz" : string.Empty;

        if (string.IsNullOrWhiteSpace(source) && string.IsNullOrWhiteSpace(rate))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            return rate;
        }

        if (string.IsNullOrWhiteSpace(rate))
        {
            return source;
        }

        return source + " " + rate;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void FfmpegLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true,
        });

        e.Handled = true;
    }

    private void FfmpegLogTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        => FfmpegLogTextBox.ScrollToEnd();

    private static readonly Regex SecondsRegex = new(@"^\d*([.]\d*)?$", RegexOptions.Compiled);

    private void SecondsTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox tb)
        {
            return;
        }

        string proposed = GetProposedText(tb, e.Text);
        e.Handled = !SecondsRegex.IsMatch(proposed);
    }

    private void SecondsTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox tb)
        {
            return;
        }

        if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
        {
            e.CancelCommand();
            return;
        }

        var pasteText = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;
        string proposed = GetProposedText(tb, pasteText);

        if (!SecondsRegex.IsMatch(proposed))
        {
            e.CancelCommand();
        }
    }

    private static string GetProposedText(System.Windows.Controls.TextBox tb, string newText)
    {
        string text = tb.Text ?? string.Empty;

        int start = tb.SelectionStart;
        int length = tb.SelectionLength;

        if (start < 0)
        {
            start = 0;
        }

        if (start > text.Length)
        {
            start = text.Length;
        }

        if (length < 0)
        {
            length = 0;
        }

        if (start + length > text.Length)
        {
            length = text.Length - start;
        }

        return text.Remove(start, length).Insert(start, newText);
    }
}