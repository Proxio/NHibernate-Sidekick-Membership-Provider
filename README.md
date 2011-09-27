NHibernate Sidekick's ASP.NET Membership Provider
=================================================
This project is an implementation of the [ASP.NET Membership Provider](http://msdn.microsoft.com/en-us/library/yh26yfzy.aspx) using [NHibernate](http://nhforge.org) and [Sharp Architecture](http://www.sharparchitecture.net/).

Implementation
---------------------
### 1. Create your `User` entity
Create the entity responsible for persisting data between the Membership Provider and your database.
<pre><code>public class User : NHibernate.Sidekick.Security.MembershipProvider.Domain.UserBase  { }
</code></pre>

This model assumes your class' Identifier is an integer. If this is not the case, inherit from `UserBaseWithTypedId<TId>` instead.

### 2. Create your provider
This is who unobtrusively does all the work for you.
<pre><code>public class MembershipProvider : NHibernate.Sidekick.Security.MembershipProvider.Providers.MembershipProvider<User> { }
</code></pre>
This model assumes your `User`'s Identifier is an integer. If this is not the case, inherit from `MembershipProviderWithTypedId<T, TId>` instead.

### 3. Ignore `UserBase` from Fluent NHibernate's Automap generator
<pre><code>public class AutoPersistenceModelGenerator : SharpArch.NHibernate.FluentNHibernate.IAutoPersistenceModelGenerator
{
	public AutoPersistenceModel Generate()
    {
		AutoPersistenceModel mappings = AutoMap.AssemblyOf<User>(new AutomappingConfiguration());
		// Default Sharp Architecture options go here.		
		mappings.IgnoreBase<UserBase>();
		mappings.IgnoreBase(typeof(UserBaseWithTypedId<>));
	}
}
</code></pre>
This step is only relevant if you're using Fluent NHibernate's [Automapping mechanism](http://wiki.fluentnhibernate.org/Auto_mapping).

### 4. Set your application's authentication mode to `Forms` 
Set this within your application's `web.config`:
<pre><code><configuration>
	<system.web>
		<authentication mode="Forms" />
	</system.web>
</configuration>
</code></pre>

### 5. Set your provider's configuration options
Set this within your application's `web.config`:
<pre><code>&lt;configuration>
	&lt;system.web>
		&lt;membership defaultProvider="SidekickMembershipProvider" userIsOnlineTimeWindow="15">
			&lt;providers>
				&lt;add    name="SidekickMembershipProvider"
						applicationName ="Sidekick_Security_SampleApp"
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
<pre><code>@if(Request.IsAuthenticated) {
    <text>Welcome <strong>@User.Identity.Name</strong>!</text>
}
</pre></code>
If you're unfamiliar with ASP.NET's Membership Provider, you can find more information on Microsoft's [MSDN](http://msdn.microsoft.com/en-us/library/system.web.security.membership.aspx) and [Patterns and Practices](http://msdn.microsoft.com/en-us/library/ff648345.aspx).

Third-Party Dependencies
-------------------------
* [NHibernate 3.1.0.4000](http://sourceforge.net/projects/nhibernate/files/NHibernate/)
* [Fluent NHibernate 1.2.0.712](http://fluentnhibernate.org/downloads)
* [Sharp Architecture 2.0.0.3 RC](https://github.com/sharparchitecture/Sharp-Architecture/downloads)
