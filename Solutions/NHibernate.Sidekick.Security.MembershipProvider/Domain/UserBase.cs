using System;

namespace NHibernate.Sidekick.Security.MembershipProvider.Domain
{
    [Serializable]
    public abstract class UserBase : UserBaseWithTypedId<int>
    {}
}