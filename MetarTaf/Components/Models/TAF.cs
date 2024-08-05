namespace MetarTaf.Components.Models
{
    public class TAF
    {
            public Meta? meta { get; set; }
            public string? raw { get; set; }
            public string? station { get; set; }
            public Time? time { get; set; }
            public string? remarks { get; set; }
            public Forecast[]? forecast { get; set; }
            public Start_Time? start_time { get; set; }
            public End_Time? end_time { get; set; }
            public string? max_temp { get; set; }
            public string? min_temp { get; set; }
            public object? alts { get; set; }
            public object? temps { get; set; }
            public Units? units { get; set; }


        public class Meta
        {
            public string? timestamp { get; set; }
        }

        public class Time
        {
            public string? repr { get; set; }
            public DateTime? dt { get; set; }
        }

        public class Start_Time
        {
            public string? repr { get; set; }
            public string? dt { get; set; }
        }

        public class End_Time
        {
            public string? repr { get; set; }
            public string? dt { get; set; }
        }

        public class Units
        {
            public string? altimeter { get; set; }
            public string? altitude { get; set; }
            public string? temperature { get; set; }
            public string? visibility { get; set; }
            public string? wind_speed { get; set; }
        }

        public class Forecast
        {
            public string? altimeter { get; set; }
            public Cloud[]? clouds { get; set; }
            public string? flight_rules { get; set; }
            public object[]? other { get; set; }
            public string? sanitized { get; set; }
            public Visibility? visibility { get; set; }
            public Wind_Direction? wind_direction { get; set; }
            public Wind_Gust? wind_gust { get; set; }
            public Wind_Speed? wind_speed { get; set; }
            public Wx_Codes[]? wx_codes { get; set; }
            public End_Time1? end_time { get; set; }
            public object[]? icing { get; set; }
            public object? probability { get; set; }
            public string? raw { get; set; }
            public Start_Time1? start_time { get; set; }
            public object[]? turbulence { get; set; }
            public string? type { get; set; }
            public object? wind_shear { get; set; }
            public string? summary { get; set; }
        }

        public class Visibility
        {
            public string? repr { get; set; }
            public object? value { get; set; }
            public string? spoken { get; set; }
        }

        public class Wind_Direction
        {
            public string? repr { get; set; }
            public int? value { get; set; }
            public string? spoken { get; set; }
        }

        public class Wind_Gust
        {
            public string? repr { get; set; }
            public int? value { get; set; }
            public string? spoken { get; set; }
        }

        public class Wind_Speed
        {
            public string? repr { get; set; }
            public int? value { get; set; }
            public string? spoken { get; set; }
        }

        public class End_Time1
        {
            public string? repr { get; set; }
            public string? dt { get; set; }
        }

        public class Start_Time1
        {
            public string? repr { get; set; }
            public string? dt { get; set; }
        }

        public class Cloud
        {
            public string? repr { get; set; }
            public string? type { get; set; }
            public int? altitude { get; set; }
            public object? modifier { get; set; }
            public object? direction { get; set; }
        }

        public class Wx_Codes
        {
            public string? repr { get; set; }
            public string? value { get; set; }
        }



    }
}
