using System;
using System.Web.Security;
using NHibernate.Sidekick.Security.MembershipProvider.Contracts.Repositories;
using NHibernate.Sidekick.Security.MembershipProvider.Contracts.Tasks;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;
using NHibernate.Sidekick.Security.MembershipProvider.Specifications;

namespace NHibernate.Sidekick.Security.MembershipProvider.Tasks
{
    public class MembershipProviderTaskWithTypedId<T,TId> : IMembershipProviderTaskWithTypedId<T, TId> where T : UserBaseWithTypedId<TId>
    {
        private readonly IMembershipProviderRepositoryWithTypedId<T,TId> membershipProviderRepository;

        public MembershipProviderTaskWithTypedId(IMembershipProviderRepositoryWithTypedId<T,TId> membershipProviderRepository)
        {
            this.membershipProviderRepository = membershipProviderRepository;
        }

        public bool Delete(string username, string applicationName)
        {
            T user = membershipProviderRepository.FindOne(new UserByUsername<T, TId>(username, applicationName));
            if (user == null)
            {
                return false;
            }

            membershipProviderRepository.Delete(user);

            return true;
        }

        public MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords, string name, string applicationName)
        {
            return membershipProviderRepository.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords,
                                                                name, applicationName);
        }

        public T Get(TId id, string applicationName)
        {
            return membershipProviderRepository.FindOne(new UserById<T,TId>(id, applicationName));
        }

        public T Get(string username, string applicationName)
        {
            return membershipProviderRepository.FindOne(new UserByUsername<T,TId>(username, applicationName));
        }

        public string GetUserNameByEmail(string email, string applicationName)
        {
            return membershipProviderRepository.GetUsernameByEmail(email, applicationName);
        }

        public MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords, string providerName, string applicationName)
        {
            return membershipProviderRepository.GetAllUsers(pageIndex, pageSize, out totalRecords, providerName, applicationName);
        }

        public int GetNumberOfUsers(string applicationName, DateTime? onlineCompareTime = null)
        {
            return membershipProviderRepository.GetNumberOfUsers(applicationName, onlineCompareTime);
        }

        public bool ValidateUser(string username, string password, string applicationName)
        {
            T user = membershipProviderRepository.FindOne(new UserValidation<T,TId>(username, password, applicationName));

            if (user == null || user.IsLockedOut)
            {
                return false;
            }

            user.LastLoginDate = DateTime.Now;
            membershipProviderRepository.Save(user);

            return true;
        }

        public string GetPassword(string username, string applicationName)
        {
            return membershipProviderRepository.GetPassword(username, applicationName);
        }

        public void SaveOrUpdate(T user)
        {
            membershipProviderRepository.SaveOrUpdate(user);
        }

        public MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords, string name, string applicationName)
        {
            MembershipUserCollection users = membershipProviderRepository.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords, name, applicationName);
            return users;
        }
    }
}