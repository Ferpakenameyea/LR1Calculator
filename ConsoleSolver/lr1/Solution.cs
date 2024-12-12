using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using System.Runtime.ExceptionServices;
using ConsoleTables;

public class Solution
{
    public List<Closure> Closures { get; init; } = new();
    public HashSet<Symbol> NonTerminals { get; init; } = [];
    public HashSet<Symbol> Terminals { get; init; }= [];
    public List<Rule> Rules { get; init; }
    public Dictionary<Closure, string> ClosureToName { get; init; } = new();
    private readonly ClosureGenerator closureGenerator;
    public Solution(List<Rule> rules)
    {
        this.Rules = rules;
        closureGenerator = new ClosureGenerator(this);
    }

    public Dictionary<Symbol, HashSet<Symbol>> First = [];
    public Dictionary<Symbol, HashSet<Symbol>> Follow = [];

    public ClosureResult Launch(int startRuleIndex)
    {
        NonTerminals.UnionWith(Rules.Select(x => x.Left));
        NonTerminals.UnionWith(Rules.SelectMany(x => x.Right).Where(x => !x.IsTerminal));
        Terminals.UnionWith(Rules.SelectMany(x => x.Right).Where(x => x.IsTerminal));
        Terminals.Add('#');

        GenFirstAndFollow();

        var start = Rules[startRuleIndex];
        var startItem = Item.FromRule(start);
        var startClosure = closureGenerator.GetClosure(startItem);
        ClosureToName.Add(startClosure, "I0");
        Closures.Add(startClosure);

        for (int i = 0; i < Closures.Count; i++)
        {
            var closure = Closures[i];
            var acceptables = 
                (from c in closure.Items
                 where !c.IsComplete
                 select c.NextSymbol).Distinct();
            foreach (var symbol in acceptables)
            {
                var generated = closure.Accept(symbol);
                var newClosure = closureGenerator.GetClosure(generated);
                if (!Closures.Contains(newClosure))
                {
                    Closures.Add(newClosure);
                    ClosureToName.Add(newClosure, "I" + (Closures.Count - 1));
                } 
                else 
                {
                    newClosure = Closures.First(c => c.Equals(newClosure));
                }
                closure.Transitions.Add((symbol, newClosure));
            }
        }

        var NameToClosure = new Dictionary<string, Closure>();
        foreach (var pair in ClosureToName)
        {
            NameToClosure.Add(pair.Value, pair.Key);
        }

        return new ClosureResult()
        {
            StartRuleIndex = startRuleIndex,
            Closures = Closures,
            ClosureToName = ClosureToName,
            NameToClosure = NameToClosure,
            Rules = Rules,
            NonTerminals = NonTerminals,
            Terminals = Terminals
        };
    }

    private void GenFirstAndFollow()
    {
        foreach (var symbol in NonTerminals)
        {
            First.Add(symbol, new());
            Follow.Add(symbol, new());
        }

        // gen first phase 1
        foreach (var symbol in NonTerminals)
        {
            var targets = Rules.Where(rule => rule.Left.Equals(symbol))
                .Where(rule => rule.Right.First().IsTerminal)
                .Select(rule => rule.Right.First());
            var set = First[symbol];
            set.UnionWith(targets);
        }

        // gen first phase 2
        bool changed = false;
        do
        {
            changed = false;
            foreach (var symbol in NonTerminals)
            {
                var set = First[symbol];
                var affectors = from rule in Rules
                                where rule.Left.Equals(symbol) && 
                                    !rule.Right.First().IsTerminal && 
                                    !rule.Right.First().Equals(symbol)
                                select rule.Right.First();
                foreach (var affector in affectors)
                {
                    var before = set.Count;
                    set.UnionWith(First[affector]);
                    if (set.Count != before)
                    {
                        changed = true;
                    }
                }
            }
        } while (changed);

        // gen follow
        do
        {
            foreach (var symbol in NonTerminals)
            {
                var set = Follow[symbol];
                var rules = from rule in Rules
                            where rule.Right.Contains(symbol)
                            select rule;
                foreach (var rule in rules)
                {
                    var index = rule.Right.IndexOf(symbol);
                    if (index == rule.Right.Count - 1)
                    {
                        var before = set.Count;
                        set.UnionWith(Follow[rule.Left]);
                        changed = set.Count != before;
                    }
                    else
                    {
                        var nextSymbol = rule.Right[index + 1];
                        if (nextSymbol.IsTerminal)
                        {
                            set.Add(nextSymbol);
                        }
                        else
                        {
                            var before = Follow[symbol].Count;
                            set.UnionWith(First[nextSymbol]);
                            changed = set.Count != before;
                        }
                    }
                }
            }
        } while (changed);
        
        return;
    }
}

