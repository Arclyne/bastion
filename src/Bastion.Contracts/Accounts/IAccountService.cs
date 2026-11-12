using System.ServiceModel;

namespace Bastion.Contracts.Accounts;

/// <summary>
/// Logs registered players in and opens their session.
/// </summary>
[ServiceContract]
public interface IAccountService
{
    /// <summary>
    /// Logs a player in with a nickname or an email and a password, and records the attempt in the access log.
    /// </summary>
    /// <param name="credentials">The nickname or email and the password the player entered.</param>
    /// <param name="device">The fingerprint and the version of the client the player logs in from.</param>
    /// <returns>The token of the new session, with the nickname and preferred language of the player.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidCredentialsFault"/>: the credentials or the device are missing, no account matches the
    /// identifier, the account was deleted, or the password is wrong.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="AccountRestrictedFault"/>: the account is locked, pending, suspended or banned; the detail says
    /// which, and for how many minutes it stays locked.
    /// </exception>
    /// <exception cref="FaultException">The server could not record the login or reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidCredentialsFault))]
    [FaultContract(typeof(AccountRestrictedFault))]
    PlayerSession LogIn(PlayerCredentials? credentials, ClientDevice? device);
}
