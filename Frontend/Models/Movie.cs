using System;
using System.Collections.Generic;
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

        [JsonPropertyName("imdbRating")]
        public string ImdbRating { get; set; }

        private string _posterUrl;
        [JsonPropertyName("posterUrl")]
        public string PosterUrl
        {
            get => _posterUrl;
            set
            {
                _posterUrl = string.IsNullOrWhiteSpace(value) ? null : value;
                OnPropertyChanged();
            }
        }

        [JsonPropertyName("genre")]
        public string Genre { get; set; }

        public List<Screening> Screenings { get; set; } = new List<Screening>();

        [JsonIgnore]
        public ObservableCollection<string> Showtimes { get; set; } = new ObservableCollection<string>();

        [JsonIgnore]
        public string Duration => $"{DurationMinutes} min";

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("releaseDate")]
        public DateTime ReleaseDate { get; set; }

        private string _backdropUrl;
        [JsonPropertyName("backdropUrl")]
        public string BackdropUrl
        {
            get => _backdropUrl;
            set
            {
                _backdropUrl = string.IsNullOrWhiteSpace(value) ? null : value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public System.Windows.Media.Color PlaceholderColor { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}