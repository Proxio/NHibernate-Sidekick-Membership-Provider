using System;
using System.IO;
using System.Reflection;
using FluentNHibernate;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using SharpArch.NHibernate;

namespace NHibernate.Sidekick.Security.Sampler.Infrastructure.NHibernateMaps
{
    public class FluentConfigurer
    {
        private Configuration Configuration { get; set; }
        private const string ExportDirectory = "C:\\Db\\";

        public FluentConfigurer(Configuration configuration)
        {
            Configuration = configuration;
        }

        public static class Configurer
        {
            public static IPersistenceConfigurer SQLite
            {
                get
                {
                    return SQLiteConfiguration.Standard
                        .UsingFile(@"C:\\Db\Sidekick.db3")
                        .ShowSql()
                        .FormatSql();
                }
            }
        }

        public FluentConfigurer GenerateDatabaseSchema()
        {
            var session = NHibernateSession.CurrentFor(NHibernateSession.DefaultFactoryKey);

            if (!Directory.Exists(ExportDirectory))
            {
                Directory.CreateDirectory(ExportDirectory);
            }

            string database = Path.Combine(ExportDirectory, "Sidekick.db3");
            if (File.Exists(database))
            {
                File.Delete(database);
            }

            using (TextWriter stringWriter = new StreamWriter(string.Format("{0}/Sidekick_Schema.sql", ExportDirectory)))
            {
                stringWriter.WriteLine("    --- Creation date: {0} ---", DateTime.Now.ToString("G"));
                new SchemaExport(Configuration).Execute(true, true, false, session.Connection, stringWriter);
            }

            return this;
        }

        public FluentConfigurer ExportHbmMappings()
        {
            if (!Directory.Exists(ExportDirectory))
            {
                Directory.CreateDirectory(ExportDirectory);
            }

            var persistenceModel = new PersistenceModel();
            persistenceModel.AddMappingsFromAssembly(Assembly.GetAssembly(typeof(FluentConfigurer)));
            persistenceModel.Configure(Configuration);
            persistenceModel.WriteMappingsTo(ExportDirectory);

            return this;
        }
    }
}
