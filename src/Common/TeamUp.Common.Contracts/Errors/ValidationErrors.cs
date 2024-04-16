using RailwayResult;

namespace TeamUp.Common.Contracts.Errors;

public sealed record ValidationErrors(string Key, string Message, IDictionary<string, string[]> Errors) : Error(Key, Message);
