using System;
using System.CommandLine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AisReplay;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Option<string> hostOption = new("-h", "--host")
        {
            Description = "Specifies the hostname or IP address.",
            Arity = ArgumentArity.ZeroOrOne
        };
        Option<int> portOption = new("-p", "--port")
        {
            Description = "Specifies the port number to broadcast the records."
        };
        Option<int> speedOption = new("-x", "--x-speed")
        {
            Description = "Specifies the playback speed."
        };
        Option<bool> gpsOption = new("-g", "--gps")
        {
            Description = "Specifies whether output contain own ship positions"
        };

        RootCommand rootCommand = new();

        rootCommand.Options.Add(hostOption);
        rootCommand.Options.Add(portOption);
        rootCommand.Options.Add(speedOption);
        rootCommand.Options.Add(gpsOption);

        var parseResult = rootCommand.Parse(args);

        if (parseResult.Errors.Count != 0 ) return 1;

        return await PublishMessages(parseResult.GetValue(hostOption)!, parseResult.GetValue(portOption), parseResult.GetValue(speedOption), parseResult.GetValue(gpsOption));
    }

    private static async Task<int> PublishMessages(string host, int port, int speed, bool gps = false)
    {
        using var udp = new UdpClient();
        var endpoint = new IPEndPoint(IPAddress.Parse(host), port);

        var prevTimestamp = DateTime.MinValue;
        var count = 0;

        while (await Console.In.ReadLineAsync() is { } line)
        {
            var record = CsvParser.ParseAisRecord(line);

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