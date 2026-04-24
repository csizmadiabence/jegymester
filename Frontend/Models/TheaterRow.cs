using System.Collections.ObjectModel;

namespace ticketmasterwpf.Models
{
    public class TheaterRow
    {
        public int Id { get; set; }
        public int RowNumber { get; set; }
        public ObservableCollection<Seat> Seats { get; set; }
    }
}