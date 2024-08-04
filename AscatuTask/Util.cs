using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;

namespace AscatuTask
{
    public class Util
    {
        public static async Task CheckPersonIdAsync(HttpClient httpClient, string personId)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"http://172.21.0.5:8080/api/v1/person/{personId}");
            if (!(response.IsSuccessStatusCode))
            {
                throw new InvalidOperationException($"Failed to retrieve person data.");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var person = JsonSerializer.Deserialize<Person>(jsonResponse);

            if (person == null )
            {
                throw new InvalidOperationException("Invalid person ID.");
            }
        }
    }
}
