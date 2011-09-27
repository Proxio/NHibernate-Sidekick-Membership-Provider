using System;
using System.Web.Security;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;
using SharpArch.Domain.PersistenceSupport;

namespace NHibernate.Sidekick.Security.MembershipProvider.Contracts.Repositories
{
    public interface IMembershipProviderRepositoryWithTypedId<T, TId> : ILinqRepository<T>
        where T: UserBaseWithTypedId<TId>
    {
        MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords, string name, string applicationName);
        MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords, string name, string applicationName);
        MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords, string providerName, string applicationName); 
        int GetNumberOfUsers(string applicationName, DateTime? onlineCompareTime = null);
        string GetUsernameByEmail(string email, string applicationName);
    }
}