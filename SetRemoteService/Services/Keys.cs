using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Refit;

namespace SetRemoteService
{
    public interface IDevice
    {
        [Get("/robot/v1/metadata/x509/{email}")]
        Task<string> GetKeys(string email);
    }
    public interface IKeys
    {
        public IList<SecurityKey> SecurityKeys { get; set; }
        public Task<bool> GetKeys();
        
    }

    public class Keys : IKeys
    {
        private readonly string _email;
        public IList<SecurityKey> SecurityKeys { get; set; }
        public Keys(string email)
        {
            _email = email;
        }
        public async Task<bool> GetKeys()
        {
            var refit = RestService.For<IDevice>("https://www.googleapis.com");
            var result = refit.GetKeys(_email).Result.Replace("\"", "").Split(new string[] { ": ", "," }, StringSplitOptions.None);

            SecurityKeys = new List<SecurityKey>();

            for (int i = 1; i < result.Length; i += 2)
            {
                System.IO.File.WriteAllText("cert.txt", result[i].Replace("\\n", "\n"));
                X509Certificate2 cert = new X509Certificate2("cert.txt");
                SecurityKeys.Add(new X509SecurityKey(cert));
            }

            System.IO.File.Delete("cert.txt");

            return true;
        }
    }
}
