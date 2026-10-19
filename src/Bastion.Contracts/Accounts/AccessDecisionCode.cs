using System.Runtime.Serialization;

namespace Bastion.Contracts.Accounts;

[DataContract]
public enum AccessDecisionCode
{
    [EnumMember]
    Granted,

    [EnumMember]
    InvalidCredentials,

    [EnumMember]
    AccountLocked,

    [EnumMember]
    AccountPending,

    [EnumMember]
    AccountSuspended,

    [EnumMember]
    AccountBanned,

    [EnumMember]
    ClientOutdated,

    [EnumMember]
    ServerError,
}
