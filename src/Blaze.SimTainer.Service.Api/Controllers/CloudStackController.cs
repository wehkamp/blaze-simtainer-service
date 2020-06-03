using Blaze.SimTainer.Service.Api.Dtos.Game;
using Blaze.SimTainer.Service.Api.Interfaces;
using Blaze.SimTainer.Service.Api.Services;
using Blaze.SimTainer.Service.Providers.CloudStack.Services;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Blaze.SimTainer.Service.Api.Controllers
{
	/// <summary>
	///     The CloudStack Controller.
	/// </summary>
	/// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
	[ApiController]
	[ApiVersion("1")]
	[Route("v{version:apiVersion}/[controller]")]
	public class CloudStackController : ControllerBase
	{
		private readonly CloudStackService _cloudStackService;
		private readonly IDataConverter<IApplication> _cloudStackDataTransformationService;
		private readonly IMemoryCache _cacheAdapter;

		public CloudStackController(
			CloudStackService cloudStackService, IMemoryCache memoryCache
		)
		{
			_cloudStackService = cloudStackService;
			_cloudStackDataTransformationService = new CloudStackDataConverterService();
			_cacheAdapter = memoryCache;
		}

		/// <summary>
		/// This function will return the current state of the game. It is cached for 60 seconds to avoid heavy load on the server.
		/// </summary>
		/// <returns>The current game dto.</returns>
		[HttpGet("game")]
		[ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
		[ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
		public ActionResult<GameDto> GetGameResult()
		{
			// Look for cache key.
			if (_cacheAdapter.TryGetValue(typeof(GameDto).Name, out GameDto cacheEntry)) return cacheEntry;

			// Key not in cache, so get data.
			cacheEntry =
				_cloudStackDataTransformationService.GenerateGameDto(_cloudStackService.Applications
					.OrderBy(x => x.Identifier).ToList());

			// Set cache options.
			MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
				// Keep in cache for this time, reset time if accessed.
				.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

			// Save data in cache.
			_cacheAdapter.Set(typeof(GameDto).Name, cacheEntry, cacheEntryOptions);

			return cacheEntry;
		}


		/// <summary>
		/// This function will return the dashboard url of a neighbourhood.
		/// </summary>
		/// <returns>The current game dto.</returns>
		[HttpGet("game/url")]
		[ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
		[ProducesResponseType(typeof(NeighbourhoodUrlDto), StatusCodes.Status200OK)]
		public ActionResult<NeighbourhoodUrlDto> GetDashboardUrResult([Required] string identifier)
		{
			// Look for cache key.
			if (_cacheAdapter.TryGetValue(identifier, out NeighbourhoodUrlDto cacheEntry)) return cacheEntry;

			string dashboardUrl = _cloudStackService.GetDashboardUrl(identifier);
			if (dashboardUrl == null)
			{
				return NotFound();
			}

			// Key not in cache, so set the data.
			cacheEntry = new NeighbourhoodUrlDto {Url = dashboardUrl};

			// Set cache options.
			MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
				// Keep in cache for this time, reset time if accessed.
				.SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

			// Save data in cache.
			_cacheAdapter.Set(identifier, cacheEntry, cacheEntryOptions);

			return cacheEntry;
		}

		/// <summary>
		/// This function will kill a visualized object real-time (a building/container).
		/// </summary>
		/// <param name="identifier">The identifier of a container.</param>
		/// <param name="force">Set force to true if you want to skip the check if there are at least 2 containers running.</param>
		/// <returns>An accepted status code.</returns>
		[HttpDelete("game")]
		[ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public ActionResult DeleteGameResult([Required] string identifier, bool force = false)
		{
			if (identifier == null)
				return BadRequest();

			if (_cloudStackService.KillTask(identifier, force))
				return Accepted();

			return UnprocessableEntity();
		}
	}
}