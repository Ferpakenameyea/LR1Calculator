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

    public Result Launch(int startRuleIndex)
    {
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
                    closure.Transitions.Add((symbol, newClosure));
                }
            }
        }

        return new Result()
        {
            StartRuleIndex = startRuleIndex,
            Closures = Closures,
            ClosureToName = ClosureToName,
            Rules = Rules,
            NonTerminals = NonTerminals,
            Terminals = Terminals
        };
    }
}

public record Result
{
    public required List<Closure> Closures { get; init; }
    public required Dictionary<Closure, string> ClosureToName { get; init; }
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
            var newSet = (
                from item in set
                join rule in Context.Rules on item.NextSymbol equals rule.Left
                select new Item() 
                {
                    BaseRule = rule,
                    DotAt = 0,
                    Condition = item.RemainingSymbols == 1 ? item.Condition : item.Peek(1)
                }
            ).Union(set).ToHashSet();

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