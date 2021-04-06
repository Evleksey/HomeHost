using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeHost.Utils
{
    public static class Utils
    {

        public static string GET(string Url)
        {
            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create(Url);
                req.Timeout = 1000;
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.Stream stream = resp.GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                string Out = sr.ReadToEnd();
                sr.Close();
                return Out;
            }
            catch { }
            return null;
        }
    }
}