public record ClosureResult
{
    public required List<Closure> Closures { get; init; }
    public required Dictionary<Closure, string> ClosureToName { get; init; }
    public required Dictionary<string, Closure> NameToClosure { get; init; }
    public required int StartRuleIndex { get; init; }
    public required List<Rule> Rules { get; init; }
    public required HashSet<Symbol> NonTerminals { get; init; }
    public required HashSet<Symbol> Terminals { get; init; }

    public void Dump()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("LR(1) Solution:");
        System.Console.WriteLine($"Closure count: {Closures.Count} (from I0 to I{Closures.Count - 1})");
        foreach (var closure in Closures)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"Closure {ClosureToName[closure]}:");
            
            Console.ForegroundColor = ConsoleColor.White;
            closure.Dump();

            Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("Transitions:");
            foreach (var (symbol, nextClosure) in closure.Transitions)
            {
                System.Console.Write("When receive ");
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.Write(symbol);
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.Write(" go to ");
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine(ClosureToName[nextClosure]);
                System.Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine("==========================");
        }
    }
}

public class Closure
{
    public HashSet<Item> Items { get; private set; }
    public List<(Symbol, Closure)> Transitions { get; private set; } = new();

    public Closure(HashSet<Item> items)
    {
        Items = items;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        Closure other = (Closure)obj;
        return this.Items.SetEquals(other.Items);
    }
    
    public override int GetHashCode()
    {
        return Items.Aggregate(0, (hash, item) => hash ^ item.GetHashCode());
    }

    public void Dump()
    {
        var table = new ConsoleTable("Item", "Condition");
        foreach (var item in Items.OrderBy(x => x.ToString()))
        {
            table.AddRow(item.BodyString(), item.Condition.ToString());
        }
        table.Write();
    }

    public IEnumerable<Item> Accept(Symbol symbol)
    {
        return 
            from item in Items
            where item.NextSymbol == symbol
            select new Item(item.BaseRule, item.DotAt + 1, item.Condition);
    }
}

public class ClosureGenerator
{
    public Solution Context { get; init; }
    public ClosureGenerator(Solution context)
    {
        Context = context;
    }

    public Closure GetClosure(IEnumerable<Item> items)
    {
        HashSet<Item> set = new(items);
        while (true)
        {
            var query1 = (
                from item in set
                join rule in Context.Rules on item.NextSymbol equals rule.Left
                where item.RemainingSymbols == 1 || item.Peek(1).IsTerminal
                select new Item() 
                {
                    BaseRule = rule,
                    DotAt = 0,
                    Condition = item.RemainingSymbols == 1 ? item.Condition : item.Peek(1)
                }
            ).Union(set);

            var firsts = from item in set
                         join rule in Context.Rules on item.NextSymbol equals rule.Left
                         where item.RemainingSymbols >= 2 && !item.Peek(1).IsTerminal
                         select Context.First[item.Peek(1)].Select(symbol => new Item(rule, 0, symbol));

            var newSet = query1.Union(firsts.SelectMany(x => x)).ToHashSet();

            if (newSet.SetEquals(set))
            {
                return new Closure(set);
            }

            set = newSet;
        }
    }

    public Closure GetClosure(params Item[] items)
    {
        return GetClosure((IEnumerable<Item>)items);
    }
}

public class AnalyzeTableGenerator
{
    private readonly ClosureResult closureResult;
    private Dictionary<(Closure, Symbol), Action> result = new();

    public AnalyzeTableGenerator(ClosureResult closureResult)
    {
        this.closureResult = closureResult;
    }

    public AnalyzeTable Generate()
    {
        foreach (var closure in closureResult.Closures)
        {
            TranslateTransitions(closure);
            TranslateCompleted(closure);
        }

        return new AnalyzeTable()
        {
            Table = result,
            ClosureResult = closureResult,
        };
    }

