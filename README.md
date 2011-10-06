NHibernate Sidekick's ASP.NET Membership Provider
=================================================
This project is an implementation of the [ASP.NET Membership Provider](http://msdn.microsoft.com/en-us/library/yh26yfzy.aspx) using [NHibernate](http://nhforge.org) and [Sharp Architecture](http://www.sharparchitecture.net/).

Changelog
---------------------
 * 0.9.2
  - Passwords are encrypted by default using a salt.
   
Implementation
---------------------
### 1. Create your `User` entity
Create the entity responsible for persisting data between the Membership Provider and your database.
<pre><code>public class User : NHibernate.Sidekick.Security.MembershipProvider.Domain.UserBase  { }
</code></pre>

This model assumes your `User`'s Identifier is an integer. If this is not the case, inherit from `UserBaseWithTypedId<TId>` instead.

### 2. Create your provider
This is who unobtrusively does all the work for you.
<pre><code>public class MembershipProvider : NHibernate.Sidekick.Security.MembershipProvider.Providers.MembershipProvider&lt;User> { }
</code></pre>
This model assumes your `User`'s Identifier is an integer. If this is not the case, inherit from `MembershipProviderWithTypedId<T, TId>` instead.

### 3. Ignore `UserBase` from Fluent NHibernate's Automap generator
<pre><code>public class AutoPersistenceModelGenerator : SharpArch.NHibernate.FluentNHibernate.IAutoPersistenceModelGenerator
{
	public AutoPersistenceModel Generate()
    {
		AutoPersistenceModel mappings = AutoMap.AssemblyOf&lt;User>(new AutomappingConfiguration());
		// Default Sharp Architecture options go here.		
		mappings.IgnoreBase&lt;UserBase>();
		mappings.IgnoreBase(typeof(UserBaseWithTypedId&lt;>));
	}
}
</code></pre>
This step is only relevant if you're using Fluent NHibernate's [Automapping mechanism](http://wiki.fluentnhibernate.org/Auto_mapping).

### 4. Set your application's authentication mode to `Forms` 
Set this within your application's `web.config`:
<pre><code>&lt;configuration>
	&lt;authentication mode="Forms">
          &lt;forms loginUrl="Account/LogOn" defaultUrl="/" />
	&lt;/authentication>
&lt;/configuration>
</code></pre>

### 5. Set your provider's configuration options
Set this within your application's `web.config`:
<pre><code>&lt;configuration>
	&lt;system.web>
		&lt;membership defaultProvider="SidekickMembershipProvider" userIsOnlineTimeWindow="15">
			&lt;providers>
				&lt;add    name="SidekickMembershipProvider"
						applicationName="Sidekick_Security_SampleApp"
						salt="SidekickSalt"
						type="NHibernate.Sidekick.Security.Sampler.Domain.MembershipProvider"
						enablePasswordRetrieval="true"
						enablePasswordReset="true"
						requiresQuestionAndAnswer="true"
						passwordFormat="Clear"/>
			&lt;/providers>
		&lt;/membership>
	&lt;/system.web>
&lt;/configuration>
</code></pre>

Simple usage
-------------
<pre><code>@if (Request.IsAuthenticated) {
    &lt;text>Welcome &lt;strong>@User.Identity.Name&lt;/strong>!&lt;/text>
}
</pre></code>
<pre><code>[Authorize]
public class HomeController : Controller { }
</pre></code>
If you're unfamiliar with ASP.NET's Membership Provider, you can find more information on Microsoft's [MSDN](http://msdn.microsoft.com/en-us/library/system.web.security.membership.aspx) and [Patterns and Practices](http://msdn.microsoft.com/en-us/library/ff648345.aspx).

.NET Dependencies
------------------
* .NET Framework 4.0
* System
* System.Core
* System.Data
* System.Configuration
* System.Web
* System.Web.ApplicationServices

Third-Party Dependencies
-------------------------
* [NHibernate 3.1.0.4000](http://sourceforge.net/projects/nhibernate/files/NHibernate/)
* [Fluent NHibernate 1.2.0.712](http://fluentnhibernate.org/downloads)
* [Sharp Architecture 2.0.0.3 RC](https://github.com/sharparchitecture/Sharp-Architecture/downloads)
