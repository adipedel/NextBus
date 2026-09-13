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

        public RoutesPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _geocodingService = new GeocodingService();
        }

        private async void OnOriginTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length < 3)
            {
                OriginSuggestionsView.IsVisible = false;
                return;
            }

            var predictions = await _geocodingService.SearchPlacesAsync(e.NewTextValue);
            OriginSuggestionsView.ItemsSource = predictions;
            OriginSuggestionsView.IsVisible = predictions.Count > 0;
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

                OriginSuggestionsView.IsVisible = false;
                OriginSuggestionsView.SelectedItem = null;
            }
        }

        private async void OnDestinationTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length < 3)
            {
                DestinationSuggestionsView.IsVisible = false;
                return;
            }

            var predictions = await _geocodingService.SearchPlacesAsync(e.NewTextValue);
            DestinationSuggestionsView.ItemsSource = predictions;
            DestinationSuggestionsView.IsVisible = predictions.Count > 0;
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

                DestinationSuggestionsView.IsVisible = false;
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