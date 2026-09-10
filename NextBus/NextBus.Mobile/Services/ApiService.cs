using NextBus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace NextBus.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "https://localhost:7112/api/";

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public async Task<List<Station>> GetStationsAsync()
        {
            try
            {
                var stations = await _httpClient.GetFromJsonAsync<List<Station>>("stations");
                return stations ?? new List<Station>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching stations: {ex.Message}");
                return new List<Station>();
            }
        }

        public async Task<List<ArrivalRealTime>> GetArrivalsAsync(int stationId)
        {
            try
            {
                var arrivals = await _httpClient.GetFromJsonAsync<List<ArrivalRealTime>>($"stations/{stationId}/arrivals");
                return arrivals ?? new List<ArrivalRealTime>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching arrivals: {ex.Message}");
                return new List<ArrivalRealTime>();
            }
        }

        public async Task<List<RouteSolution>> SearchRoutesAsync(string origin, string destination)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<RouteSolution>>($"routes/search?origin={Uri.EscapeDataString(origin)}&destination={Uri.EscapeDataString(destination)}");
                return response ?? new List<RouteSolution>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching routes: {ex.Message}");
                return new List<RouteSolution>();
            }
        }
    }
}
