using System;

namespace AisLoader;

public record AisRecord(
    DateTime Timestamp,
    string? TypeOfMobile,
    int Mmsi,
    double Latitude,
    double Longitude,
    string? NavigationalStatus,
    double Rot,
    double Sog,
    double Cog,
    int Heading,
    int? Imo,
    string? Callsign,
    string? Name,
    string? ShipType,
    string? CargoType,
    int? Width,
    int? Length,
    string? TypeOfPositionFixingDevice,
    double? Draught,
    string? Destination,
    string? Eta,
    string? DataSourceType,
    int? DimensionA,
    int? DimensionB,
    int? DimensionC,
    int? DimensionD);
