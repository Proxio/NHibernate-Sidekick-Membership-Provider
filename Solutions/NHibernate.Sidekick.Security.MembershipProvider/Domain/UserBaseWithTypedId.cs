using System;
using System.Diagnostics;
using NHibernate.Sidekick.Security.MembershipProvider.Contracts.Domain;
using SharpArch.Domain.DomainModel;

namespace NHibernate.Sidekick.Security.MembershipProvider.Domain
{
    [Serializable]
    [DebuggerDisplay("{Username}")]
    public abstract class UserBaseWithTypedId<TId> : EntityWithTypedId<TId>, IUserBaseWithTypedId<TId>
    {
        protected UserBaseWithTypedId()
        {
            CreationDate = DateTime.Now;
            Comment = string.Empty;
            FailedPasswordAttemptCount = 0;
            FailedPasswordAnswerAttemptCount = 0;
            FailedPasswordAttemptWindowStart = CreationDate;
            FailedPasswordAnswerAttemptWindowStart = CreationDate;
            IsLockedOut = false;
            LastActivityDate = CreationDate;
            LastPasswordChangedDate = CreationDate;
            LastLockedOutDate = CreationDate;
        }

        public virtual string Username { get; set; }
        public virtual string ApplicationName { get; set; }
        public virtual string Email { get; set; }
        public virtual string Comment { get; set; }
        public virtual string Password { get; set; }
        public virtual string PasswordQuestion { get; set; }
        public virtual string PasswordAnswer { get; set; }
        public virtual bool IsApproved { get; set; }
        public virtual DateTime LastActivityDate { get; set; }
        public virtual DateTime? LastLoginDate { get; set; }
        public virtual DateTime LastPasswordChangedDate { get; set; }
        public virtual DateTime CreationDate { get; protected set; }
        public virtual bool IsOnline { get; set; }
        public virtual bool IsLockedOut { get; set; }
        public virtual DateTime LastLockedOutDate { get; set; }
        public virtual int FailedPasswordAttemptCount { get; set; }
        public virtual int FailedPasswordAnswerAttemptCount { get; set; }
        public virtual DateTime FailedPasswordAttemptWindowStart { get; set; }
        public virtual DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }
    } 
}
