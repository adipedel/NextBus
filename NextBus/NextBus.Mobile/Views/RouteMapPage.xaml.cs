using NextBus.Shared.Models;

namespace NextBus.Mobile.Views
{
    public partial class RouteMapPage : ContentPage
    {
        public RouteMapPage(RouteSolution route, double originLat, double originLon, double destLat, double destLon)
        {
            InitializeComponent();

            MapTitleLabel.Text = route.IsTransfer
                ? $"מסלול: קו {route.FirstLineNumber} ← קו {route.SecondLineNumber}"
                : $"מסלול: קו {route.LineNumber}";

            LoadMap(route, originLat, originLon, destLat, destLon);
        }

        private void LoadMap(RouteSolution route, double originLat, double originLon, double destLat, double destLon)
        {
            // בדיקת קיום קואורדינטות תחנות (במידה ועדיין לא אוכלסו ב-API, נשתמש בברירת מחדל של הנקודות)
            double boardLat = route.OriginStationLat != 0 ? route.OriginStationLat : originLat;
            double boardLon = route.OriginStationLon != 0 ? route.OriginStationLon : originLon;

            double dropLat = route.DestinationStationLat != 0 ? route.DestinationStationLat : destLat;
            double dropLon = route.DestinationStationLon != 0 ? route.DestinationStationLon : destLon;

            bool hasTransfer = route.IsTransfer && route.TransferStationLat.HasValue && route.TransferStationLon.HasValue;
            double transferLat = route.TransferStationLat ?? 0;
            double transferLon = route.TransferStationLon ?? 0;

            string htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <style>
        html, body, #map {{
            margin: 0;
            padding: 0;
            height: 100%;
            width: 100%;
            background-color: #121212;
        }}
        .leaflet-tile-pane {{
            filter: brightness(0.6) invert(1) contrast(3) hue-rotate(200deg) saturate(0.3) brightness(0.7);
        }}
        .leaflet-popup-content-wrapper {{
            background: #1E293B;
            color: #FFFFFF;
            font-family: sans-serif;
            text-align: right;
            direction: rtl;
            border-radius: 8px;
            box-shadow: 0 4px 6px -1px rgba(0,0,0,0.5);
        }}
        .leaflet-popup-tip {{
            background: #1E293B;
        }}
        /* סמנים עגולים בעיצוב מודרני */
        .custom-pin {{
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 50%;
            color: white;
            font-size: 13px;
            font-weight: bold;
            box-shadow: 0 2px 5px rgba(0,0,0,0.6);
            border: 2px solid white;
        }}
        .pin-origin {{ background-color: #64748B; }}
        .pin-dest {{ background-color: #22C55E; }}
        .pin-bus {{ background-color: #0284C7; }}
        .pin-transfer {{ background-color: #7C3AED; }}
    </style>
</head>
<body>
    <div id='map'></div>
    <script>
        var map = L.map('map', {{ zoomControl: true }}).setView([{originLat}, {originLon}], 13);

        L.tileLayer('https://tile.openstreetmap.de/{{z}}/{{x}}/{{y}}.png', {{
            maxZoom: 18,
            attribution: '&copy; OpenStreetMap contributors'
        }}).addTo(map);

        // פונקציית עזר ליצירת אייקון מותאם
        function createIcon(className, text) {{
            return L.divIcon({{
                className: '',
                html: '<div class=""custom-pin ' + className + '"" style=""width:28px;height:28px;"">' + text + '</div>',
                iconSize: [28, 28],
                iconAnchor: [14, 14],
                popupAnchor: [0, -14]
            }});
        }}

        var allPoints = [];

        // 1. נקודת מוצא
        L.marker([{originLat}, {originLon}], {{ icon: createIcon('pin-origin', '🚶') }})
            .addTo(map)
            .bindPopup('<b>נקודת מוצא</b>');
        allPoints.push([{originLat}, {originLon}]);

        // 2. מסלול הליכה לתחנת העלייה (קו מקווקו אפור/צהבהב)
        var walkToStation = [
            [{originLat}, {originLon}],
            [{boardLat}, {boardLon}]
        ];
        L.polyline(walkToStation, {{
            color: '#FBBF24',
            weight: 3,
            dashArray: '5, 8',
            opacity: 0.9
        }}).addTo(map).bindPopup('הליכה לתחנה ({route.WalkToStationMinutes} דק׳)');

        // 3. תחנת עלייה לאוטובוס
        L.marker([{boardLat}, {boardLon}], {{ icon: createIcon('pin-bus', '🚏') }})
            .addTo(map)
            .bindPopup('<b>תחנת עלייה:</b><br/>{EscapeJs(route.OriginStationName)}');
        allPoints.push([{boardLat}, {boardLon}]);

        {(hasTransfer ? $@"
        // 4. מסלול נסיעה קו ראשון להחלפה (כחול)
        L.polyline([
            [{boardLat}, {boardLon}],
            [{transferLat}, {transferLon}]
        ], {{
            color: '#0284C7',
            weight: 5,
            opacity: 0.9
        }}).addTo(map).bindPopup('נסיעה בקו {route.FirstLineNumber}');

        // תחנת החלפה (סגול)
        L.marker([{transferLat}, {transferLon}], {{ icon: createIcon('pin-transfer', '🔄') }})
            .addTo(map)
            .bindPopup('<b>תחנת החלפה:</b><br/>{EscapeJs(route.TransferStationName)}');
        allPoints.push([{transferLat}, {transferLon}]);

        // מסלול נסיעה קו שני (סגול)
        L.polyline([
            [{transferLat}, {transferLon}],
            [{dropLat}, {dropLon}]
        ], {{
            color: '#7C3AED',
            weight: 5,
            opacity: 0.9
        }}).addTo(map).bindPopup('נסיעה בקו {route.SecondLineNumber}');
        " : $@"
        // 4. מסלול נסיעה ישיר (כחול)
        L.polyline([
            [{boardLat}, {boardLon}],
            [{dropLat}, {dropLon}]
        ], {{
            color: '#0284C7',
            weight: 5,
            opacity: 0.9
        }}).addTo(map).bindPopup('נסיעה בקו {route.LineNumber}');
        ")}

        // 5. תחנת ירידה
        L.marker([{dropLat}, {dropLon}], {{ icon: createIcon('pin-bus', '🚏') }})
            .addTo(map)
            .bindPopup('<b>תחנת ירידה:</b><br/>{EscapeJs(route.DestinationStationName)}');
        allPoints.push([{dropLat}, {dropLon}]);

        // 6. מסלול הליכה מהתחנה ליעד (קו מקווקו צהבהב)
        var walkToDest = [
            [{dropLat}, {dropLon}],
            [{destLat}, {destLon}]
        ];
        L.polyline(walkToDest, {{
            color: '#FBBF24',
            weight: 3,
            dashArray: '5, 8',
            opacity: 0.9
        }}).addTo(map).bindPopup('הליכה ליעד ({route.WalkFromStationMinutes} דק׳)');

        // 7. יעד סופי
        L.marker([{destLat}, {destLon}], {{ icon: createIcon('pin-dest', '🏁') }})
            .addTo(map)
            .bindPopup('<b>יעד סופי</b>');
        allPoints.push([{destLat}, {destLon}]);

        // התאמת הזום להכל
        var bounds = L.latLngBounds(allPoints);
        map.fitBounds(bounds, {{ padding: [50, 50] }});
    </script>
</body>
</html>";

            MapWebView.Source = new HtmlWebViewSource
            {
                Html = htmlContent
            };
        }

        private static string EscapeJs(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Replace("'", "\\'").Replace("\"", "\\\"").Replace("\n", " ");
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}