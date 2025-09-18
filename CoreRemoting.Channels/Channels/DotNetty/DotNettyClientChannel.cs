using CoreRemoting.Channels;
using CoreRemoting.Channels.DotNetty.Util;
using CoreRemoting.Threading;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CoreRemoting.Channels.DotNetty
{
    /// <summary>
    /// Client side DotNetty TCP channel implementation.
    /// </summary>
    public class DotNettyClientChannel : IClientChannel, IRawMessageTransport
    {
        private IEventLoopGroup _group;
        private Bootstrap _bootstrap;
        private IChannel _channel;
        private Dictionary<string, object> _handshakeMetadata;

        /// <summary>
        /// Event: Fires when a message is received from server.
        /// </summary>
        public event Action<byte[]> ReceiveMessage;

        /// <summary>
        /// Event: Fires when an error is occurred.
        /// </summary>
        public event Action<string, Exception> ErrorOccured;

        /// <inheritdoc />
        public event Action Disconnected;

        private AsyncLock DisposeLock { get; } = new();

        /// <summary>
        /// Initializes the channel.
        /// </summary>
        /// <param name="client">CoreRemoting client</param>
        public void Init(IRemotingClient client)
        {
            _group = new MultithreadEventLoopGroup();
            _bootstrap = new Bootstrap();
            _serverHostName = client.Config.ServerHostName;
            _serverPort = client.Config.ServerPort;

            _handshakeMetadata = new()
            {
                { "MessageEncryption", client.MessageEncryption }
            };

            if (client.MessageEncryption)
                _handshakeMetadata.Add("ShakeHands", Convert.ToBase64String(client.PublicKey));

            _bootstrap
                .Group(_group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new LengthFieldPrepender(4));
                    pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                    pipeline.AddLast(new ClientHandler(this));
                }));
        }

        private string _serverHostName;
        private int _serverPort;

        /// <summary>
        /// Establish a connection with the server.
        /// </summary>
        public async Task ConnectAsync()
        {
            if (_bootstrap == null)
                throw new InvalidOperationException("Channel is not initialized.");

            if (_channel != null && _channel.Active)
                return;

            _channel = AsyncHelper.RunSync(() => _bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(_serverHostName), _serverPort)));

            // Send handshake metadata
            var handshakeData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_handshakeMetadata));
            var buffer = Unpooled.Buffer();
            buffer.WriteBytes(handshakeData);
            await _channel.WriteAndFlushAsync(buffer)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public async Task DisconnectAsync()
        {
            using (await DisposeLock)
            {
                if (_channel != null)
                {
                    await _channel.CloseAsync().ConfigureAwait(false);
                    _channel = null;
                }
            }
        }

        /// <summary>
        /// Gets whether the connection is established or not.
        /// </summary>
        public bool IsConnected => _channel?.Active ?? false;

        /// <summary>
        /// Gets the raw message transport component for this connection.
        /// </summary>
        public IRawMessageTransport RawMessageTransport => this;

        /// <summary>
        /// Gets or sets the last exception.
        /// </summary>
        public NetworkException LastException { get; set; }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="rawMessage">Raw message data</param>
        public async Task<bool> SendMessageAsync(byte[] rawMessage)
        {
            if (_channel == null || !_channel.Active)
                return false;

            try
            {
                var buffer = Unpooled.Buffer();
                buffer.WriteBytes(rawMessage);
                await _channel.WriteAndFlushAsync(buffer).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                LastException = new NetworkException(ex.Message, ex);
                ErrorOccured?.Invoke(ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Event procedure: Called when a error occurs on the client.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="ex">Exception</param>
        internal void FireErrorOccured(string message, Exception ex)
        {
            LastException = new NetworkException(message, ex);
            ErrorOccured?.Invoke(message, ex);
        }

        /// <summary>
        /// Event procedure: Called when a message from server is received.
        /// </summary>
        /// <param name="rawMessage">Raw message data</param>
        internal void FireReceiveMessage(byte[] rawMessage)
        {
            ReceiveMessage?.Invoke(rawMessage);
        }

        /// <summary>
        /// Event procedure: Called when disconnected from server.
        /// </summary>
        internal void FireDisconnected()
        {
            Disconnected?.Invoke();
        }

        /// <summary>
        /// Stops listening and frees managed resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            using (await DisposeLock)
            {
                if (_channel != null)
                {
                    await _channel.CloseAsync().ConfigureAwait(false);
                    _channel = null;
                }

                if (_group != null)
                {
                    await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
                        .ConfigureAwait(false);
                    _group = null;
                }
            }
        }
    }
}