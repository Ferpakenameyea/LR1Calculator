public class NotLR1Exception((string, Symbol, Action) conflict1, (string, Symbol, Action) conflict2) : Exception
{
    public (string, Symbol, Action) Conflict1 { get; init; } = conflict1;
    public (string, Symbol, Action) Conflict2 { get; init; } = conflict2;

    public override string Message => $"Conflict found! When at state {Conflict1.Item1} and meet {Conflict1.Item2}, the action is {Conflict1.Item3} conflicts with: When at state {Conflict2.Item1} and meet {Conflict2.Item2}, the action is {Conflict2.Item3}";

}