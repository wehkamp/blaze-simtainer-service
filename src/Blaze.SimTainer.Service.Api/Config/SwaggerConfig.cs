using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IO;
using System.Reflection;

namespace Blaze.SimTainer.Service.Api.Config
{
	internal static class SwaggerConfig
	{
		private static readonly string ApiName = "Blaze SimTainer Service";

		private static string XmlCommentsFilePath
		{
			get
			{
				string basePath = PlatformServices.Default.Application.ApplicationBasePath;
				string fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
				return Path.Combine(basePath, fileName);
			}
		}

		public static void AddSwagger(this IServiceCollection services)
		{
			services.AddSwaggerGen(options =>
			{
				IApiVersionDescriptionProvider provider = services
					.BuildServiceProvider()
					.GetRequiredService<IApiVersionDescriptionProvider>();

				foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
					options.SwaggerDoc(
						description.GroupName,
						new OpenApiInfo
						{
							Title = $"{ApiName}",
							Version = description.ApiVersion.ToString()
						});


				RegisterHeaders(options);

				// add the XML comments to Swagger providing the file
				options.IncludeXmlComments(XmlCommentsFilePath);
			});
		}

		private static void RegisterHeaders(SwaggerGenOptions options)
		{
		}

		public static IApplicationBuilder UseSwagger(this IApplicationBuilder app,
			IApiVersionDescriptionProvider versionDescriptionProvider)
		{
			app.UseSwagger();
			app.UseSwaggerUI(options =>
			{
				foreach (ApiVersionDescription description in versionDescriptionProvider.ApiVersionDescriptions)
					options.SwaggerEndpoint(
						$"/swagger/{description.GroupName}/swagger.json",
						description.GroupName.ToLowerInvariant()
					);
			});

			return app;
		}
	}
}