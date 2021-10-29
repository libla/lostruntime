using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

[Init]
public sealed class HttpServer : IDisposable
{
	private readonly IHostBuilder builder;
	private readonly CancellationTokenSource cts = new CancellationTokenSource();
	private IHost listener;

	static HttpServer()
	{
		Adpter.run += () =>
		{
			HttpServer server = new HttpServer("http://+:8080");
			server.Start();
		};
	}

	public class Options
	{
		internal Options()
		{
			urls = new List<string>();
		}

		public readonly List<string> urls;
	}

	public HttpServer(string url) : this(options => options.urls.Add(url)) { }
	public HttpServer(params string[] urls) : this(options => options.urls.AddRange(urls)) { }

	public HttpServer(Action<Options> configuration)
	{
		Options options = new Options();
		configuration(options);
		builder = CreateHostBuilder(options);
	}

	private IHostBuilder CreateHostBuilder(Options options)
	{
		var host = new HostBuilder();
		host.ConfigureHostConfiguration(config =>
		{
			config.AddEnvironmentVariables(prefix: "DOTNET_");
		}).UseDefaultServiceProvider((context, options) => {
			bool isDevelopment = context.HostingEnvironment.IsDevelopment();
			options.ValidateScopes = isDevelopment;
			options.ValidateOnBuild = isDevelopment;
		});
		return host.ConfigureWebHostDefaults(web =>
		{
			web.UseUrls(options.urls.ToArray()).UseStartup(context => new Startup(this, context.Configuration));
		});
	}

	private class Startup
	{
		private readonly HttpServer server;

		public Startup(HttpServer server, IConfiguration configuration)
		{
			this.server = server;
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.Map("/", async context =>
				{
					await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("Hello, world"));
				});
			});
		}
	}

	public async void Dispose()
	{
		cts.Cancel();
		await listener.StopAsync();
	}

	public void Start()
	{
		if (listener == null)
		{
			listener = builder.Build();
			listener.Run();
		}
	}

	public async void Stop()
	{
		if (listener != null)
		{
			var old = listener;
			listener = null;
			await old.StopAsync();
		}
	}
}