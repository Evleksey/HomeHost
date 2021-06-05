using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Refit;
using System.IO;
namespace AccessService
{   
    public class GoogleTokenValidator : ISecurityTokenValidator
    {
        private int _maxTokenSizeInBytes = TokenValidationParameters.DefaultMaximumTokenSizeInBytes;
        private JwtSecurityTokenHandler _tokenHandler;
        private readonly IKeys _keys;

        public GoogleTokenValidator(IKeys keys)
        {
            _keys = keys ?? throw new ArgumentNullException(nameof(_keys));
            _tokenHandler = new JwtSecurityTokenHandler();
        }
        public bool CanValidateToken  { get { return true; } }

        public int MaximumTokenSizeInBytes
        {
            get
            {
                return _maxTokenSizeInBytes;
            }

            set
            {
                _maxTokenSizeInBytes = value;
            }
        }
        public bool CanReadToken(string securityToken)
        {
            return _tokenHandler.CanReadToken(securityToken);
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            validationParameters.IssuerSigningKeys = _keys.SecurityKeys;

            var principal = _tokenHandler.ValidateToken(securityToken, validationParameters, out validatedToken);

            return principal;
        }
    }
}
