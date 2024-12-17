namespace PostService.Services
{
    public class IDValidationService
    {
        private readonly HttpClient _httpClient;

        public IDValidationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ValidateIDAsync(string userId)
        {
            try
            {
                Console.WriteLine($"Making request to {_httpClient.BaseAddress}validate-id/{userId}");

                var response = await _httpClient.GetAsync($"validate-id/{userId}");

                Console.WriteLine($"Response Status Code: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {content}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating ID: {ex.Message}");
                return false;
            }
        }
    }
}
