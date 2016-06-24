# NRedisSessionProvider
NRedisSessionProvider is a class library for persisting ASP.NET/MVC Session data to a Redis.

##Installation

###nuget

 install `NRedisSessionProvider`  via nuget

```
 PM> Install-Package NRedisSessionProvider
```

##Using

###Modifying the web.config

the web.config **sessionState** section should be changed to mark `NRedisSessionProvider` as the provider of the Session data.

example:

```
 <sessionState mode="Custom" customProvider="NRedisSessionProvider" timeout="120">
      <providers>
        <add name="NRedisSessionProvider" type="NRedisProvider.NRedisSessionProvider,NRedisSessionProvider" />
      </providers>
    </sessionState>
```

###Configuring redis 

-  specify the sessionState providers of web.config 

```
 <sessionState mode="Custom" customProvider="NRedisSessionProvider"  timeout="120">
      <providers>
        <add name="NRedisSessionProvider" type="NRedisProvider.NRedisSessionProvider,NRedisSessionProvider" host="127.0.0.1:6379" pooled="false" prefix="_sess"/>
      </providers>
    </sessionState>
    
```

- or configuring redis's config in your application (Global.asax application_start)

```
 public class Global : HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);
  			NRedisProvider.NRedisSessionProvider.InitRedisConfig("localhost:6379", false);
		}
	}

```


###options

- prefix: session key prefix for redis(`optional`)

- host:

```
 localhost
 127.0.0.1:6379
 redis://localhost:6379
 password @localhost:6379
 clientid:password @localhost:6379
 redis://clientid:password@localhost:6380?ssl=true&db=1
 
```

- pooled:`true/false`  default:`false`