using Bastion.Contracts.Sessions;

namespace Bastion.Client.Networking.Channels;

public interface ISessionChannelFactory
{
    ISessionService CreateChannel();
}
