using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Communication.Serialization;
using RxSpy.Events;

namespace RxSpy.Communication
{
    internal class RxSpyFileWriter: IRxSpyServer
    {
        string _path;
        StreamWriter _writer;
        RxSpyJsonSerializerStrategy _serializerStrategy;

        public Uri Address
        {
            get { return new Uri(_path); }
        }

        public RxSpyFileWriter(string path)
        {
            _path = path;
            _serializerStrategy = new RxSpyJsonSerializerStrategy();
        }

        public void WaitForConnection(TimeSpan timeout)
        {
            _writer = new StreamWriter(File.OpenWrite(_path));
        }

        public void EnqueueEvent(Event ev)
        {
            lock (_writer)
            {
                _writer.WriteLine(SimpleJson.SerializeObject(ev, _serializerStrategy));
            }
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
