using System.Collections.ObjectModel;
using System.Net.Http.Json;
using NextBus.Shared.Models;

namespace NextBus.Mobile.Views
{
    public partial class StationDetailsPage : ContentPage
    {
        private readonly string _stopCode;
        private readonly HttpClient _httpClient;
        private IDispatcherTimer? _refreshTimer;

        public ObservableCollection<ArrivalRealTime> Arrivals { get; set; } = new();

        public StationDetailsPage(Station station)
        {
            InitializeComponent();

            _stopCode = station.StationCode?.Trim() ?? "";
            StationTitleLabel.Text = station.Name;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(6) };

            ArrivalsCollectionView.ItemsSource = Arrivals;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await FetchArrivalsAsync();

            // רענון אוטומטי כל 20 שניות
            _refreshTimer = Dispatcher.CreateTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(20);
            _refreshTimer.Tick += async (s, e) => await FetchArrivalsAsync();
            _refreshTimer.Start();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _refreshTimer?.Stop();
            _refreshTimer = null;
        }

        private async Task FetchArrivalsAsync()
        {
            if (string.IsNullOrWhiteSpace(_stopCode)) return;

            // הפעלת חיווי טעינה עדין
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingSpinner.IsVisible = true;
                LoadingSpinner.IsRunning = true;
            });

            try
            {
                string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                    ? "http://10.0.2.2:5281"
                    : "http://localhost:5281";

                string url = $"{baseUrl}/api/Stations/{_stopCode}/realtime";

                var list = await _httpClient.GetFromJsonAsync<List<ArrivalRealTime>>(url);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Arrivals.Clear();
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            Arrivals.Add(item);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NextBus Error] {ex.Message}");
            }
            finally
            {
                // כיבוי והעלמת הגלגל ב-UI Thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadingSpinner.IsRunning = false;
                    LoadingSpinner.IsVisible = false;
                });
            }
        }
    }
}