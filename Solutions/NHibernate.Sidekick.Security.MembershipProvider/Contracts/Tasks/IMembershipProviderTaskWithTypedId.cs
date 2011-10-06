using System;
using System.Web.Security;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;

namespace NHibernate.Sidekick.Security.MembershipProvider.Contracts.Tasks
{
    public interface IMembershipProviderTaskWithTypedId<T, TId> where T : UserBaseWithTypedId<TId>
    {
        bool Delete(string username, string applicationName);
        MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords, string name, string applicationName);
        MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords, string name, string applicationName);
        T Get(string username, string applicationName);
        T Get(TId id, string applicationName);
        MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords, string providerName, string applicationName);
        int GetNumberOfUsers(string applicationName, DateTime? onlineCompareTime = null);
        string GetUserNameByEmail(string email, string applicationName);
        void SaveOrUpdate(T user);
        bool ValidateUser(string username, string password, string applicationName);
        string GetPassword(string username, string applicationName);
    }
}