using NextBus.Mobile.Services;
using NextBus.Shared.Models;
using NextBus.Mobile.Views;

namespace NextBus.Mobile
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly LocationService _locationService;
        private List<Station> _allStations = new();

        // מיקום ברירת מחדל (מרכז תל אביב) במקרה שאין הרשאה או GPS כבוי
        private double _currentUserLat = 32.0853;
        private double _currentUserLon = 34.7818;

        public MainPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _locationService = new LocationService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_allStations.Count == 0)
            {
                await LoadAndFilterStationsAsync();
            }
        }

        private async Task LoadAndFilterStationsAsync()
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            // 1. שליפת מיקום GPS אמיתי של המכשיר
            var location = await _locationService.GetCurrentLocationAsync();
            if (location != null)
            {
                _currentUserLat = location.Latitude;
                _currentUserLon = location.Longitude;
            }

            // 2. קבלת התחנות מה-API
            _allStations = await _apiService.GetStationsAsync();

            // 3. חישוב מרחק המשתמש מכל תחנה לפי המיקום הנוכחי שנשלף
            foreach (var station in _allStations)
            {
                station.DistanceInMeters = CalculateDistance(_currentUserLat, _currentUserLon, station.Latitude, station.Longitude);
            }

            // 4. הצגת תחנות קרובות (עד 600 מטר)
            var nearbyStations = _allStations
                .Where(s => s.DistanceInMeters <= 600)
                .OrderBy(s => s.DistanceInMeters)
                .ToList();

            StationsCollectionView.ItemsSource = nearbyStations.Count > 0
                ? nearbyStations
                : _allStations.OrderBy(s => s.DistanceInMeters).Take(20).ToList();

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string query = e.NewTextValue?.Trim().ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(query))
            {
                var nearby = _allStations
                    .Where(s => s.DistanceInMeters <= 600)
                    .OrderBy(s => s.DistanceInMeters)
                    .ToList();

                StationsCollectionView.ItemsSource = nearby.Count > 0
                    ? nearby
                    : _allStations.OrderBy(s => s.DistanceInMeters).Take(20).ToList();
                return;
            }

            // סינון תוך כדי הקלדה לפי שם או קוד תחנה
            var filtered = _allStations
                .Where(s => s.Name.ToLower().Contains(query) || (s.StationCode != null && s.StationCode.Contains(query)))
                .OrderBy(s => s.DistanceInMeters)
                .ToList();

            StationsCollectionView.ItemsSource = filtered;
        }

        private async void OnStationSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedStation = e.CurrentSelection.FirstOrDefault() as Station;
            if (selectedStation == null)
                return;

            ((CollectionView)sender).SelectedItem = null;
            await Navigation.PushAsync(new StationDetailsPage(selectedStation));
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double r = 6371e3; // רדיוס כדור הארץ במטרים
            double phi1 = lat1 * Math.PI / 180;
            double phi2 = lat2 * Math.PI / 180;
            double deltaPhi = (lat2 - lat1) * Math.PI / 180;
            double deltaLambda = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) *
                       Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return r * c;
        }
    }
}