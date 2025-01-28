using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ComicBin.Services;


public class ApiKeyValidationResult
{
  public bool IsValid { get; }
  public string Message { get; }
  public ApiKeyValidationResult(bool isValid, string message)
  {
    IsValid = isValid;
    Message = message;
  }
}

public interface IApiKeyHandler
{
  Task<ApiKeyValidationResult> VerifyApiKeyAsync(string apiKey);
   Task OpenComicVineSite();
}

public class ApiKeyHandler : IApiKeyHandler
{
  private const string apiUrl = "https://comicvine.gamespot.com/api/issues/?api_key=";
  public async Task<ApiKeyValidationResult> VerifyApiKeyAsync(string apiKey)
  {
    var url = $"{apiUrl}{apiKey}&format=json";
    try
    {
      using HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Add("User-Agent", "ComicBin");
      HttpResponseMessage response = await client.GetAsync(url);

      if (response.IsSuccessStatusCode)
        return new ApiKeyValidationResult(true, "Api key is valid.");

      return new ApiKeyValidationResult(false, "Api key is invalid."); ;
    }
    catch (Exception ex)
    {
      Console.Write(ex.Message);
      return new ApiKeyValidationResult(false, "Api key is invalid.");;
    }
  }

  public async Task OpenComicVineSite()
  {
    var url = "https://comicvine.gamespot.com/api/";
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
      Process.Start("xdg-open", url);
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      Process.Start("open", url);
    }
  }
}

