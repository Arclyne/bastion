using System;
using System.ServiceModel;
using log4net;

namespace Bastion.Client.Networking.Channels;

public static class ChannelCloser
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ChannelCloser));

    public static void CloseOrAbort(ICommunicationObject channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        if (channel.State != CommunicationState.Opened)
        {
            channel.Abort();
            return;
        }

        try
        {
            channel.Close();
        }
        catch (CommunicationException ex)
        {
            LogAndAbort(channel, ex);
        }
        catch (TimeoutException ex)
        {
            LogAndAbort(channel, ex);
        }
    }

    private static void LogAndAbort(ICommunicationObject channel, Exception ex)
    {
        _logger.Warn($"The channel could not be closed and was aborted. Channel={channel.GetType().Name}", ex);
        channel.Abort();
    }
}
