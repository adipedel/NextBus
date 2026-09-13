using NextBus.Mobile.Models;
using NextBus.Mobile.Services;

namespace NextBus.Mobile
{
    public partial class RoutesPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly GeocodingService _geocodingService;

        private double? _originLat;
        private double? _originLon;
        private double? _destLat;
        private double? _destLon;
        private CancellationTokenSource? _originCts;
        private CancellationTokenSource? _destCts;

        public RoutesPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _geocodingService = new GeocodingService();

            // חיבור אירועי ההקלדה ישירות בקוד כדי להבטיח זיהוי ב-Windows
            OriginSearchEntry.TextChanged += OnOriginTextChanged;
            DestinationSearchEntry.TextChanged += OnDestinationTextChanged;
        }

        private async void OnSearchOriginClicked(object sender, EventArgs e)
        {
            await TriggerSearch(OriginSearchEntry.Text, OriginSuggestionsView, OriginSuggestionsBorder);
        }

        private async void OnSearchDestClicked(object sender, EventArgs e)
        {
            await TriggerSearch(DestinationSearchEntry.Text, DestinationSuggestionsView, DestinationSuggestionsBorder);
        }

        private async void OnOriginTextChanged(object sender, TextChangedEventArgs e)
        {
            _originCts?.Cancel();
            _originCts = new CancellationTokenSource();
            var token = _originCts.Token;

            var text = e.NewTextValue;
            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 2)
            {
                OriginSuggestionsBorder.IsVisible = false;
                return;
            }

            try
            {
                // ממתין 400ms - אם המשתמש ממשיך להקליד, הבקשה מתבטלת ולא נשלחת
                await Task.Delay(400, token);

                var predictions = await _geocodingService.SearchPlacesAsync(text, token);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OriginSuggestionsView.ItemsSource = predictions;
                    OriginSuggestionsBorder.IsVisible = predictions.Count > 0;
                });
            }
            catch (OperationCanceledException) { }
        }

        private async void OnDestinationTextChanged(object sender, TextChangedEventArgs e)
        {
            _destCts?.Cancel();
            _destCts = new CancellationTokenSource();
            var token = _destCts.Token;

            var text = e.NewTextValue;
            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 2)
            {
                DestinationSuggestionsBorder.IsVisible = false;
                return;
            }

            try
            {
                await Task.Delay(400, token);

                var predictions = await _geocodingService.SearchPlacesAsync(text, token);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DestinationSuggestionsView.ItemsSource = predictions;
                    DestinationSuggestionsBorder.IsVisible = predictions.Count > 0;
                });
            }
            catch (OperationCanceledException) { }
        }

        private async Task TriggerSearch(string text, CollectionView view, Border border)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 2)
            {
                MainThread.BeginInvokeOnMainThread(() => border.IsVisible = false);
                return;
            }

            var predictions = await _geocodingService.SearchPlacesAsync(text);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                view.ItemsSource = predictions;
                border.IsVisible = predictions != null && predictions.Count > 0;
            });
        }

        private void OnOriginSuggestionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is PlacePrediction selected)
            {
                _originLat = selected.Latitude;
                _originLon = selected.Longitude;

                OriginSearchEntry.TextChanged -= OnOriginTextChanged;
                OriginSearchEntry.Text = selected.DisplayName;
                OriginSearchEntry.TextChanged += OnOriginTextChanged;

                OriginSuggestionsBorder.IsVisible = false;
                OriginSuggestionsView.SelectedItem = null;
            }
        }

        private void OnDestinationSuggestionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is PlacePrediction selected)
            {
                _destLat = selected.Latitude;
                _destLon = selected.Longitude;

                DestinationSearchEntry.TextChanged -= OnDestinationTextChanged;
                DestinationSearchEntry.Text = selected.DisplayName;
                DestinationSearchEntry.TextChanged += OnDestinationTextChanged;

                DestinationSuggestionsBorder.IsVisible = false;
                DestinationSuggestionsView.SelectedItem = null;
            }
        }

        
        private async void OnFindRouteClicked(object sender, EventArgs e)
        {
            if (_originLat == null || _originLon == null || _destLat == null || _destLon == null)
            {
                await DisplayAlert("חסר מידע", "נא לבחור כתובת מוצא ויעד מתוך רשימת ההצעות", "אישור");
                return;
            }

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            RoutesCollectionView.ItemsSource = null;

            try
            {
                var routes = await _apiService.PlanRouteAsync(_originLat.Value, _originLon.Value, _destLat.Value, _destLon.Value);
                RoutesCollectionView.ItemsSource = routes;

                if (routes == null || routes.Count == 0)
                {
                    await DisplayAlert("הודעה", "לא נמצאו מסלולים בין נקודות אלו בקרבת התחנות הקיימות", "אישור");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("שגיאה", $"אירעה תקלה בחיפוש: {ex.Message}", "אישור");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}