using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CoreRemoting.Channels;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Codecs;

namespace CoreRemoting.Channels.DotNetty
{
    /// <summary>
    /// Server side DotNetty TCP channel implementation.
    /// </summary>
    public class DotNettyServerChannel : IServerChannel
    {
        private IRemotingServer _remotingServer;
        private IEventLoopGroup _bossGroup;
        private IEventLoopGroup _workerGroup;
        private IChannel _boundChannel;
        private ServerBootstrap _bootstrap;
        private readonly ConcurrentDictionary<Guid, DotNettyServerConnection> _connections;

        /// <summary>
        /// Creates a new instance of the DotNettyServerChannel class.
        /// </summary>
        public DotNettyServerChannel()
        {
            _connections = new ConcurrentDictionary<Guid, DotNettyServerConnection>();
        }

        /// <summary>
        /// Initializes the channel.
        /// </summary>
        /// <param name="server">CoreRemoting sever</param>
        public void Init(IRemotingServer server)
        {
            _remotingServer = server ?? throw new ArgumentNullException(nameof(server));

            _bossGroup = new MultithreadEventLoopGroup(1);
            _workerGroup = new MultithreadEventLoopGroup();

            _bootstrap = new ServerBootstrap();
            _bootstrap
                .Group(_bossGroup, _workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));

                    // Create a new connection for each client
                    var connectionId = Guid.NewGuid();
                    var connection = new DotNettyServerConnection(connectionId, this, _remotingServer);
                    _connections.TryAdd(connectionId, connection);
                    pipeline.AddLast(new ServerHandler(connection));
                }));
        }

        /// <summary>
        /// Start listening for client requests.
        /// </summary>
        public void StartListening()
        {
            if (_bootstrap == null)
                throw new InvalidOperationException("Channel is not initialized.");

            if (_boundChannel != null && _boundChannel.Active)
                return;

            // Bind to port asynchronously
            _boundChannel = _bootstrap.BindAsync(_remotingServer.Config.NetworkPort).Result;
        }

        /// <summary>
        /// Stop listening for client requests.
        /// </summary>
        public void StopListening()
        {
            _boundChannel?.CloseAsync().Wait();
        }

        /// <summary>
        /// Gets whether the channel is listening or not.
        /// </summary>
        public bool IsListening => _boundChannel?.Active ?? false;

        /// <summary>
        /// Removes a connection from the connection pool.
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        internal void RemoveConnection(Guid connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        /// <summary>
        /// Stops listening and frees managed resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_boundChannel != null)
            {
                await _boundChannel.CloseAsync().ConfigureAwait(false);
                _boundChannel = null;
            }

            if (_workerGroup != null)
            {
                await _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
                    .ConfigureAwait(false);
                _workerGroup = null;
            }

            if (_bossGroup != null)
            {
                await _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
                    .ConfigureAwait(false);
                _bossGroup = null;
            }
        }
    }
}