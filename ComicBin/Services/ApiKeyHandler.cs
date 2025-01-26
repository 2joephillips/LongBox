using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ComicBin.Services
{
  public interface IApiKeyHandler
  {
    Task<bool> VerifyApiKeyAsync(string apiKey);
  }

  public class ApiKeyHandler : IApiKeyHandler
  {
    private const string apiUrl = "https://comicvine.gamespot.com/api/issues/?api_key=";
    public async Task<bool> VerifyApiKeyAsync(string apiKey)
    {
      var url = $"{apiUrl}{apiKey}&format=json";
      try
      {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ComicBin");
        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
          return true;

        return false;
      }
      catch (Exception ex)
      {
        Console.Write(ex.Message);
        return false;
      }
    }
  }
}
