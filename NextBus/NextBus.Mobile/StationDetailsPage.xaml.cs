using NextBus.Mobile.Services;
using NextBus.Shared.Models;

namespace NextBus.Mobile
{
    public partial class StationDetailsPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly Station _station;

        public StationDetailsPage(Station station)
        {
            InitializeComponent();
            _apiService = new ApiService();
            _station = station;

            Title = station.Name;
            StationTitleLabel.Text = $"{station.Name} (קוד {station.StationCode})";

            LoadArrivals();
        }

        private async void LoadArrivals()
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var arrivals = await _apiService.GetArrivalsAsync(_station.StationId);

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;

            if (arrivals.Count > 0)
            {
                ArrivalsCollectionView.ItemsSource = arrivals;
                EmptyLabel.IsVisible = false;
            }
            else
            {
                EmptyLabel.IsVisible = true;
            }
        }
    }
}