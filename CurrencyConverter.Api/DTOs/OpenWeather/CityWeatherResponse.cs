namespace CurrencyConverter.Api.DTOs.OpenWeather
{
    public class CityWeatherResponse
    {
        public string message { get; set; } = null!;
        public string cod { get; set; } = null!;
        public int count { get; set; }
        public List[] list { get; set; }
    }

    public class List
    {
        public int id { get; set; }
        public string name { get; set; } = null!;
        public Coord coord { get; set; }
        public Main main { get; set; }
        public int dt { get; set; }
        public Wind wind { get; set; }
        public Sys sys { get; set; }
        public object rain { get; set; }
        public object snow { get; set; }
        public Clouds clouds { get; set; }
        public Weather[] weather { get; set; }
    }

    public class Coord
    {
        public float lat { get; set; }
        public float lon { get; set; }
    }

    public class Main
    {
        public float temp { get; set; }
        public float feels_like { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public int sea_level { get; set; }
        public int grnd_level { get; set; }
    }

    public class Wind
    {
        public float speed { get; set; }
        public int deg { get; set; }
    }

    public class Sys
    {
        public string country { get; set; } = null!;
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; } = null!;
        public string description { get; set; } = null!;
        public string icon { get; set; } = null!;
    }

}
