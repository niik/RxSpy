using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.HttpTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient();

            Task.Factory.StartNew(() => StreamResponse(client, args[0])).Wait();

            Console.ReadLine();
        }

        static async Task StreamResponse(HttpClient client, string address)
        {
            try
            {
                var response = await client.GetAsync(address + "stream", HttpCompletionOption.ResponseHeadersRead);

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var sw = new StreamReader(responseStream))
                {
                    while (true)
                    {
                        Console.WriteLine(await sw.ReadLineAsync());
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
