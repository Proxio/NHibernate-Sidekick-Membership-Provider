using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Text;
using System.Web.Configuration;
using System.Web.Security;
using NHibernate.Sidekick.Security.MembershipProvider.Contracts.Repositories;
using NHibernate.Sidekick.Security.MembershipProvider.Domain;
using NHibernate.Sidekick.Security.MembershipProvider.Enums;
using NHibernate.Sidekick.Security.MembershipProvider.Repositories;
using NHibernate.Sidekick.Security.MembershipProvider.Tasks;

namespace NHibernate.Sidekick.Security.MembershipProvider.Providers
{
    public abstract class MembershipProviderWithTypedId<T, TId> : System.Web.Security.MembershipProvider
        where T : UserBaseWithTypedId<TId>, new()
    {
        #region Private
        private readonly MembershipProviderTaskWithTypedId<T, TId> membershipProviderTask;
        // Global connection string, generated password length, generic exception message, event log info.
        private int newPasswordLength = 8;
        private const string eventSource = "SidekickMembershipProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";
        private string connectionString;

        private string _applicationName;
        private bool _enablePasswordReset;
        private bool _enablePasswordRetrieval;
        private bool _requiresQuestionAndAnswer;
        private bool _requiresUniqueEmail;
        private int _maxInvalidPasswordAttempts;
        private int _passwordAttemptWindow;
        private MembershipPasswordFormat _passwordFormat;
        // Used when determining encryption key values.
        private MachineKeySection _machineKey;
        private int _minRequiredNonAlphanumericCharacters;
        private int _minRequiredPasswordLength;
        private string _passwordStrengthRegularExpression;

        #endregion

        protected MembershipProviderWithTypedId()
        {
            membershipProviderTask = new MembershipProviderTaskWithTypedId<T, TId>(new MembershipProviderRepositoryWithTypedId<T, TId>());
        }

        protected MembershipProviderWithTypedId(IMembershipProviderRepositoryWithTypedId<T,TId> repository)
        {
            membershipProviderTask = new MembershipProviderTaskWithTypedId<T, TId>(repository);
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            // Initialize values from web.config.
            if (config == null)
                throw new ArgumentNullException("config");
            
            if (name.Length == 0)
                name = eventSource;

            if (String.IsNullOrWhiteSpace(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "NHibernate Sidekick's Membership Provider");
            }
            
            // Initialize the abstract base class.
            base.Initialize(name, config);

            _applicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            _maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            _passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            _minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            _minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            _passwordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            _enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            _enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            _requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            _requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            WriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));

            string passwordFormat = config["passwordFormat"];
            _passwordFormat = string.IsNullOrWhiteSpace(passwordFormat)
                                  ? MembershipPasswordFormat.Hashed
                                  : PasswordFormatter[passwordFormat];
            
            //Encryption skipped
            Configuration cfg = WebConfigurationManager
                .OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            _machineKey = (MachineKeySection) cfg.GetSection("system.web/machineKey");

