using Microsoft.Extensions.Options;
using NATS.Client;
using NATS.Client.JetStream;
using System.Collections.Concurrent;
using System.IO;

namespace NATS_WS.Services
{
    public class NatsReaderOptions
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Topic { get; set; }
        public string StreamName { get; set; }
        public int BatchSize { get; set; }
    }
    public class NatsReader : IDisposable
    {
        private readonly NatsReaderOptions _opts;
        private readonly IConnection _connection;
        private readonly IJetStream _js;
        private readonly IJetStreamPullSubscription _jpull;
        private bool disposed = false;

        public NatsReader(IOptions<NatsReaderOptions> opts)
        {
            _opts = opts.Value;

            var NOpts = ConnectionFactory.GetDefaultOptions();
            NOpts.Url = _opts.Url;
            NOpts.User = _opts.User;
            NOpts.Password = _opts.Password;
            _connection = new ConnectionFactory()
                .CreateConnection(NOpts);
            _js = _connection.CreateJetStreamContext();
            var conf = ConsumerConfiguration.Builder()
                .WithAckWait(1000)
                .Build();
            var _defOpts = PullSubscribeOptions.Builder()
                .WithDurable("test-pull-dur")
                .WithConfiguration(conf)
                .Build();
            _jpull = _js.PullSubscribe(_opts.Topic, _defOpts);
        }

        public IList<Msg> ReadStream()
        {
            return _jpull.Fetch(_opts.BatchSize, 500);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                _jpull.Dispose();
                _connection.Dispose();
                disposed = true;
            }
        }
    }
}
