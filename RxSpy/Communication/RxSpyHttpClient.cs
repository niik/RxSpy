using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Communication
{
    public class RxSpyHttpClient
    {
        readonly HttpClient _client;

        public RxSpyHttpClient()
        {
            _client = new HttpClient();
        }

        public void Connect(Uri address, TimeSpan timeout)
        {

        }
    }
}
