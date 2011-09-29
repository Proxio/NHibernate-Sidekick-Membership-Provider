using System;
using SharpArch.Domain.DomainModel;

namespace NHibernate.Sidekick.Security.MembershipProvider.Contracts.Domain
{
    public interface IUserBaseWithTypedId<TId> : IEntityWithTypedId<TId>
    {
        string Username { get; set; }
        string ApplicationName { get; set; }
        string Email { get; set; }
        string Comment { get; set; }
        string Password { get; set; }
        string PasswordQuestion { get; set; }
        string PasswordAnswer { get; set; }
        bool IsApproved { get; set; }
        DateTime LastActivityDate { get; set; }
        DateTime? LastLoginDate { get; set; }
        DateTime LastPasswordChangedDate { get; set; }
        DateTime CreationDate { get; }
        bool IsOnline { get; set; }
        bool IsLockedOut { get; set; }
        DateTime LastLockedOutDate { get; set; }
        int FailedPasswordAttemptCount { get; set; }
        int FailedPasswordAnswerAttemptCount { get; set; }
        DateTime FailedPasswordAttemptWindowStart { get; set; }
        DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }
    }
}