            if (_machineKey.ValidationKey.Contains("AutoGenerate") && PasswordFormat != MembershipPasswordFormat.Clear)
                throw new ProviderException("Hashed or Encrypted passwords are not supported with auto-generated keys.");
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);
            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            email = email.ToLowerAndTrim();
            if (RequiresUniqueEmail && !string.IsNullOrWhiteSpace(GetUserNameByEmail(email)))
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            username = username.ToLowerAndTrim();
            MembershipUser muser = GetUser(username, false);

            if (muser == null)
            {
                T user = new T
                             {
                                 Username = username,
                                 Password = EncodePassword(password),
                                 Email = email,
                                 PasswordQuestion = passwordQuestion,
                                 PasswordAnswer = EncodePassword(passwordAnswer),
                                 IsApproved = isApproved,
                                 ApplicationName = ApplicationName
                             };

                membershipProviderTask.SaveOrUpdate(user);

                if (typeof(TId) == typeof(int))
                {
                    int id = Convert.ToInt32(user.Id);
                    status = id <= 0
                                 ? MembershipCreateStatus.UserRejected
                                 : MembershipCreateStatus.Success;
                }
                else
                {
                    status = MembershipCreateStatus.Success;
                }
                
                MembershipUser membershipUser = user.GetMembershipUser();
                return membershipUser;
            }

            status = MembershipCreateStatus.DuplicateUserName;
            return null;
        }

        private string EncodePassword(string password)
        {
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword = 
                        Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    // TODO Password encryption (hashed);
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }
            return encodedPassword;
        }

        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password = 
                        Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            T user = membershipProviderTask.Get(username, ApplicationName);

            if (user == null)
            {
                return false;
            }

            DateTime start = DateTime.Now;
            user.Password = EncodePassword(password);
            user.PasswordQuestion = newPasswordQuestion;
            user.PasswordAnswer = EncodePassword(newPasswordAnswer);
            user.FailedPasswordAttemptCount = 0;
            user.FailedPasswordAttemptWindowStart = start;
            user.FailedPasswordAnswerAttemptCount = 0;
            user.FailedPasswordAnswerAttemptWindowStart = start;
            user.LastPasswordChangedDate = start;

            membershipProviderTask.SaveOrUpdate(user);

            return true;
        }

        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval || PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException("Password retrieval is not available.");
            }

            T user = membershipProviderTask.Get(username, ApplicationName);

            if (user == null)
            {
                throw new MembershipPasswordException(string.Format("The username \"{0}\" was not found", username));
            }

            if (user.IsLockedOut)
            {
                throw new MembershipPasswordException(string.Format("The username \"{0}\" is locked out", username));
            }

            string password = user.Password;
            string passwordAnswer = user.PasswordAnswer;

            if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
            {
                UpdateFailureCount(username, PasswordFailureType.Answer);
                throw new MembershipPasswordException("Incorrect password answer.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                password = UnEncodePassword(password);
            }

            return password;
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            T user = membershipProviderTask.Get(username, ApplicationName);

            if (user.Password != oldPassword)
            {
                return false;
            }

            DateTime start = DateTime.Now;
            user.Password = EncodePassword(newPassword);
            user.FailedPasswordAttemptCount = 0;
            user.FailedPasswordAttemptWindowStart = start;
            user.LastPasswordChangedDate = start;

            membershipProviderTask.SaveOrUpdate(user);

            return true;
        }

        public override string ResetPassword(string username, string answer)
        {
            T user = membershipProviderTask.Get(username, ApplicationName);

            if (user == null)
                throw new MembershipPasswordException("The supplied user name is not found.");

            if (user.IsLockedOut)
                throw new MembershipPasswordException("The supplied user is locked out.");

            if (RequiresQuestionAndAnswer && !CheckPassword(answer, user.PasswordAnswer))
            {
                UpdateFailureCount(username, PasswordFailureType.Answer);
                throw new MembershipPasswordException("Incorrect password answer.");
            }

            DateTime start = DateTime.Now;
            user.PasswordAnswer = Guid.NewGuid().ToString();
            user.FailedPasswordAttemptCount = 0;
            user.FailedPasswordAttemptWindowStart = start;
            user.LastPasswordChangedDate = start;
            
            membershipProviderTask.SaveOrUpdate(user);

            return user.Password;
        }

        public override void UpdateUser(MembershipUser user)
        {
            T usr = membershipProviderTask.Get(user.UserName, ApplicationName);
            if (usr != null)
            {
                usr.Email = user.Email.ToLowerAndTrim();
                usr.Comment = user.Comment;
                usr.IsApproved = user.IsApproved;
                membershipProviderTask.SaveOrUpdate(usr);
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = membershipProviderTask.ValidateUser(username, password, ApplicationName);
            if (!isValid)
            {
                UpdateFailureCount(username, PasswordFailureType.Password);
            }

            return isValid;
        }

        public override bool UnlockUser(string userName)
        {
            T user = membershipProviderTask.Get(userName, ApplicationName);

            if (user == null)
            {
                return false;
            }

            user.LastLockedOutDate = DateTime.Now;
            user.IsLockedOut = false;

            membershipProviderTask.SaveOrUpdate(user);

            return true;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return GetMembershipUserByKeyOrUser(false, username, 0, userIsOnline);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return GetMembershipUserByKeyOrUser(true, string.Empty, providerUserKey, userIsOnline);
        }

        public override string GetUserNameByEmail(string email)
        {
            return membershipProviderTask.GetUserNameByEmail(email, ApplicationName);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            return membershipProviderTask.Delete(username, ApplicationName);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = membershipProviderTask.GetAllUsers(pageIndex, pageSize, out totalRecords, Name, ApplicationName);
            return users;
        }

        public override int GetNumberOfUsersOnline()
        {
            TimeSpan onlineSpan = new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0);
            DateTime onlineCompareTime = DateTime.Now.Subtract(onlineSpan);
            int numberOfUsersOnline = membershipProviderTask.GetNumberOfUsers(ApplicationName, onlineCompareTime);

            return numberOfUsersOnline;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = membershipProviderTask.FindUsersByName(usernameToMatch, pageIndex, pageSize, out totalRecords, Name, ApplicationName);
            return users;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = membershipProviderTask.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords, Name, ApplicationName);
            return users;
        }

        #region Public properties
        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return _enablePasswordRetrieval; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return _requiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqueEmail; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }

        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _passwordFormat; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _minRequiredPasswordLength; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _passwordStrengthRegularExpression; }
        }

        // If false, exceptions are thrown to the caller. If true,
        // exceptions are written to the event log.
        public bool WriteExceptionsToEventLog { get; set; }
        #endregion

        #region Helper functions
        private void UpdateFailureCount(string username, PasswordFailureType failureType)
        {
            DateTime windowStart = new DateTime();
            int failureCount = 0;

            T user = membershipProviderTask.Get(username, ApplicationName);

            if (user != null)
            {
                if (failureType == PasswordFailureType.Password)
                {
                    failureCount = user.FailedPasswordAttemptCount;
                    windowStart = user.FailedPasswordAttemptWindowStart;
                }

                if (failureType == PasswordFailureType.Answer)
                {
                    failureCount = user.FailedPasswordAnswerAttemptCount;
                    windowStart = user.FailedPasswordAnswerAttemptWindowStart;
                }

                DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                if (failureCount == 0 || DateTime.Now > windowEnd)
                {
                    // First password failure or outside of PasswordAttemptWindow. 
                    // Start a new password failure count from 1 and a new window starting now.
                    if (failureType == PasswordFailureType.Password)
                    {
                        user.FailedPasswordAttemptCount = 1;
                        user.FailedPasswordAttemptWindowStart = DateTime.Now;
                    }

                    if (failureType == PasswordFailureType.Answer)
                    {
                        user.FailedPasswordAnswerAttemptCount = 1;
                        user.FailedPasswordAnswerAttemptWindowStart = DateTime.Now;
                    }

                    membershipProviderTask.SaveOrUpdate(user);
                }
                else
                {
                    if (failureCount++ >= MaxInvalidPasswordAttempts)
                    {
                        // Password attempts have exceeded the failure threshold. 
                        // Lock out the user.
                        user.IsLockedOut = true;
                        user.LastLockedOutDate = DateTime.Now;

                        membershipProviderTask.SaveOrUpdate(user);
                    }
                    else
                    {
                        // Password attempts have not exceeded the failure threshold. 
                        // Update the failure counts. Leave the window the same.
                        if (failureType == PasswordFailureType.Password)
                            user.FailedPasswordAttemptCount = failureCount;

                        if (failureType == PasswordFailureType.Answer)
                            user.FailedPasswordAnswerAttemptCount = failureCount;

                        membershipProviderTask.SaveOrUpdate(user);
                    }
                }
            }
        }

        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
            }

            return pass1 == pass2;
        }
        
        private string GetConfigValue(string configValue, string defaultValue)
        {
            return String.IsNullOrWhiteSpace(configValue)
                       ? defaultValue
                       : configValue;
        }

        private MembershipUser GetMembershipUserByKeyOrUser(bool isKeySupplied, string username, object providerUserKey, bool userIsOnline)
        {
            T user = isKeySupplied
                            ? membershipProviderTask.Get((TId) providerUserKey, ApplicationName)
                            : membershipProviderTask.Get(username, ApplicationName);

            MembershipUser membershipUser = null;
            if (user != null)
            {
                membershipUser = user.GetMembershipUser();

                if (userIsOnline)
                {
                    user.LastActivityDate = DateTime.Now;
                    membershipProviderTask.SaveOrUpdate(user);
                }
            }

            return membershipUser;
        }

        private readonly Dictionary<string, MembershipPasswordFormat> PasswordFormatter = new Dictionary<string, MembershipPasswordFormat>
                                                                                              {
                                                                                                  {"Hashed", MembershipPasswordFormat.Hashed},
                                                                                                  {"Encrypted", MembershipPasswordFormat.Encrypted},
                                                                                                  {"Clear", MembershipPasswordFormat.Clear}
                                                                                              };
        #endregion
    }
}