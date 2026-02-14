using System.ComponentModel;
using Spectre.Console.Cli;

namespace AisFileLoader;

public class Options : CommandSettings
{
    [CommandOption("-i|--input")]
    [Description("Input CSV file path(s) (can be specified multiple times)")]
    public string[] Inputs { get; set; } = [];

    [CommandOption("-o|--output")]
    [Description("Output CSV file path (default: write to stdout)")]
    public string? Output { get; set; }

    [CommandOption("-m|--mmsi-file")]
    [Description("File containing MMSI numbers to filter (one per line)")]
    public string? MmsiFile { get; set; }

    [CommandOption("-l|--mmsi-list")]
    [Description("Comma-separated list of MMSI numbers to filter")]
    public string? MmsiList { get; set; }

    [CommandOption("--mmsi-stdin")]
    [DefaultValue(false)]
    [Description("Read MMSI numbers from stdin (one per line)")]
    public bool MmsiStdin { get; set; }

    [CommandOption("-e|--exclude")]
    [DefaultValue(false)]
    [Description("Exclude the specified MMSIs instead of including only them")]
    public bool Exclude { get; set; }

    [CommandOption("-d|--date")]
    [Description("Download data from ais.dk for specific date(s) (YYYY-MM-DD, can be specified multiple times)")]
    public string[] Dates { get; set; } = [];
}
