using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MetarTaf.Components.Models
{
    public class Metar
    {
        public Altimeter? Altimeter { get; set; }
        public List<Cloud>? Clouds { get; set; }
        [JsonPropertyName("density_altitude")]
        public int DensityAltitude { get; set; }
        public Temperature? Dewpoint { get; set; }
        [JsonPropertyName("flight_rules")]
        public string? FlightRules { get; set; }
        public Meta? Meta { get; set; }
        public List<object>? Other { get; set; }
        [JsonPropertyName("pressure_altitude")]
        public int PressureAltitude { get; set; }
        public string? Raw { get; set; }
        [JsonPropertyName("relative_humidity")]
        public double RelativeHumidity { get; set; }
        public string? Remarks { get; set; }
        [JsonPropertyName("remarks_info")]
        public RemarksInfo? RemarksInfo { get; set; }
        [JsonPropertyName("runway_visibility")]
        public List<object>? RunwayVisibility { get; set; }
        public string? Sanitized { get; set; }
        public string? Station { get; set; }
        public Temperature? Temperature { get; set; }
        public Time? Time { get; set; }
        public Units? Units { get; set; }
        public Visibility? Visibility { get; set; }
        [JsonPropertyName("wind_direction")]
        public WindDirection? WindDirection { get; set; }
        [JsonPropertyName("wind_gust")]
        public object? WindGust { get; set; }
        [JsonPropertyName("wind_speed")]
        public WindSpeed? WindSpeed { get; set; }
        [JsonPropertyName("wind_variable_direction")]
        public List<WindVariableDirection>? WindVariableDirection { get; set; }
        [JsonPropertyName("wx_codes")]
        public List<object>? WxCodes { get; set; }
    }

    public class Altimeter
    {
        public string? Repr { get; set; }
        public string? Spoken { get; set; }
        public double Value { get; set; }
    }

    public class Cloud
    {
        public int Altitude { get; set; }
        public object? Direction { get; set; }
        public object? Modifier { get; set; }
        public string? Repr { get; set; }
        public string? Type { get; set; }
    }

    public class Temperature
    {
        public string? Repr { get; set; }
        public string? Spoken { get; set; }
        public double Value { get; set; }
    }

    public class Time
    {
        public DateTime Dt { get; set; }
        public string? Repr { get; set; }
    }

    public class Visibility
    {
        public string? Repr { get; set; }
        public string? Spoken { get; set; }
        public double Value { get; set; }
    }

    public class WindDirection
    {
        public string? Repr { get; set; }
        public string? Spoken { get; set; }
        public double Value { get; set; }
    }

    public class WindSpeed
    {
        public string? Repr { get; set; }
        public string? Spoken { get; set; }
        public double Value { get; set; }
    }

    public class WindVariableDirection
    {
        public string? Repr { get; set; }
        public string? Spoken { get; set; }
        public double Value { get; set; }
    }

    public class Meta
    {
        [JsonPropertyName("stations_updated")]
        public string? StationsUpdated { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class RemarksInfo
    {
        public List<object>? Codes { get; set; }
        [JsonPropertyName("dewpoint_decimal")]
        public object? DewpointDecimal { get; set; }
        [JsonPropertyName("maximum_temperature_24")]
        public object? MaximumTemperature24 { get; set; }
        [JsonPropertyName("maximum_temperature_6")]
        public object? MaximumTemperature6 { get; set; }
        [JsonPropertyName("minimum_temperature_24")]
        public object? MinimumTemperature24 { get; set; }
        [JsonPropertyName("minimum_temperature_6")]
        public object? MinimumTemperature6 { get; set; }
        [JsonPropertyName("precip_24_hours")]
        public object? Precip24Hours { get; set; }
        [JsonPropertyName("precip_36_hours")]
        public object? Precip36Hours { get; set; }
        [JsonPropertyName("precip_hourly")]
        public object? PrecipHourly { get; set; }
        [JsonPropertyName("pressure_tendency")]
        public object? PressureTendency { get; set; }
        [JsonPropertyName("sea_level_pressure")]
        public object? SeaLevelPressure { get; set; }
        [JsonPropertyName("snow_depth")]
        public object? SnowDepth { get; set; }
        [JsonPropertyName("sunshine_minutes")]
        public object? SunshineMinutes { get; set; }
        [JsonPropertyName("temperature_decimal")]
        public object? TemperatureDecimal { get; set; }
    }

    public class Units
    {
        public string? Accumulation { get; set; }
        public string? Altimeter { get; set; }
        public string? Altitude { get; set; }
        public string? Temperature { get; set; }
        public string? Visibility { get; set; }
        [JsonPropertyName("wind_speed")]
        public string? WindSpeed { get; set; }
    }
}
