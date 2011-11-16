using System;
using System.Text;
using System.Web;
using NHibernate.Sidekick.Security.MembershipProvider.Contracts.Repositories;
using NHibernate.Sidekick.Security.MembershipProvider.Contracts.Tasks;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;

namespace NHibernate.Sidekick.Security.MembershipProvider
{
    /// <remarks>
    /// http://stackoverflow.com/questions/3355601/asp-net-remember-me-cookie/3355868#3355868
    /// </remarks>
    public class PasswordManager
    {
        public PasswordManager(string salt)
        {
            Salt = salt;
        }

        private readonly string Salt;

        /// <summary>
        /// Create a hash of the given password and salt.
        /// </summary>
        public string CreateHash(string password)
        {
            // Get a byte array containing the combined password + salt.
            string authDetails = password + Salt;
            byte[] authBytes = Encoding.ASCII.GetBytes(authDetails);

            // Use MD5 to compute the hash of the byte array, and return the hash as
            // a Base64-encoded string.
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashedBytes = md5.ComputeHash(authBytes);
            string hash = Convert.ToBase64String(hashedBytes);

            return hash;
        }
    }
}