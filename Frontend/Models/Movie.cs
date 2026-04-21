using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace ticketmasterwpf.Models
{
    public class Movie : INotifyPropertyChanged
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        private string _title;
        [JsonPropertyName("title")]
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("durationMinutes")]
        public int DurationMinutes { get; set; }

        // --- ÚJ IMDB MEZŐK ---
        [JsonPropertyName("imdbRating")]
        public string ImdbRating { get; set; }

        [JsonPropertyName("posterUrl")]
        public string PosterUrl { get; set; }

        [JsonPropertyName("genre")]
        public string Genre { get; set; }

        [JsonIgnore]
        public ObservableCollection<string> Showtimes { get; set; } = new ObservableCollection<string>();

        [JsonIgnore]
        public string Duration => $"{DurationMinutes} min";

        public string Status { get; set; }

        [JsonIgnore]
        public System.Windows.Media.Color PlaceholderColor { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}