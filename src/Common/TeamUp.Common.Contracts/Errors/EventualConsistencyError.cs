using RailwayResult;

namespace TeamUp.Common.Contracts.Errors;

public sealed record EventualConsistencyError(string Key, string Message) : Error(Key, Message);
