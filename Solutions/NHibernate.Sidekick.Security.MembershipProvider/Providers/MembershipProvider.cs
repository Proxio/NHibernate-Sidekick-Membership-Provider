using NHibernate.Sidekick.Security.MembershipProvider.Domain;

namespace NHibernate.Sidekick.Security.MembershipProvider.Providers
{
    public abstract class MembershipProvider<T> : MembershipProviderWithTypedId<T, int>
        where T : UserBase, new()
    {
    }
}

