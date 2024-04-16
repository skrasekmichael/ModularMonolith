using RailwayResult;

namespace TeamUp.Common.Contracts.Errors;

public sealed record ConcurrencyError(string Key, string Message) : Error(Key, Message);
