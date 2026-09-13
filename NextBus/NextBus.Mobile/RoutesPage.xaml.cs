using NextBus.Mobile.Services;

namespace NextBus.Mobile
{
    public partial class RoutesPage : ContentPage
    {
        private readonly ApiService _apiService;

        public RoutesPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private async void OnFindRouteClicked(object sender, EventArgs e)
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            try
            {
                var fromCoords = OriginEntry.Text.Split(',');
                var toCoords = DestinationEntry.Text.Split(',');

                if (fromCoords.Length == 2 && toCoords.Length == 2 &&
                    double.TryParse(fromCoords[0].Trim(), out double fromLat) &&
                    double.TryParse(fromCoords[1].Trim(), out double fromLon) &&
                    double.TryParse(toCoords[0].Trim(), out double toLat) &&
                    double.TryParse(toCoords[1].Trim(), out double toLon))
                {
                    var routes = await _apiService.PlanRouteAsync(fromLat, fromLon, toLat, toLon);
                    RoutesCollectionView.ItemsSource = routes;

                    if (routes == null || routes.Count == 0)
                    {
                        await DisplayAlert("הודעה", "לא נמצאו מסלולים מתאימים עבור מיקומים אלו", "אישור");
                    }
                }
                else
                {
                    await DisplayAlert("שגיאה", "פורמט הקואורדינטות אינו תקין (נדרש: רוחב, אורך)", "אישור");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("שגיאה", $"אירעה תקלה: {ex.Message}", "אישור");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }
    }
}