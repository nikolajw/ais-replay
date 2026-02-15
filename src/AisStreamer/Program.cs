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
            Description = "Path to input file to be streamed"
        };


        RootCommand rootCommand = new();

        rootCommand.Options.Add(hostOption);
        rootCommand.Options.Add(portOption);
        rootCommand.Options.Add(speedOption);
        rootCommand.Options.Add(gpsOption);
        rootCommand.Options.Add(fileOption);

        rootCommand.SetAction(async parseResult =>
        {
            var host = parseResult.GetValue(hostOption)!;
            var port = parseResult.GetValue(portOption);
            var speed = parseResult.GetValue(speedOption);
            var file = parseResult.GetValue(fileOption);
            var gps = parseResult.GetValue(gpsOption);

            return await PublishMessages(host, port, speed, file, gps);
        });

        var parseResult = rootCommand.Parse(args);
        await parseResult.InvokeAsync();

        return 0;
    }

    private static async Task<int> PublishMessages(string host, int port, int speed, FileInfo? inputFile = null,
        bool gps = false)
    {
        
        try
        {
            using var udp = new UdpClient();
            udp.Connect(host, port);

            var prevTimestamp = DateTime.MinValue;
            var count = 0;

            var reader = inputFile?.OpenText() ?? Console.In;

            while (await reader.ReadLineAsync() is { } line)
            {
                var record = CsvParser.ParseAisRecord(line);

                if (record == AisRecord.None) continue;

                Sleep(speed, prevTimestamp, record);
    
                var sentence = gps ? NmeaEncoder.ToGprmc(record) : NmeaEncoder.ToNmea0183(record);
                var bytes = Encoding.ASCII.GetBytes(sentence + "\r\n");

                await udp.SendAsync(bytes, bytes.Length);

                Console.Error.WriteLine(record);
                prevTimestamp = record.Timestamp;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
        }

        return 0;
        }

        private static void Sleep(int speed, DateTime prevTimestamp, AisRecord record)
        {
            if (prevTimestamp != DateTime.MinValue)
            {
                var delay = record.Timestamp - prevTimestamp;
                if (delay.TotalMilliseconds > 0.0)
                    Thread.Sleep(delay / speed);
            }
        }
    }