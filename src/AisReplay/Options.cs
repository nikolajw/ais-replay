using System.ComponentModel;
using Spectre.Console.Cli;

namespace AisReplay;

public class Options : CommandSettings
{
    [CommandOption("-f|--file")]
    [Description("Path to a CSV file with AIS records")]
    public string? File { get; set; }

    [CommandOption("-d|--date")]
    [Description("Download data for a specific date (YYYY-MM-DD)")]
    public string? Date { get; set; }

    [CommandOption("-m|--mmsi")]
    [Description("Filter to a specific vessel by MMSI (Maritime Mobile Service Identity)")]
    public string? Mmsi { get; set; }

    [CommandOption("-x|--x-speed")]
    [DefaultValue(1)]
    [Description("Playback speed multiplier (default: 1)")]
    public int Speed { get; set; }

    [CommandOption("-g|--gps")]
    [DefaultValue(false)]
    [Description("Output GPS format (GPRMC) instead of NMEA 0183")]
    public bool Gps { get; set; }

    [CommandOption("-s|--skip-moored")]
    [DefaultValue(false)]
    [Description("Skip moored/stationary vessels")]
    public bool SkipMoored { get; set; }

    [CommandOption("-c|--purge-cache")]
    [DefaultValue(false)]
    [Description("Clear cached downloads and exit")]
    public bool PurgeCache { get; set; }

    [CommandOption("--host")]
    [DefaultValue("127.0.0.1")]
    [Description("UDP host/IP address to send events to (default: 127.0.0.1)")]
    public string Host { get; set; } = "127.0.0.1";

    [CommandOption("-p|--port")]
    [DefaultValue(10110)]
    [Description("UDP port to send events to (default: 10110)")]
    public int Port { get; set; }
}
