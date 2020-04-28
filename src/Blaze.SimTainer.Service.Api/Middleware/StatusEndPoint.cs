using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace Blaze.SimTainer.Service.Api.Middleware
{
	public static class StatusEndpoint
	{
		public static void MapStatusEndpoint(this IEndpointRouteBuilder builder)
		{
			builder.MapGet("/status/", WriteStatus);
		}

		private static async Task WriteStatus(HttpContext context)
		{
			context.Response.ContentType = "text/json";
			await context.Response.WriteAsync(@"{ ""status"": ""OK"" }");
		}
	}
}