using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Blaze.SimTainer.Service.Api.Config.Mapping
{
	internal static class AutoMapperConfig
	{
		public static IServiceCollection AddAutoMapper(this IServiceCollection services)
		{
			return services.AddAutoMapper(x => x.AddProfile<MapProfile>(), typeof(Startup).Assembly);
		}
	}
}