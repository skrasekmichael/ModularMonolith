using RailwayResult;

namespace TeamUp.Common.Infrastructure.Processing.IntegrationEvents;

public sealed class RetryToConsumeIntegrationEventException(Error error) : Exception(error.ToString());
