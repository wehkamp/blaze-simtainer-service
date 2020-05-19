using System;
using System.Runtime.CompilerServices;
using Blaze.SimTainer.Service.Providers.CloudStack.Models.Apis.Prometheus;
using Blaze.SimTainer.Service.Providers.Shared.Interfaces;

[assembly: InternalsVisibleTo("Blaze.SimTainer.Service.Api.Integration.UnitTests")]
namespace Blaze.SimTainer.Service.Providers.CloudStack.UnitTests.Factories
{
	internal class PrometheusFactory
	{
		private readonly Random _randomNumber = new Random();

		/// <summary>
		/// This function will create  a prometheus result based on the given parameters. ContainerIdentifier and instance number are always random.
		/// </summary>
		/// <param name="application"></param>
		/// <param name="job"></param>
		/// <param name="validResults"></param>
		/// <returns></returns>
		public PrometheusResult Create(IApplication application, string? job, bool validResults = true)
		{
			return validResults
				? GenerateValidPrometheusResults(application, job)
				: GenerateInvalidPrometheusResults();
		}

		/// <summary>
		/// Generate a Prometheus result based on a application mock.
		/// </summary>
		/// <param name="applicationMock"></param>
		/// <param name="job"></param>
		/// <returns></returns>
		private PrometheusResult GenerateValidPrometheusResults(IApplication applicationMock, string job)
		{
			return new PrometheusResult
			{
				Metric = new PrometheusMetric
				{
					Identifier = $"blaze-{_randomNumber.Next(1, 1000)}",
					Instance = $"10.0.0.{_randomNumber.Next(1, 254)}",
					Job = job,
					Role = applicationMock.Role,
					Name = $"{applicationMock.Name}-instance",
					Status = "200",
					TraceName = "*"
				},
				Value = new[] {"10", "10.0"}
			};
		}

		/// <summary>
		/// Generate a list with invalid prometheus results. They contain information which you normally should not get.
		/// </summary>
		/// <returns></returns>
		private PrometheusResult GenerateInvalidPrometheusResults()
		{
			return new PrometheusResult
			{
				Metric = new PrometheusMetric
				{
					Identifier = $"blaze-{_randomNumber.Next(1, 1000)}",
					Instance = $"10.0.0.{_randomNumber.Next(1, 254)}",
					Job = "test123",
					Role = "this-role-does-not-exists",
					Name = "something",
					Status = "495921",
					TraceName = "*"
				},
				Value = new[] {"10", "sadfg.asdf"}
			};
		}
	}
}