    private void TranslateCompleted(Closure closure)
    {
        var completedItems = from item in closure.Items
                             where item.IsComplete
                             select item;
        foreach (var item in completedItems)
        {
            var matchingRuleIndex = closureResult.Rules.FindIndex(x => x.Equals(item.BaseRule));
            if (matchingRuleIndex == -1)
            {
                throw new Exception("Rule not found: " + item.BaseRule);
            }
            Action actionToAdd = matchingRuleIndex == this.closureResult.StartRuleIndex ?
                new() { ActionType = Action.Type.Accept, Index = -1 } :
                new() { ActionType = Action.Type.Reduce, Index = matchingRuleIndex };
            AddToTable((closure, item.Condition), actionToAdd);
        }
    }

    private void TranslateTransitions(Closure closure)
    {
        foreach (var (symbol, nextClosure) in closure.Transitions)
        {
            if (!symbol.IsTerminal)
            {
                AddToTable((closure, symbol), new()
                {
                    ActionType = Action.Type.Goto,
                    Index = ClosureIndex(closureResult.ClosureToName[nextClosure])
                });
                continue;
            }

            // is terminal
            AddToTable((closure, symbol), new()
            {
                ActionType = Action.Type.Shift,
                Index = ClosureIndex(closureResult.ClosureToName[nextClosure])
            });
        }
    }

    private void AddToTable((Closure, Symbol) position, Action action)
    {
        if (result.TryGetValue(position, out var resultValue))
        {
            var existingAction = resultValue;
            if (!existingAction.ActionType.Equals(action))
            {
                throw new NotLR1Exception(
                    (closureResult.ClosureToName[position.Item1], position.Item2, action),
                    (closureResult.ClosureToName[position.Item1], position.Item2, existingAction) 
                );
            }
            return;
        }
        result.Add(position, action);
    }

    private static int ClosureIndex(string closureName)
    {
        return int.Parse(closureName[1..]);
    }
}

public record AnalyzeTable
{
    public required ClosureResult ClosureResult { get; init; }
    public required Dictionary<(Closure, Symbol), Action> Table { get; init; }
    public void Dump()
    {
        var terminals = ClosureResult.Terminals.OrderBy(x => x.Value).ToList();
        var nonTerminals = ClosureResult.NonTerminals.OrderBy(x => x.Value).ToList();
        var columns = new List<string> { "Closure" };

        columns.AddRange(terminals.Select(x => x.Value.ToString()));
        columns.AddRange(nonTerminals.Select(x => x.Value.ToString()));

        var table = new ConsoleTable(columns.ToArray());

        foreach (var closure in ClosureResult.Closures)
        {
            List<string> row = [ClosureResult.ClosureToName[closure], .. Enumerable.Repeat("error", terminals.Count + nonTerminals.Count)]; // 第一列是闭包信息

            for (int i = 0; i < terminals.Count; i++)
            {
                if (Table.TryGetValue((closure, terminals[i]), out var action))
                {
                    row[i + 1] = action.ToString();
                }
            }

            for (int i = 0; i < nonTerminals.Count; i++)
            {
                if (Table.TryGetValue((closure, nonTerminals[i]), out var action))
                {
                    row[i + 1 + terminals.Count] = action.ToString();
                }
            }

            table.AddRow(row.ToArray());
        }

        table.Write();
    }

}



public struct Action
{
    public enum Type
    {
        Shift,
        Goto,
        Reduce,
        Accept
    }

    public required Type ActionType { get; init; }
    public required int Index { get; init; }
    public string GetMessage()
    {
        return this.ActionType switch 
        {
            Type.Shift => "Shift and turn to state " + this.Index,
            Type.Reduce => "Reduce by rule " + this.Index,
            Type.Accept => "Accepted",
            Type.Goto => "Turn to state " + this.Index,
            _ => throw new Exception("Unknown action type")
        };
    }

    public override string ToString()
    {
        return this.ActionType switch
        {
            Type.Shift => "s" + this.Index,
            Type.Reduce => "r" + this.Index,
            Type.Accept => "acc",
            Type.Goto => this.Index.ToString(),
            _ => throw new Exception("Unknown action type")
        };
    }
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        var other = (Action)obj;
        return this.ActionType == other.ActionType && this.Index == other.Index;
    }
    
    public override int GetHashCode()
    {
        return this.ActionType.GetHashCode() ^ this.Index.GetHashCode();
    }
}