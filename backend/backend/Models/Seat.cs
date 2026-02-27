using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace backend.Models;

public class Seat
{
    public int Row { get; set; }
    public int Number { get; set; }
    public bool IsHidden { get; set; } // A folyosóhoz

    private bool _isOccupied;
    public bool IsOccupied
    {
        get => _isOccupied;
        set { _isOccupied = value; OnPropertyChanged(); }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// Egy teljes sor (számokkal és székekkel)
public class TheaterRow
{
    public int RowNumber { get; set; }
    public ObservableCollection<Seat> Seats { get; set; }
}
