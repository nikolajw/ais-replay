# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**AisReplay** is a C# .NET application that replays Automatic Identification System (AIS) vessel tracking data. It reads AIS records from CSV files or downloads them from [aisdata.ais.dk](http://aisdata.ais.dk/), converts them to marine communication standards (NMEA 0183 or GPS GPRMC), and broadcasts them via UDP to `localhost:10110` for integration with marine navigation software.

## Architecture

The application follows a simple, single-file pipeline architecture:

- **Program.cs** (top-level statements)
  - CLI argument parsing (--file, --date, --mmsi, --x-speed, --gps, --skip-moored, --purge-cache)
  - CSV file reading or HTTP download with ZIP extraction and caching
  - Record filtering (by MMSI, moored status)
  - Playback speed control and timing
  - UDP broadcasting to `127.0.0.1:10110`

- **AisRecord.cs**
  - C# record (immutable) representing a single AIS position report
  - Fields: Timestamp, MMSI, Latitude, Longitude, NavigationalStatus, ROT, SOG, COG, Heading

- **CsvParser.cs**
  - Static utility parsing CSV lines into AisRecord objects
  - Parses timestamps in `dd/MM/yyyy HH:mm:ss` format
  - Graceful handling of missing/invalid numeric values (returns 0 or -1)

- **NmeaEncoder.cs**
  - Static utility for encoding AIS records into marine communication formats
  - `ToNmea0183()`: Encodes Type 1 Position Report (168-bit payload) into NMEA 0183 sentence with checksum
  - `ToGprmc()`: Encodes records into GPS GPRMC (Recommended Minimum) format
  - Bit-packing logic for NMEA encoding with AIS armoring (ASCII-encoded base 64)
  - Navigation status mapping (e.g., "under way using engine" → code 0)

## Common Commands

```bash
# Build the project
dotnet build

# Build in Release configuration
dotnet build --configuration Release

# Run with local CSV file
dotnet run -- --file shipdata.csv

# Run with all options
dotnet run -- --file shipdata.csv --mmsi 220382000 --x-speed 10 --skip-moored --gps

# Download and replay data from specific date
dotnet run -- --date 2024-01-15

# Clear cached downloads
dotnet run -- --purge-cache

# Publish for distribution (Linux, macOS, Windows)
dotnet publish --configuration Release --runtime linux-x64 --self-contained --output ./publish/linux-x64
```

## Key Implementation Details

**CSV Parsing**
- Input format: `Timestamp,Vessel Name,MMSI,Latitude,Longitude,NavigationalStatus,ROT,SOG,COG,Heading`
- Timestamp parsing is strict: `dd/MM/yyyy HH:mm:ss` (use `CultureInfo.InvariantCulture`)
- Name field (index 1) is parsed but not used currently

**NMEA 0183 Type 1 Encoding**
- 168-bit payload (28 bytes) including:
  - Message Type (6 bits) = 1
  - Repeat Indicator (2 bits)
  - MMSI (30 bits)
  - Status Code (4 bits, mapped from navigational status string)
  - ROT (8 bits, signed, encoded as √(ROT/4.733))
  - SOG (10 bits, 0.1 knot resolution)
  - Longitude/Latitude (28/27 bits, 1/600000 degree resolution)
  - COG (12 bits, 0.1 degree resolution)
  - Heading (9 bits, true heading or 511 if unavailable)
  - Spare bits and second timestamp
- Payload encoded with "armoring" (ASCII-friendly 6-bit encoding)
- Final format: `!AIVDM,1,1,,A,<payload>,<fillBits>*<checksum>`

**GPS GPRMC Format**
- Standard GPS format: `$GPRMC,time,status,lat,N/S,lon,E/W,sog,cog,date,magvar,magvar_dir*checksum`
- Coordinates in `DDMM.MMMM` format (degrees and decimal minutes)

**Data Flow**
1. Parse arguments
2. Load or download CSV
3. Read line-by-line (memory efficient)
4. Filter records (skip moored, MMSI filter)
5. Apply playback speed (sleep between records)
6. Encode to NMEA/GPRMC
7. Send via UDP
8. Print record to console

## Release Process

Automated via GitHub Actions on push to `main`:
1. Builds in Release configuration
2. Publishes self-contained binaries for Linux (x64), macOS (x64), Windows (x64)
3. Creates tar.gz and zip archives
4. Creates GitHub release with version tag: `v<YYYY.MM.DD>-<short-commit-sha>`
5. Uploads archives as release artifacts

## Complete Release Workflow

When releasing a new version (from code change to Homebrew update):

### 1. Make Code Changes and Commit
```bash
# Make your code changes
# Test locally to verify
git add <files>
git commit -m "Fix/Feature description"
git push origin main
```

### 2. Bump Version Numbers
```bash
# Update version in both projects
sed -i '' 's/<Version>OLD<\/Version>/<Version>NEW<\/Version>/g' \
  src/AisReplay/AisReplay.csproj src/AisLoader/AisLoader.csproj

# Update ArgsParser.cs version method
# Change: System.Console.WriteLine("AisLoader OLD");
# To:     System.Console.WriteLine("AisLoader NEW");
```

### 3. Commit Version Bump
```bash
git add src/AisReplay/AisReplay.csproj src/AisLoader/AisLoader.csproj src/AisLoader/ArgsParser.cs
git commit -m "Bump version to X.Y.Z"
```

### 4. Create and Push Tag
```bash
git tag -a vX.Y.Z -m "Release X.Y.Z - description"
git push origin main vX.Y.Z
```

### 5. Wait for GitHub Actions
- Wait for release artifacts to be created on GitHub
- Verify all 4 artifacts exist (linux-x64, macos-arm64, macos-x64, windows-x64)

### 6. Get SHA256 Hashes (via gh CLI, no download needed)
```bash
gh release view vX.Y.Z --repo nikolajw/ais-replay --json assets \
  --jq '.assets[] | select(.name | contains("macos")) | "\(.name): \(.digest)"'
```

### 7. Update Homebrew Formulas
In `/Users/nikolajw/Repositories/homebrew-aisreplay/Formula/`:

Update **aisloader.rb**:
- Change `version` from old to new
- Update both macOS URLs to reference new tag (vX.Y.Z)
- Update both SHA256 hashes with values from step 6

Update **aisreplay.rb**:
- Same changes as aisloader.rb

### 8. Commit and Push Homebrew Changes
```bash
cd homebrew-aisreplay
git add Formula/aisloader.rb Formula/aisreplay.rb
git commit -m "Update Homebrew formulas to vX.Y.Z"
git push origin main
```

## Testing Notes

- No formal test project; manual testing with CSV files is the primary validation method
- Can test locally with `--file` argument pointing to sample CSV
- UDP output can be monitored with tools like `nc -u -l 10110` (macOS/Linux) or Wireshark
- Test with both real AIS data and synthetic CSV files to validate encoding

## .NET Configuration

- **Target Framework**: net10.0 (requires .NET 10.0+)
- **Nullable**: Enabled (strict null-checking)
- **Type**: Console executable
- **Language Features**: C# top-level statements, records, local functions, newer target-typed new expressions

## Cache Location

- Downloaded files cached in: `%TEMP%/AisReplay/` (Windows) or `/tmp/AisReplay/` (Linux/macOS)
- Use `--purge-cache` to clear cached files
