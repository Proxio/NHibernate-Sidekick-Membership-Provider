using System.Linq;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;

namespace NHibernate.Sidekick.Security.MembershipProvider.Specifications
{
    public class UserByUsername<T,TId> : UserSpecificationBase<T,TId> 
        where T : UserBaseWithTypedId<TId>
    {
        public UserByUsername(string username, string applicationName) : base(username: username, applicationName: applicationName)
        {
        }

        public override IQueryable<T> SatisfyingElementsFrom(IQueryable<T> candidates)
        {
            return candidates.Where(x => x.Username.ToLower() == Username &&
                                         x.ApplicationName == ApplicationName);
        }
    }
}