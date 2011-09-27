using NHibernate.Sidekick.Security.MembershipProvider.Domain;

namespace NHibernate.Sidekick.Security.MembershipProvider.Repositories
{
    public class MembershipProviderRepository<T> : MembershipProviderRepositoryWithTypedId<T, int>
        where T : UserBase
    {}
}