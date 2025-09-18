using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Common.Utilities;

namespace CoreRemoting.Channels.DotNetty
{
    /// <summary>
    /// Server side message handler for DotNetty channel.
    /// </summary>
    internal class ServerHandler : ChannelHandlerAdapter
    {
        private readonly DotNettyServerConnection _connection;
        private bool _handshakeDone;
        private Dictionary<string, object> _handshakeMetadata;

        /// <summary>
        /// Creates a new instance of the ServerHandler class.
        /// </summary>
        /// <param name="connection">Server connection instance</param>
        public ServerHandler(DotNettyServerConnection connection)
        {
            _connection = connection;
            _handshakeMetadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Called when a message is received from a client.
        /// </summary>
        /// <param name="context">Channel handler context</param>
        /// <param name="message">Received message</param>
        public override async void ChannelRead(IChannelHandlerContext context, object message)
        {
            _connection.SetChannelContext(context);

            if (message is IByteBuffer buffer)
            {
                try
                {
                    var bytes = new byte[buffer.ReadableBytes];
                    buffer.ReadBytes(bytes);

                    _connection.FireReceiveMessage(bytes, _handshakeMetadata);
                }
                catch (Exception ex)
                {
                    _connection.LastException = new NetworkException(ex.Message, ex);
                }
                finally
                {
                    ReferenceCountUtil.Release(message);
                }
            }
        }

        /// <summary>
        /// Called when an exception occurs.
        /// </summary>
        /// <param name="context">Channel handler context</param>
        /// <param name="exception">Exception that occurred</param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _connection.LastException = new NetworkException("Error in DotNetty server channel", exception);
            context.CloseAsync();
        }

        /// <summary>
        /// Called when the channel is closed.
        /// </summary>
        /// <param name="context">Channel handler context</param>
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            // Clean up resources when client disconnects
            base.ChannelInactive(context);
        }
    }
}