using Blaze.SimTainer.Service.Api.Config;
using Blaze.SimTainer.Service.Api.Config.Mapping;
using Blaze.SimTainer.Service.Api.EventHubs;
using Blaze.SimTainer.Service.Api.Middleware;
using Blaze.SimTainer.Service.Api.Services;
using Blaze.SimTainer.Service.Providers.CloudStack.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using System.IO.Compression;
using System.Linq;
using Blaze.SimTainer.Service.Providers.Shared.Models;

namespace Blaze.SimTainer.Service.Api
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddOptions();
			services.AddSignalR()
				.AddJsonProtocol(options => { options.PayloadSerializerOptions.PropertyNamingPolicy = null; });

			services.Configure<ApiOptions>(Configuration.GetSection("Api"));
			services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
			services.AddResponseCompression(options =>
			{
				options.EnableForHttps = true;
				options.Providers.Add<GzipCompressionProvider>();
				options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/unityweb" });
			});

			services
				.AddMvc(options => options.EnableEndpointRouting = false)
				.AddJsonOptions(options => options.JsonSerializerOptions.IgnoreNullValues = true)
				.AddControllersAsServices()
				.SetCompatibilityVersion(CompatibilityVersion.Latest);

			services.AddRouting(options =>
			{
				options.AppendTrailingSlash = true;
				options.LowercaseUrls = true;
			});


			services.AddApiVersioning(o =>
			{
				o.AssumeDefaultVersionWhenUnspecified = true;
				o.ReportApiVersions = true;
				o.DefaultApiVersion = new ApiVersion(1, 0);
			});

			services.AddVersionedApiExplorer(options =>
			{
				options.SubstituteApiVersionInUrl = true;
				options.GroupNameFormat = "'v'VVV";
			});


			services.AddSwagger();
			services.AddAutoMapper();
			services.AddSingleton<CloudStackService>();
			services.AddHostedService<ApiCollectorService>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
			IApiVersionDescriptionProvider versionDescriptionProvider)
		{
			app.UseExceptionHandling(env);

			app.UseResponseCompression();
			if (env.IsDevelopment() || env.IsEnvironment("Local")) app.UseSwagger(versionDescriptionProvider);


			app.UseMiddleware<RequestMetricsMiddleware>();
			app.UseMetricServer();


			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapStatusEndpoint();
				endpoints.MapControllers();
				endpoints.MapHub<CloudStackGameEventHub>("/hubs/cloudstack/game");
			});

			FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();
			// Add new mappings
			provider.Mappings[".unityweb"] = "application/unityweb";

			app.UseStaticFiles(new StaticFileOptions
			{
				ContentTypeProvider = provider
			});
		}
	}
}