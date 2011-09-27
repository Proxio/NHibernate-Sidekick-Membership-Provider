using System.Linq;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;
using SharpArch.Domain.Specifications;

namespace NHibernate.Sidekick.Security.MembershipProvider.Specifications
{
    public abstract class UserSpecificationBase<T, TId> : ILinqSpecification<T> 
        where T : UserBaseWithTypedId<TId>
    {
        protected readonly TId Id;
        protected readonly string Username;
        protected readonly string ApplicationName;

        protected UserSpecificationBase(string applicationName, string username = null)
        {
            Username = username.ToLowerAndTrim();
            ApplicationName = applicationName;
        }

        protected UserSpecificationBase(string applicationName, TId id, string username = null)
        {
            Id = id;
            Username = username.ToLowerAndTrim();
            ApplicationName = applicationName;
        }

        public abstract IQueryable<T> SatisfyingElementsFrom(IQueryable<T> candidates);
    }
}