namespace NextBus.Mobile.Services
{
    public class LocationService
    {
        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                // בדיקת הרשאות מיקום ובקשתן במידת הצורך
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                if (status != PermissionStatus.Granted)
                {
                    return null; // המשתמש דחה את הבקשה
                }

                // ניסיון שליפה מהיר ממיקום שמור אחרון
                var location = await Geolocation.Default.GetLastKnownLocationAsync();
                if (location != null && (DateTimeOffset.UtcNow - location.Timestamp).TotalMinutes < 2)
                {
                    return location;
                }

                // דגימת מיקום מדויק ועכשווי (עד 8 שניות המתנה)
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(8));
                return await Geolocation.Default.GetLocationAsync(request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocationService Error]: {ex.Message}");
                return null;
            }
        }
    }
}