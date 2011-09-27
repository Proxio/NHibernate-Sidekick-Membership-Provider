using System.Linq;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;

namespace NHibernate.Sidekick.Security.MembershipProvider.Specifications
{
    public class UserValidation<T,TId> : UserSpecificationBase<T,TId> 
        where T : UserBaseWithTypedId<TId>
    {
        private readonly string Password;

        public UserValidation(string username, string password, string applicationName) 
            : base(username: username, applicationName: applicationName)
        {
            Password = password;
        }

        public override IQueryable<T> SatisfyingElementsFrom(IQueryable<T> candidates)
        {
            return candidates.Where(x => x.Username.ToLower() == Username &&
                                         x.Password == Password &&
                                         x.ApplicationName == ApplicationName);
        }
    }
}