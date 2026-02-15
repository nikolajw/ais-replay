using System;
using System.CommandLine;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AisStreamer;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Option<string> hostOption = new("--host")
        {
            Description = "Specifies the hostname or IP address (default: 127.0.0.1).",
            DefaultValueFactory = _ => "127.0.0.1"
        };
        Option<int> portOption = new("-p", "--port")
        {
            Description = "Specifies the port number to broadcast the records (default: 10110).",
            DefaultValueFactory = _ => 10110
        };
        Option<int> speedOption = new("-x", "--x-speed")
        {
            Description = "Specifies the playback speed.",
            DefaultValueFactory = _ => 1
        };
        Option<bool> gpsOption = new("-g", "--gps")
        {
            Description = "Specifies whether output contain own ship positions"
        };
        Option<FileInfo> fileOption = new("-f", "--input-file")
        {
            Description = "Path to file to be streamed"
        };

        
        RootCommand rootCommand = new();

        rootCommand.Options.Add(hostOption);
        rootCommand.Options.Add(portOption);
        rootCommand.Options.Add(speedOption);
        rootCommand.Options.Add(gpsOption);
        rootCommand.Options.Add(fileOption);
        
        var parseResult = rootCommand.Parse(args);

        if (parseResult.Errors.Count != 0 ) return 1;

        return await PublishMessages(parseResult.GetValue(hostOption)!, parseResult.GetValue(portOption), parseResult.GetValue(speedOption), parseResult.GetValue(fileOption), parseResult.GetValue(gpsOption));
    }

    private static async Task<int> PublishMessages(string host, int port, int speed, FileInfo? inputFile = null, bool gps = false)
    {
        using var udp = new UdpClient();
        var endpoint = new IPEndPoint(IPAddress.Parse(host), port);

        var prevTimestamp = DateTime.MinValue;
        var count = 0;

        var reader = inputFile?.OpenText() ?? Console.In;

        while (await reader.ReadLineAsync() is { } line)
        {
            var record = CsvParser.ParseAisRecord(line);

            if (record == AisRecord.None) continue;

            count++;
            if (count % speed != 0) return 0;
            
            if (prevTimestamp != DateTime.MinValue)
            {
                var delay = record.Timestamp - prevTimestamp;
                if (delay.TotalMilliseconds > 0.0)
                    Thread.Sleep(delay / speed);
            }

            var sentence = gps ? NmeaEncoder.ToGprmc(record) : NmeaEncoder.ToNmea0183(record);
            var bytes = Encoding.ASCII.GetBytes(sentence + "\r\n");
            await udp.SendAsync(bytes, bytes.Length, endpoint);
            Console.WriteLine(record);
            prevTimestamp = record.Timestamp;
        }
        
        return 0;
    }
}