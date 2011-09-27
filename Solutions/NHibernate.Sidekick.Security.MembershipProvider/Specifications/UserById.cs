using System.Linq;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;

namespace NHibernate.Sidekick.Security.MembershipProvider.Specifications
{
    public class UserById<T,TId> : UserSpecificationBase<T,TId> 
        where T : UserBaseWithTypedId<TId>
    {
        public UserById(TId id, string applicationName): base(applicationName, id)
        {
        }

        public override IQueryable<T> SatisfyingElementsFrom(IQueryable<T> candidates)
        {
            return candidates.Where(x => Equals(x.Id, Id));
        }
    }
}