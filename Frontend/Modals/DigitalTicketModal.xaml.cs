using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using QRCoder;

namespace ticketmasterwpf.Modals
{
    public partial class DigitalTicketModal : UserControl, INotifyPropertyChanged
    {
        public string SelectedMovieTitle { get; set; }
        public string SelectedShowtime { get; set; }
        public string SelectedRoomName { get; set; }
        public string SeatInfo { get; set; }
        public string TicketId { get; set; }
        public string SelectedMoviePoster { get; set; }
        public string TotalAmount { get; set; }
        public bool IsNewPurchase { get; set; } = false;

        private BitmapImage _qrCodeImage;
        public BitmapImage QRCodeImage
        {
            get => _qrCodeImage;
            set { _qrCodeImage = value; OnPropertyChanged(); }
        }

        public DigitalTicketModal(string movieTitle, string showtime, string roomName, string seatInfo, string ticketId, string moviePoster, string totalAmount,bool isNewPurchase = false)
        {
            InitializeComponent();

            SelectedMovieTitle = movieTitle;
            SelectedShowtime = showtime;
            SelectedRoomName = roomName;
            SeatInfo = seatInfo;
            TicketId = ticketId;
            SelectedMoviePoster = moviePoster;
            TotalAmount = totalAmount;

            GenerateQRCode(ticketId);

            this.IsNewPurchase = isNewPurchase;
            this.DataContext = this;
        }

        private void GenerateQRCode(string payload)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);
                BitmapImage bitmap = new BitmapImage();
                using (var stream = new MemoryStream(qrCodeAsPngByteArr))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                }
                QRCodeImage = bitmap;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            mainWindow?.HideModal();

            if (this.IsNewPurchase && mainWindow?.MainFrame != null)
            {
                mainWindow.MainFrame.Navigate(new HomePage());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}