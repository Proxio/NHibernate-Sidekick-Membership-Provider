using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Sidekick.Security.MembershipProvider.Contracts.Repositories;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;
using SharpArch.NHibernate;

namespace NHibernate.Sidekick.Security.MembershipProvider.Repositories
{
    public class MembershipProviderRepositoryWithTypedId<T, TId> : LinqRepository<T>, IMembershipProviderRepositoryWithTypedId<T,TId> 
        where T : UserBaseWithTypedId<TId>
    {
        public string GetUsernameByEmail(string email, string applicationName)
        {
            email = email.ToLowerAndTrim();

            return Session.QueryOver<T>()
                .WhereRestrictionOn(x => x.Email).IsInsensitiveLike(email, MatchMode.Exact)
                .AndRestrictionOn(x => x.ApplicationName).IsInsensitiveLike(applicationName, MatchMode.Exact)
                .Select(x => x.Username)
                .SingleOrDefault<string>();
        }

        public MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords, string name, string applicationName)
        {
            emailToMatch = emailToMatch.ToLowerAndTrim();

            IQueryable<T> query = Session.Query<T>()
                .Where(x => x.Email.Contains(emailToMatch) &&
                            x.ApplicationName == applicationName);

            MembershipUserCollection userCollection = new MembershipUserCollection();

            totalRecords = query.Count();
            if (totalRecords <= 0)
            {
                return userCollection;
            }

            int startIndex = pageSize * pageIndex;

            List<MembershipUser> users = query
                .Select(user => user.GetMembershipUser())
                .Skip(startIndex)
                .Take(pageSize)
                .ToList();

            users.ForEach(userCollection.Add);

            return userCollection;
        }

        public MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords, string name, string applicationName)
        {
            usernameToMatch = usernameToMatch.ToLowerAndTrim();

            IQueryable<T> query = Session.Query<T>()
                .Where(x => x.Username.Contains(usernameToMatch) &&
                            x.ApplicationName == applicationName);
            
            MembershipUserCollection userCollection = new MembershipUserCollection();

            totalRecords = query.Count();
            if (totalRecords <= 0)
            {
                return userCollection;
            }

            int startIndex = pageSize * pageIndex;
            
            List<MembershipUser> users = query
                .Select(user => user.GetMembershipUser())
                .Skip(startIndex)
                .Take(pageSize)
                .ToList();
            
            users.ForEach(userCollection.Add);

            return userCollection;
        }

        public MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords, string providerName, string applicationName)
        {
            MembershipUserCollection userCollection = new MembershipUserCollection();
            
            totalRecords = GetNumberOfUsers(applicationName);
            if (totalRecords <= 0)
            {
                return userCollection;
            }

            int startIndex = pageSize * pageIndex;
            
            List<MembershipUser> users = Session.Query<T>()
                .Where(x => x.ApplicationName == applicationName)
                .Select(usr => usr.GetMembershipUser())
                .Skip(startIndex)
                .Take(pageSize)
                .ToList();

            users.ForEach(userCollection.Add);

            return userCollection;
        }

        public int GetNumberOfUsers(string applicationName, DateTime? onlineCompareTime = null)
        {
            var query = Session.QueryOver<T>()
                .WhereRestrictionOn(x => x.ApplicationName)
                .IsInsensitiveLike(applicationName, MatchMode.Exact);

            if (onlineCompareTime.HasValue)
            {
                query = query.And(x => x.LastActivityDate > onlineCompareTime.Value);
            }

            return query.RowCount();
        }
    }
}
