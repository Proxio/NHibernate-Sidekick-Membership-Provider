using NHibernate.Sidekick.Security.MembershipProvider.Domain;

namespace NHibernate.Sidekick.Security.MembershipProvider.Contracts.Repositories
{
    public interface IMembershipProviderRepository<T> : IMembershipProviderRepositoryWithTypedId<T,int>
        where T : UserBase 
    {}
}
