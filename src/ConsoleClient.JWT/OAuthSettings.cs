using System;
using System.ComponentModel.DataAnnotations;

namespace ConsoleClient.JWT;

public class OAuthSettings
{
    public const string Key = "OAuth";

    [Required]
    public string ClientId { get; set; }

    [Required]
    public string Authority { get; set; }

    [Required]
    public string PrivateKey { get; set; }

    [Required]
    public string Scope { get; set; }

    public void Check()
    {
        if (string.IsNullOrEmpty(ClientId))
        {
            throw new ArgumentNullException(ClientId, "ClientId is missing");
        }

        if (string.IsNullOrEmpty(Authority))
        {
            throw new ArgumentNullException(Authority, "Authority is missing");
        }

        if (string.IsNullOrEmpty(PrivateKey))
        {
            throw new ArgumentNullException(PrivateKey, "PrivateKey is missing");
        }

        if (string.IsNullOrEmpty(Scope))
        {
            throw new ArgumentNullException(Scope, "Scope is missing");
        }
    }
}
