using System;
using Bastion.Domain.Auditing;

namespace Bastion.Server.Application.Accounts;

/// <summary>
/// Logs registered players in: checks their credentials and the state of their account, opens their session and
/// records every attempt in the access log.
/// </summary>
public interface ILoginService
{
    /// <summary>
    /// Logs a player in with a nickname or an email and a password. A wrong password counts toward the lock of the
    /// account, and a correct one clears the count.
    /// </summary>
    /// <param name="attempt">The identifier the player entered, the connection it came from and the time of the attempt.</param>
    /// <param name="password">The password the player entered.</param>
    /// <returns>The token of the new session, with the nickname and preferred language of the player.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="attempt"/> or <paramref name="password"/> is null.</exception>
    /// <exception cref="InvalidCredentialsException">No account matches the identifier, the account was deleted, or the password is wrong.</exception>
    /// <exception cref="AccountRestrictedException">The account is locked, pending, suspended or banned; the exception says which, and for how many minutes it stays locked.</exception>
    LoggedInPlayer LogIn(AccessAttempt attempt, string password);
}
