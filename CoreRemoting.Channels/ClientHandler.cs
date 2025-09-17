using System;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Common.Utilities;

namespace CoreRemoting.Channels.DotNetty
{
    /// <summary>
    /// Client side message handler for DotNetty channel.
    /// </summary>
    internal class ClientHandler : ChannelHandlerAdapter
    {
        private readonly DotNettyClientChannel _clientChannel;

        /// <summary>
        /// Creates a new instance of the ClientHandler class.
        /// </summary>
        /// <param name="clientChannel">Client channel instance</param>
        public ClientHandler(DotNettyClientChannel clientChannel)
        {
            _clientChannel = clientChannel;
        }

        /// <summary>
        /// Called when a message is received from the server.
        /// </summary>
        /// <param name="context">Channel handler context</param>
        /// <param name="message">Received message</param>
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer buffer)
            {
                try
                {
                    var bytes = new byte[buffer.ReadableBytes];
                    buffer.ReadBytes(bytes);
                    _clientChannel.FireReceiveMessage(bytes);
                }
                catch (Exception ex)
                {
                    _clientChannel.FireErrorOccured("Error processing received message", ex);
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
            _clientChannel.FireErrorOccured("Error in DotNetty client channel", exception);
            context.CloseAsync();
        }

        /// <summary>
        /// Called when the channel is closed.
        /// </summary>
        /// <param name="context">Channel handler context</param>
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _clientChannel.FireDisconnected();
            base.ChannelInactive(context);
        }
    }
}