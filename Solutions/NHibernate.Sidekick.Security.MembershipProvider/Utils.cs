using System;
using System.Web.Security;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;

namespace NHibernate.Sidekick.Security.MembershipProvider
{
    public static class Utils
    {
        public static MembershipUser GetMembershipUser<TId>(this UserBaseWithTypedId<TId> user)
        {
            MembershipUser membershipUser = new MembershipUser(Membership.Provider.Name,
                                                               user.Username,
                                                               user.Id,
                                                               user.Email,
                                                               user.PasswordQuestion,
                                                               user.Comment,
                                                               user.IsApproved,
                                                               user.IsLockedOut,
                                                               user.CreationDate,
                                                               user.LastLoginDate.HasValue ? user.LastLoginDate.Value : new DateTime(),
                                                               user.LastActivityDate,
                                                               user.LastPasswordChangedDate,
                                                               user.LastLockedOutDate);

            return membershipUser;
        }

        public static string ToLowerAndTrim(this string value)
        {
            return value != null ? value.ToLowerInvariant().Trim() : null;
        }
    }
}