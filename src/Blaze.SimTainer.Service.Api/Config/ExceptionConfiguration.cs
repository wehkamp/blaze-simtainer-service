using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace Blaze.SimTainer.Service.Api.Config
{
	internal static class ExceptionConfiguration
	{
		public static void UseExceptionHandling(
			this IApplicationBuilder app,
			IWebHostEnvironment env)
		{
			if (env.IsDevelopment() || env.IsEnvironment("Local"))
				app.UseDeveloperExceptionPage();
			else
				app.UseExceptionHandler(builder =>
				{
					builder.Run(async context =>
					{
						context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
						context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

						IExceptionHandlerFeature error = context.Features.Get<IExceptionHandlerFeature>();
						if (error != null)
						{
							ILogger<Program> logger =
								context.RequestServices.GetService(typeof(ILogger<Program>)) as ILogger<Program>;
							logger.LogError(error.Error, "UnhandledException");

							string exception = error.Error.Demystify().ToString();
							string msg = error.Error.Message;
							if (error.Error.InnerException != null) msg += "\n" + error.Error.InnerException.Message;

							await context.Response.WriteAsync(msg).ConfigureAwait(false);
						}
					});
				});
		}
	}
}