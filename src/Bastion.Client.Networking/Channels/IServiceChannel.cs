using System;

namespace Bastion.Client.Networking.Channels;

public interface IServiceChannel<TContract>
    where TContract : class
{
    void Call(Action<TContract> operation);

    TResult Call<TResult>(Func<TContract, TResult> operation);
}
