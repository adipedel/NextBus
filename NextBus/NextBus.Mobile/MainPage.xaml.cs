using NextBus.Mobile.Services;

namespace NextBus.Mobile
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;

        public MainPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        // אירוע שלחיצה על הכפתור מפעילה
        private async void OnLoadStationsClicked(object sender, EventArgs e)
        {
            // הצגת האנימציה של הטעינה
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            LoadStationsBtn.IsEnabled = false;

            // שליפת התחנות מה-API בשרת
            var stations = await _apiService.GetStationsAsync();

            // עדכון הרשימה במסך במידע שהתקבל
            StationsCollectionView.ItemsSource = stations;

            // הסתרת אנימציית הטעינה
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            LoadStationsBtn.IsEnabled = true;
        }
    }
}