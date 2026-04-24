using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ticketmasterwpf.Models
{
    public class CinemaHall
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<TheaterRow> Rows { get; set; } = new List<TheaterRow>();
    }
}
