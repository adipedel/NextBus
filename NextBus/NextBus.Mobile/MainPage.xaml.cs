using NextBus.Mobile.Services;
using NextBus.Shared.Models;

namespace NextBus.Mobile
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;
        private List<Station> _allStations = new();

        // מיקום נוכחי לדוגמה (מרכז תל אביב) לצורך פיתוח
        private const double UserLat = 32.0853;
        private const double UserLon = 34.7818;

        public MainPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
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

            _allStations = await _apiService.GetStationsAsync();

            // חישוב מרחק המשתמש מכל תחנה
            foreach (var station in _allStations)
            {
                station.DistanceInMeters = CalculateDistance(UserLat, UserLon, station.Latitude, station.Longitude);
            }

            // הצגת תחנות קרובות ברדיוס של עד 500 מטר כברירת מחדל
            var nearbyStations = _allStations
                .Where(s => s.DistanceInMeters <= 500)
                .OrderBy(s => s.DistanceInMeters)
                .ToList();

            // אם אין תחנות בטווח 500 מטר, מציגים את כל התחנות ממוינות לפי מרחק
            StationsCollectionView.ItemsSource = nearbyStations.Count > 0
                ? nearbyStations
                : _allStations.OrderBy(s => s.DistanceInMeters).ToList();

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string query = e.NewTextValue?.Trim().ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(query))
            {
                // אם שורת החיפוש ריקה, חוזרים לתחנות הקרובות ביותר
                var nearby = _allStations
                    .Where(s => s.DistanceInMeters <= 500)
                    .OrderBy(s => s.DistanceInMeters)
                    .ToList();

                StationsCollectionView.ItemsSource = nearby.Count > 0
                    ? nearby
                    : _allStations.OrderBy(s => s.DistanceInMeters).ToList();
                return;
            }

            // סינון תוך כדי הקלדה לפי שם או קוד תחנה
            var filtered = _allStations
                .Where(s => s.Name.ToLower().Contains(query) || s.StationCode.ToString().Contains(query))
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

        // חישוב מרחק בקירוב במטרים לפי נוסחת Haversine
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