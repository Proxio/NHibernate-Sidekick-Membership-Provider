using System;
using System.Diagnostics;

namespace NHibernate.Sidekick.Security.MembershipProvider.Domain
{
    [Serializable]
    [DebuggerDisplay("{Username}")]
    public abstract class UserBase : UserBaseWithTypedId<int>
    {}
}