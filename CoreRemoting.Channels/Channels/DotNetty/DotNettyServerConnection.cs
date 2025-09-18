using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CoreRemoting.Channels;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace CoreRemoting.Channels.DotNetty
{
    /// <summary>
    /// Represents a server-side connection in the DotNetty channel.
    /// </summary>
    public class DotNettyServerConnection : IRawMessageTransport
    {
        private readonly Guid _connectionId;
        private readonly DotNettyServerChannel _serverChannel;
        private readonly IRemotingServer _server;
        private RemotingSession _session;
        private IChannelHandlerContext _channelContext;

        /// <summary>
        /// Creates a new instance of the DotNettyServerConnection class.
        /// </summary>
        /// <param name="connectionId">Unique connection ID</param>
        /// <param name="serverChannel">Server channel instance</param>
        /// <param name="server">CoreRemoting server instance</param>
        public DotNettyServerConnection(Guid connectionId, DotNettyServerChannel serverChannel, IRemotingServer server)
        {
            _connectionId = connectionId;
            _serverChannel = serverChannel;
            _server = server;
        }

        /// <summary>
        /// Event: Fires when a message is received from client.
        /// </summary>
        public event Action<byte[]> ReceiveMessage;

        /// <summary>
        /// Event: Fires when an error is occurred.
        /// </summary>
        public event Action<string, Exception> ErrorOccured;

        /// <summary>
        /// Gets or sets the last exception.
        /// </summary>
        public NetworkException LastException { get; set; }

        /// <summary>
        /// Sets the channel context for this connection.
        /// </summary>
        /// <param name="context">Channel handler context</param>
        internal void SetChannelContext(IChannelHandlerContext context)
        {
            _channelContext = context;
        }

        /// <summary>
        /// Fires the ReceiveMessage event.
        /// </summary>
        /// <param name="rawMessage">Raw message data</param>
        internal void FireReceiveMessage(byte[] rawMessage, Dictionary<string, object> metadata = null)
        {
            if (!CreateSessionAsNeeded(metadata))
            {
                ReceiveMessage?.Invoke(rawMessage);
            }
        }

        /// <summary>
        /// Creates the <see cref="RemotingSession"/> if it's not yet created.
        /// </summary>
        private bool CreateSessionAsNeeded(Dictionary<string, object> metadata)
        {
            if (_session != null)
                return false;

            byte[] clientPublicKey = null;
            string clientIpAddress = _channelContext?.Channel.RemoteAddress.ToString();

            if (metadata != null)
            {
                if (metadata.TryGetValue("MessageEncryption", out var messageEncryptionObj) &&
                    messageEncryptionObj is bool messageEncryption && messageEncryption)
                {
                    if (metadata.TryGetValue("ShakeHands", out var shakeHandsObj) &&
                        shakeHandsObj is string shakeHands && !string.IsNullOrEmpty(shakeHands))
                    {
                        clientPublicKey = Convert.FromBase64String(shakeHands);
                    }
                }
            }

            _session = _server.SessionRepository.CreateSession(
                clientPublicKey,
                clientIpAddress,
                _server,
                this);

            _session.BeforeDispose += BeforeDisposeSession;
            return true;
        }

        /// <summary>
        /// Closes the internal channel session.
        /// </summary>
        private async void BeforeDisposeSession()
        {
            _session = null;
            if (_channelContext != null && _channelContext.Channel.Active)
            {
                await _channelContext.Channel.CloseAsync().ConfigureAwait(false);
            }
            _serverChannel.RemoveConnection(_connectionId);
        }

        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        /// <param name="rawMessage">Raw message data</param>
        public async Task<bool> SendMessageAsync(byte[] rawMessage)
        {
            if (_channelContext == null || !_channelContext.Channel.Active)
                return false;

            try
            {
                var buffer = Unpooled.Buffer();
                buffer.WriteBytes(rawMessage);
                await _channelContext.WriteAndFlushAsync(buffer).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                LastException = new NetworkException(ex.Message, ex);
                ErrorOccured?.Invoke(ex.Message, ex);
                return false;
            }
        }
    }
}