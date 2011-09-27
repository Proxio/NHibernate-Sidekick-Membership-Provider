using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using CommonServiceLocator.WindsorAdapter;
using Castle.Windsor;
using log4net.Config;
using SharpArch.NHibernate;
using SharpArch.NHibernate.Web.Mvc;
using SharpArch.Web.Mvc.Castle;
using SharpArch.Web.Mvc.ModelBinder;
using NHibernate.Sidekick.Security.Sampler.Web.Mvc.CastleWindsor;
using NHibernate.Sidekick.Security.Sampler.Web.Mvc.Controllers;
using NHibernate.Sidekick.Security.Sampler.Infrastructure.NHibernateMaps;

namespace NHibernate.Sidekick.Security.Sampler.Web.Mvc
{
    /// <summary>
    /// Represents the MVC Application
    /// </summary>
    /// <remarks>
    /// For instructions on enabling IIS6 or IIS7 classic mode, 
    /// visit http://go.microsoft.com/?LinkId=9394801
    /// </remarks>
    public class MvcApplication : System.Web.HttpApplication
    {
        #region Default configuration
        private WebSessionStorage webSessionStorage;

        /// <summary>
        /// Due to issues on IIS7, the NHibernate initialization must occur in Init().
        /// But Init() may be invoked more than once; accordingly, we introduce a thread-safe
        /// mechanism to ensure it's only initialized once.
        /// See http://msdn.microsoft.com/en-us/magazine/cc188793.aspx for explanation details.
        /// </summary>
        public override void Init()
        {
            base.Init();
            webSessionStorage = new WebSessionStorage(this);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            NHibernateInitializer.Instance().InitializeNHibernateOnce(InitialiseNHibernateSessions);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Useful for debugging
            Exception ex = Server.GetLastError();
            var reflectionTypeLoadException = ex as ReflectionTypeLoadException;
        }

        protected void Application_Start()
        {
            XmlConfigurator.Configure();

            ViewEngines.Engines.Clear();

            ViewEngines.Engines.Add(new RazorViewEngine());

            ModelBinders.Binders.DefaultBinder = new SharpModelBinder();

            ModelValidatorProviders.Providers.Add(new ClientDataTypeModelValidatorProvider());

            InitializeServiceLocator();

            AreaRegistration.RegisterAllAreas();
            RouteRegistrar.RegisterRoutesTo(RouteTable.Routes);
        }

        /// <summary>
        /// Instantiate the container and add all Controllers that derive from
        /// WindsorController to the container.  Also associate the Controller
        /// with the WindsorContainer ControllerFactory.
        /// </summary>
        protected virtual void InitializeServiceLocator()
        {
            IWindsorContainer container = new WindsorContainer();

            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container));

            container.RegisterControllers(typeof(HomeController).Assembly);
            ComponentRegistrar.AddComponentsTo(container);

            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
        } 
        #endregion

        private void InitialiseNHibernateSessions()
        {
            NHibernateSession.ConfigurationCache = new NHibernateConfigurationFileCache();

            var configuration = NHibernateSession.Init(
                webSessionStorage,
                new[] {Server.MapPath("~/bin/NHibernate.Sidekick.Security.Sampler.Infrastructure.dll")},
                new AutoPersistenceModelGenerator().Generate(),
                null,
                null,
                null,
                FluentConfigurer.Configurer.SQLite);

            new FluentConfigurer(configuration)
                .GenerateDatabaseSchema()
                .ExportHbmMappings();
        }
    }
}