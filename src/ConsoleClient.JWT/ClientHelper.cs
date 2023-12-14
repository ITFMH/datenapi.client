using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleClient.JWT;

public static class ClientHelper
{
public static string CreateClientToken(SigningCredentials credential, string clientId, string tokenEndpoint)
    {
        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
        clientId,
        tokenEndpoint,
        new List<Claim>()
        {
        new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString()),
        new Claim(JwtClaimTypes.Subject, clientId),
        new Claim(JwtClaimTypes.IssuedAt, now.ToEpochTime().ToString(), ClaimValueTypes.Integer64)
        },
        now,
        now.AddYears(1),
        credential
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    public static JwtSecurityToken CreateRawClientToken(SigningCredentials credential, string clientId, string tokenEndpoint)
    {
        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
        clientId,
        tokenEndpoint,
        new List<Claim>()
        {
        new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString()),
        new Claim(JwtClaimTypes.Subject, clientId),
        new Claim(JwtClaimTypes.IssuedAt, now.ToEpochTime().ToString(), ClaimValueTypes.Integer64)
        },
        now,
        now.AddMinutes(1),
        credential
        );

        return token;
    }
    public static SigningCredentials GetSigningCredentialFromPrivateKey(string privateKey)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(privateKey.ToCharArray());

        var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        return signingCredentials;
    }
}
