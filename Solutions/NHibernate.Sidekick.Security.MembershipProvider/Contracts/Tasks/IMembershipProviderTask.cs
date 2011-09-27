using NHibernate.Sidekick.Security.MembershipProvider.Domain;

namespace NHibernate.Sidekick.Security.MembershipProvider.Contracts.Tasks
{
    public interface IMembershipProviderTask<T> : IMembershipProviderTaskWithTypedId<T, int>
        where T : UserBaseWithTypedId<int>
    {
    }
}