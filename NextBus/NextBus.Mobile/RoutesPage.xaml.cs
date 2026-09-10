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

        private async void OnSearchRoutesClicked(object sender, EventArgs e)
        {
            string origin = OriginEntry.Text;
            string destination = DestinationEntry.Text;

            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
            {
                await DisplayAlert("שגיאה", "אנא הזן מוצא ויעד", "אישור");
                return;
            }

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var routes = await _apiService.SearchRoutesAsync(origin, destination);

            RoutesCollectionView.ItemsSource = routes;

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;

            if (routes.Count == 0)
            {
                await DisplayAlert("הודעה", "לא נמצאו מסלולים מתאימים", "אישור");
            }
        }
    }
}