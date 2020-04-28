using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Blaze.SimTainer.Service.Api.Middleware
{
	public class RequestMetricsMiddleware
	{
		private static readonly ImmutableHashSet<string> _excludePaths = ImmutableHashSet.Create(
			StringComparer.OrdinalIgnoreCase,
			"/",
			"/status/", "/status",
			"/metrics/", "/metrics",
			"/swagger", "/swagger/", "/swagger/index.html", "/swagger/favicon-32x32.png", "/swagger/v1/swagger.json",
			"/swagger/swagger-ui.css", "/swagger/swagger-ui-standalone-preset.js", "/swagger/swagger-ui-bundle.js ",
			"/game/index.html", "/game/TemplateData/style.css", "/game/TemplateData/UnityProgress.js",
			"/game/Build/UnityLoader.js",
			"/game/TemplateData/webgl-logo.png", "/game/TemplateData/fullscreen.png",
			"/game/TemplateData/progressEmpty.Light.png",
			"/game/TemplateData/progressLogo.Light.png", "/game/TemplateData/progressFull.Light.png",
			"/game/Build/WebGL.json", "/game/Build/WebGL.json",
			"/game/Build/WebGL.data.unityweb",
			"/game/Build/WebGL.wasm.framework.unityweb",
			"/game/Build/WebGL.wasm.code.unityweb",
			"/hubs/game", "/hubs/game/negotiate", "/images/cpu_icon.png", "/images/ram_icon.png", "/images/globe_icon.png"
		);

		private static readonly Summary _summary = Metrics.CreateSummary(
			"service_trace_elapsed_time_nanoseconds",
			"summarizes the call duration for incoming requests",
			new SummaryConfiguration
			{
				AgeBuckets = 5,
				BufferSize = 500,
				LabelNames = new[] { "trace_name", "status", "method" },
				MaxAge = new TimeSpan(0, 0, 2, 0),
				Objectives = new List<QuantileEpsilonPair>
				{
					new QuantileEpsilonPair(0.01, 0.001),
					new QuantileEpsilonPair(0.05, 0.005),
					new QuantileEpsilonPair(0.5, 0.05),
					new QuantileEpsilonPair(0.9, 0.01),
					new QuantileEpsilonPair(0.95, 0.005),
					new QuantileEpsilonPair(0.99, 0.001),
					new QuantileEpsilonPair(0.999, 0.0001)
				}.AsReadOnly()
			}
		);

		private static readonly Counter _counter = Metrics.CreateCounter(
			"service_http_server",
			"counts all requests to the service",
			"trace_name", "status", "method");

		private readonly ILogger<RequestMetricsMiddleware> _logger;

		private readonly RequestDelegate _next;
		private readonly Dictionary<string, string> _traceNames = new Dictionary<string, string>();

		public RequestMetricsMiddleware(RequestDelegate next, ILogger<RequestMetricsMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public static double ToNanoSeconds(long ticks)
		{
			return ticks * 1000000000.0 / Stopwatch.Frequency;
		}

		public async Task Invoke(HttpContext context)
		{
			Stopwatch sw = Stopwatch.StartNew();

			try
			{
				await _next(context);
			}
			finally
			{
				double durationInNanoseconds = ToNanoSeconds(sw.ElapsedTicks);
				string path = context.Request.Path.Value;

				if (!_excludePaths.Contains(path))
				{
					string traceName = GetTraceName(path, context);

					if (traceName == null)
					{
						_logger.LogWarning(string.Format("Trace-name null encountered. {0} {1} {2}",
							$"{context.Request.Method} {path}",
							$"Controller: {context.GetRouteValue("controller")}",
							$"Action: {context.GetRouteValue("action")}"));
					}
					else
					{
						string method = context.Request.Method;
						string status = context.Response.StatusCode.ToString();

						_counter.Labels(traceName, status, method).Inc();
						_summary.Labels(traceName, status, method).Observe(durationInNanoseconds);
					}
				}
			}
		}

		private string GetTraceName(string uri, HttpContext httpContext)
		{
			if (_traceNames.ContainsKey(uri))
				return _traceNames[uri];

			if (httpContext.GetRouteData() == null)
				return null;

			string actionName = httpContext.GetRouteValue("action")?.ToString().ToLowerInvariant();
			string controllerName = httpContext.GetRouteValue("controller")?.ToString().ToLowerInvariant();

			// When a response is cached by the response caching middleware controller and action will be empty because no action was invoked.
			// In that case we need to use the trace-name of the first call to that endpoint which are stored in _traceNames.
			string traceName = string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(actionName)
				? null
				: $"{controllerName}_{actionName}";

			if (!string.IsNullOrEmpty(traceName) && httpContext.Response.StatusCode == 200)
				_traceNames[uri] = traceName;

			return traceName;
		}
	}
}