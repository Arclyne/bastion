using System;
using Bastion.Domain.Accounts;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Security;

namespace Bastion.Server.Application.Sessions;

public sealed class SessionVerifier : ISessionVerifier
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISessionTokenGenerator _tokenGenerator;

    public SessionVerifier(IUnitOfWork unitOfWork, ISessionTokenGenerator tokenGenerator)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(tokenGenerator);

        _unitOfWork = unitOfWork;
        _tokenGenerator = tokenGenerator;
    }

    public ConnectedPlayer Verify(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Session? session = _unitOfWork.Sessions.FindByTokenHash(_tokenGenerator.Hash(sessionToken));
        if (session is null || !session.IsActiveAt(now))
        {
            throw new InvalidSessionException();
        }

        Account? account = _unitOfWork.Accounts.FindById(session.AccountId);
        if (account is null)
        {
            throw new InvalidSessionException();
        }

        return new ConnectedPlayer(account.AccountId, account.Nickname);
    }
}
