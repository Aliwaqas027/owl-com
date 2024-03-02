using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class JWTSecurityTokenValidator : ISecurityTokenValidator
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    private readonly IConfiguration _configuration;

    // issuer1:
    //      key1: <key1 json>
    //      key2: <key2 json>
    // issuer2:
    //      key1: <key1 json>
    //      key2: <key2 json>
    // ...
    private Dictionary<String, Dictionary<String, JsonWebKey>> issuerKeysCache = new Dictionary<String, Dictionary<String, JsonWebKey>>();

    public JWTSecurityTokenValidator(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenHandler = new JwtSecurityTokenHandler();
        _configuration = configuration;
    }

    public bool CanValidateToken => true;

    public int MaximumTokenSizeInBytes { get; set; } = TokenValidationParameters.DefaultMaximumTokenSizeInBytes;

    bool ISecurityTokenValidator.CanReadToken(string securityToken)
    {
        return _tokenHandler.CanReadToken(securityToken);
    }

    public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
    {
        var jwtToken = new JwtSecurityToken(securityToken);

        if (!this.ValidateIssuer(jwtToken.Issuer))
        {
            throw new Exception("Invalid issuer " + jwtToken.Issuer);
        }

        var parameters = new TokenValidationParameters()
        {
            IssuerSigningKey = this.GetIssuerKey(jwtToken.Issuer, jwtToken.Header.Kid),
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true
        };

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        var user = handler.ValidateToken(securityToken, parameters, out validatedToken);
        var roleClaims = user.FindFirst(t => t.Type == "realm_access");
        var roleObject = JObject.Parse(roleClaims.Value.ToString());
        JArray roles = (JArray)roleObject["roles"];

        var isProductPermitted = roles.ToObject<List<String>>().Contains(_configuration["Authentication:ProductRole"]);
        var isCarrier = roles.ToObject<List<String>>().Contains(UserRole.Carrier);
        if (!isProductPermitted && !isCarrier)
        {
            throw new ApplicationException("Product not permitted!");
        }

        return user;
    }

    private bool ValidateIssuer(String issuer)
    {
        var validIssuer = _configuration["Authentication:AuthServerUrl"] + "/auth";
        return issuer.StartsWith(validIssuer);
    }

    private JsonWebKey GetIssuerKey(String issuer, String keyId)
    {
        var shouldFetchKeys = false;

        if (!this.issuerKeysCache.ContainsKey(issuer))
        {
            shouldFetchKeys = true;
        }
        else
        {
            var keyInfo = this.issuerKeysCache[issuer];
            if (!keyInfo.ContainsKey(keyId))
            {
                shouldFetchKeys = true;
            }
        }

        if (shouldFetchKeys)
        {
            var keys = this.FetchIssuerKeys(issuer);
            this.issuerKeysCache[issuer] = keys;
        }

        return this.issuerKeysCache[issuer][keyId];
    }


    private Dictionary<String, JsonWebKey> FetchIssuerKeys(String issuer)
    {
        Console.WriteLine("Fetching keys for issuer " + issuer);

        var certsEndpoint = $"{issuer}/protocol/openid-connect/certs";
        var response = HttpHelper.JsonGetRequestSync(certsEndpoint);
        JArray keys = new JArray(response["keys"][0]);

        Dictionary<String, JsonWebKey> issuerKeys = new Dictionary<string, JsonWebKey>();

        foreach (var key in keys)
        {
            var keyId = key["kid"];
            issuerKeys[(String)keyId] = new JsonWebKey(key.ToString());
        }

        return issuerKeys;
    }
}