using System;

namespace AisStreamer;

public record AisRecord(
    DateTime Timestamp,
    int Mmsi,
    double Latitude,
    double Longitude,
    string? NavigationalStatus,
    double Rot,
    double Sog,
    double Cog,
    int Heading)
{
    public static AisRecord None => new(DateTime.MinValue, 0, 0D, 0D, "",  0D, 0D, 0D, 0);
}
