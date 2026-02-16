# ais-tools

A C# .NET suite of command-line tools for working with Automatic Identification System (AIS) vessel tracking data. Download AIS records from CSV files or [aisdata.ais.dk](http://aisdata.ais.dk/), convert them to marine communication standards (NMEA 0183 or GPS GPRMC), and broadcast via UDP for marine navigation software integration.

## Tools Included

### AisStreamer
The main tool that replays AIS vessel data via UDP broadcast.

**Features:**
- Load AIS data from CSV files or download from aisdata.ais.dk
- Broadcast to UDP endpoint (customizable host/port, default: 127.0.0.1:10110)
- Filter by vessel MMSI (Maritime Mobile Service Identity)
- Convert to NMEA 0183 Type 1 or GPS GPRMC format
- Adjustable playback speed multiplier (1x, 2x, 10x, etc.)
- Skip moored/stationary vessels
- Automatic caching of downloads
- AOT compiled for fast startup and low memory usage

### AisLoader
A preprocessing utility for filtering and combining AIS CSV data.

**Features:**
- Filter CSV files by MMSI numbers
- Load MMSI lists from files or stdin
- Download and filter data from aisdata.ais.dk
- Combine multiple files and dates
- Output to stdout for piping to other tools
- Exclude mode (inverse filtering)

## Getting Started

### Requirements
- .NET 10.0 or later
- A local machine with UDP network capability

### Installation

#### Option 1: Build from Source

1. Clone the repository:
   ```bash
   git clone git@github.com:nikolajw/ais-tools.git
   cd ais-tools
   ```

2. Build both projects:
   ```bash
   dotnet build
   ```

3. Run AisStreamer:
   ```bash
   dotnet run --project src/AisStreamer -- --file data.csv
   ```

### Installation

#### Option 2: Pre-built Binaries

Download pre-built binaries for Linux, macOS, or Windows from [GitHub Releases](https://github.com/nikolajw/ais-tools/releases).

## Usage

### AisStreamer

#### Replay from a Local CSV File
```bash
aisstreamer --file shipdata.csv
```

#### Download and Replay from a Specific Date
```bash
aisstreamer --date 2024-01-15
```

#### Filter by Vessel and Adjust Playback Speed
```bash
aisstreamer -f shipdata.csv -m 220382000 -x 10
```

#### Output in GPS Format
```bash
aisstreamer -f shipdata.csv --gps
```

#### Send to a Different Host/Port
```bash
aisstreamer -f shipdata.csv -h 192.168.1.100 -p 5000
```

#### Skip Moored Vessels
```bash
aisstreamer -f shipdata.csv --skip-moored
```

#### Clear Cached Downloads
```bash
aisstreamer --purge-cache
```

### AisStreamer Options

| Short | Long | Description |
|-------|------|-------------|
| `-f` | `--file <path>` | CSV file with AIS records |
| `-d` | `--date <YYYY-MM-DD>` | Download data for specific date |
| `-m` | `--mmsi <mmsi>` | Filter to specific vessel |
| `-x` | `--x-speed <multiplier>` | Playback speed multiplier |
| `-g` | `--gps` | Output GPS GPRMC format instead of NMEA 0183 |
| `-s` | `--skip-moored` | Skip moored/stationary vessels |
| `-c` | `--purge-cache` | Clear cached downloads and exit |
| `-h` | `--host <ip>` | UDP host (default: 127.0.0.1) |
| `-p` | `--port <port>` | UDP port (default: 10110) |

### AisLoader

#### Filter by MMSI List
```bash
aisloader -i input.csv -l "220382000,210409000" > output.csv
```

#### Filter by MMSI File
```bash
aisloader -i input.csv -m vessels.txt > output.csv
```

#### Download and Filter by Date
```bash
aisloader -d 2024-01-15 -m vessels.txt > output.csv
```

#### Multiple Dates
```bash
aisloader -d 2024-01-15 -d 2024-01-16 -d 2024-01-17 -m vessels.txt > output.csv
```

#### Pipe Between Tools
```bash
# Filter and stream directly
aisloader -d 2024-01-15 -m vessels.txt | \
  aisstreamer -f /dev/stdin -h 192.168.1.100 -p 5000
```

## CSV File Format

AisStreamer expects CSV files with the following structure:

```
Timestamp,Vessel Name,MMSI,Latitude,Longitude,Navigational Status,ROT,SOG,COG,Heading
dd/MM/yyyy HH:mm:ss,Name,123456789,55.1234,12.5678,under way using engine,0.5,12.3,180.5,185
```

**Fields:**
- **Timestamp**: dd/MM/yyyy HH:mm:ss format (required)
- **Vessel Name**: Ignored in current implementation
- **MMSI**: 9-digit Maritime Mobile Service Identity
- **Latitude**: Decimal degrees (positive = North)
- **Longitude**: Decimal degrees (positive = East)
- **Navigational Status**: e.g., "under way using engine", "at anchor", "moored"
- **ROT**: Rate of Turn (degrees per minute)
- **SOG**: Speed Over Ground (knots)
- **COG**: Course Over Ground (degrees)
- **Heading**: True heading (degrees)

## Output Formats

### NMEA 0183 (Default)
Standard marine vessel tracking protocol:
```
!AIVDM,1,1,,A,15MOK=PP001G?tO`K>2IbDpUkP0S,0*5C
```

### GPS GPRMC (with --gps flag)
GPS Recommended Minimum format:
```
$GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A
```

## Project Structure

```
.
├── readme.md
├── ais-tools.slnx          # Solution file
├── CLAUDE.md               # Development guide
└── src/
    ├── AisStreamer/        # Main replay tool
    │   ├── Program.cs
    │   ├── Options.cs
    │   ├── AisRecord.cs
    │   ├── CsvParser.cs
    │   ├── NmeaEncoder.cs
    │   └── AisStreamer.csproj
    │
    └── AisLoader/          # CSV filtering tool
        ├── Program.cs
        ├── Options.cs
        ├── AisRecord.cs
        ├── CsvParser.cs
        └── AisLoader.csproj
```

## Development

For detailed development information, see [CLAUDE.md](CLAUDE.md).

### Building

```bash
# Build both projects
dotnet build

# Release build
dotnet build --configuration Release

# Publish self-contained binaries
dotnet publish --configuration Release --runtime linux-x64 --self-contained
```

## Cache

Downloaded files are cached in the system temp directory:
- **Windows**: `%TEMP%/AisStreamer/`
- **Linux/macOS**: `/tmp/AisStreamer/`

Use `--purge-cache` to clear cached files.

## License

See LICENSE file in repository.
