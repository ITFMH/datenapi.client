using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleClient.JWT;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateLogger();

        Log.Information("Starting Console Test Client");

        /*
        Console Client Application
        Authorisation sample with OAuth Client Credential Flow and JWT Token */

        var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
        .Build();

        var oAuthOptions = new OAuthSettings();
        configuration.GetSection(OAuthSettings.Key).Bind(oAuthOptions);
        oAuthOptions.Check();

        // discover endpoints from metadata
        var client = new HttpClient();
        var disco = await client.GetDiscoveryDocumentAsync(oAuthOptions.Authority);
        if (disco.IsError)
        {
            Log.Error("Error on retrieving Information from Authorisation server: {Error}",disco.Error);
            return;
        }

        var signingCredential = ClientHelper.GetSigningCredentialFromPrivateKey(oAuthOptions.PrivateKey);
        var clientToken = ClientHelper.CreateClientToken(signingCredential, oAuthOptions.ClientId, disco.TokenEndpoint);

        Log.Information(clientToken);

        var requestUrl = new Uri(disco.TokenEndpoint ?? throw new ArgumentNullException(disco.TokenEndpoint,"Token Endpoint is null"));

        var parameters = new Dictionary<string, string>();
        parameters.Add("client_assertion", clientToken);
        parameters.Add("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer");
        parameters.Add("grant_type", "client_credentials");
        parameters.Add("scope", oAuthOptions.Scope);
        HttpContent content = new FormUrlEncodedContent(parameters);

        client.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");

        //optional
        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

        var response = await client.PostAsync(requestUrl, content);

        TokenResponse tokenResponse = null;

        if (response.IsSuccessStatusCode)
        {
            Log.Information("Authorization successful; Token:");
            var responseBody = await response.Content.ReadAsStringAsync();
            Log.Information(responseBody);
            tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);

            Log.Information("Click Enter to proceed with Api Call");
            Console.ReadLine();
        }
        else
        {
            Log.Error("Authorization failure {Response]", response);
            throw new InvalidOperationException();
        }

        client.SetBearerToken(tokenResponse.AccessToken);

        var apiResponse = await client.GetAsync("https://data-test.fmh.ch/api/Testt/FmhId/123456");
        if (!apiResponse.IsSuccessStatusCode)
        {
            Log.Error("StatusCode {ResponseStatusCode}", apiResponse.StatusCode);
        }
        else
        {
            var doc = JsonDocument.Parse(await apiResponse.Content.ReadAsStringAsync()).RootElement;
            Log.Information(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}