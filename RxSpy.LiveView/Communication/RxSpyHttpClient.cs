using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Communication.Serialization;
using RxSpy.Events;

namespace RxSpy.Communication
{
    public class RxSpyHttpClient : ReactiveObject, IRxSpyClient
    {
        readonly HttpClient _client;

        public RxSpyHttpClient()
        {
            _client = new HttpClient();
        }

        public IObservable<IEvent> Connect(Uri address, TimeSpan timeout)
        {
            return GetStream(new Uri(address, "stream"), timeout);
        }

        public IObservable<IEvent> GetStream(Uri address, TimeSpan timeout)
        {
            var client = new HttpClient();
            client.Timeout = timeout;

            var req = new HttpRequestMessage(HttpMethod.Get, address);
            var disp = new CompositeDisposable(req);

            var completionOptions = HttpCompletionOption.ResponseHeadersRead;

            return Observable.FromAsync<HttpResponseMessage>(ct => client.SendAsync(req, completionOptions, ct))
                .SelectMany(resp =>
                    resp.StatusCode == HttpStatusCode.OK
                        ? Observable.FromAsync(() => resp.Content.ReadAsStreamAsync())
                        : Observable.Throw<Stream>(new Exception("Could not open room stream: " + resp.ReasonPhrase)))
                .SelectMany(ReadEvents)
                .Finally(disp.Dispose);
        }

        IObservable<IEvent> ReadEvents(Stream stream)
        {
            var strategy = new RxSpyJsonSerializerStrategy();

            return Observable.Create<IEvent>(async (observer, ct) =>
            {
                using (var sr = new StreamReader(stream))
                {
                    string line;

                    while (((line = await sr.ReadLineAsync()) != null))
                    {
                        ct.ThrowIfCancellationRequested();
                        observer.OnNext(SimpleJson.DeserializeObject<IEvent>(line, strategy));
                    }

                    observer.OnCompleted();
                }
            });
        }
    }
}
