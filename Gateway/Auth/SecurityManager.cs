using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using System.Text;
using System.Net.Http;
using System;
using System.Collections.Generic;
using Microsoft.Owin.Security.DataHandler.Encoder;
using System.Globalization;
using Newtonsoft.Json;
using System.Security.Cryptography;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
using JWT;
using JWT.Builder;
using JWT.Algorithms;

namespace Gateway
{
    public interface IGoogleOAuth2
    {
        public string AccessToken { get; set; }

        public async Task<bool> RequestAccessTokenAsync()
        {
            return false;
        }
    }    

    public class GoogleJwt : IGoogleOAuth2
    {

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        
        private readonly string _defaultScope;

        private readonly string _serviceAccount;

        private readonly string _certificateFile;

        public string AccessToken { get; set; }

        public GoogleJwt(string defaultScope, string serviceAccount, string certificateFile)
        {
            _defaultScope = defaultScope;
            _serviceAccount = serviceAccount;
            _certificateFile = certificateFile;
        }

        public async Task<bool> RequestAccessTokenAsync()
        {
            var certificate = new X509Certificate2(_certificateFile, "notasecret", X509KeyStorageFlags.Exportable);

            DateTime now = DateTime.UtcNow;

            var serviceAccountCredential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(_serviceAccount)
            {
                Scopes = new[] { _defaultScope }

            }.FromCertificate(certificate));

            var token = JwtBuilder.Create()
                      .WithAlgorithm(new RS256Algorithm(certificate))
                      .AddClaim("iss", "api-477@homehost-315909.iam.gserviceaccount.com")
                      .AddClaim("email", "api-477@homehost-315909.iam.gserviceaccount.com")
                      .AddClaim("aud", "acessserver")
                      .AddClaim("iat", ((int)now.Subtract(UnixEpoch).TotalSeconds).ToString())
                      .AddClaim("exp", ((int)now.AddMinutes(55).Subtract(UnixEpoch).TotalSeconds).ToString())
                      .Encode();            

            AccessToken = token;
            return true;
        }

    }
}