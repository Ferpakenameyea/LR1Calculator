/// <summary>
/// LR(1)文法中的规则
/// </summary>
public struct Rule
{
    /// <summary>
    /// 左侧
    /// </summary>
    public Symbol Left { get; init; }
    /// <summary>
    /// 右侧
    /// </summary>
    public List<Symbol> Right { get; init; }

    public Rule(Symbol left, params Symbol[] right)
    {
        Left = left;
        Right = new List<Symbol>(right);
    }

    public Rule(Symbol left, List<Symbol> right)
    {
        Left = left;
        Right = right;
    }

    public override string ToString()
    {
        return $"{Left} -> {string.Join("", Right)}";
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        var other = (Rule)obj;
        return Left.Equals(other.Left) && Right.SequenceEqual(other.Right);
    }
    
    public override int GetHashCode()
    {
        return Left.GetHashCode() ^ Right.GetContentHashCode();
    }

    public static implicit operator Rule(string rawpres) 
    {
        var strs = rawpres.Split(" -> ");
        if (strs.Length != 2)
        {
            throw new ArgumentException("规则格式错误");
        }
        var leftRaw = strs[0];
        var rightRaw = strs[1];
        if (leftRaw.Length != 1)
        {
            throw new ArgumentException("规则格式错误，左侧只能有一个符号");
        }

        Symbol left = leftRaw[0];
        List<Symbol> right = rightRaw.Select(c => (Symbol)c).ToList();
        return new Rule(left, right);
    }
}

/// <summary>
/// LR(1)文法中的项目
/// </summary>
public struct Item
{
    /// <summary>
    /// 项目对应的原始规则
    /// </summary>
    public Rule BaseRule { get; init; }
    
    /// <summary>
    /// 表示点在第几个空位（第一个空位是0）
    /// </summary>
    public int DotAt { get; init; }
    public Symbol Condition { get; init; }

    private int EmptySpaces { get => BaseRule.Right.Count + 1; }

    public Symbol NextSymbol => IsComplete ? '#' : BaseRule.Right[DotAt];
    public bool IsComplete => DotAt == EmptySpaces - 1;
    public int RemainingSymbols => EmptySpaces - 1 - DotAt;

    public Symbol Peek(int skip) 
    {
        if (DotAt + skip >= EmptySpaces - 1)
        {
            throw new ArgumentException("超出范围");
        }

        return BaseRule.Right[DotAt + skip];
    }

    public Item(Rule baseRule, int dotAt, Symbol condition)
    {
        BaseRule = baseRule;
        DotAt = dotAt;
        Condition = condition;
        if (dotAt < 0 || dotAt > EmptySpaces)
        {
            throw new ArgumentException("项目中的点超出范围");
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        var other = (Item)obj;
        var result = BaseRule.Equals(other.BaseRule) && DotAt == other.DotAt && Condition.Equals(other.Condition);
        return result;
    }
    
    public override int GetHashCode()
    {
        return BaseRule.GetHashCode() ^ DotAt.GetHashCode() ^ Condition.GetHashCode();
    }

    public override string ToString()
    {
        var right = BaseRule.Right;
        return $"{BaseRule.Left} -> {string.Join("", right.Take(DotAt))}.{string.Join("", right.Skip(DotAt))}|{Condition}";
    }

    public string BodyString() 
    {
        var right = BaseRule.Right;
        return $"{BaseRule.Left} -> {string.Join("", right.Take(DotAt))}.{string.Join("", right.Skip(DotAt))}";
    }

    public static Item FromRule(Rule rule) => FromRule(rule, '#');
    public static Item FromRule(Rule rule, Symbol condition)
    {
        return new Item()
        {
            BaseRule = rule,
            DotAt = 0,
            Condition = condition
        };
    }

    public bool CanApply(Symbol input) => this.NextSymbol == input;
    public Item Apply(Symbol input) 
    {
        if (!CanApply(input))
        {
            throw new ArgumentException("无法应用输入");
        }

        return new Item(BaseRule, this.DotAt + 1, Condition);
    }

    public static implicit operator Item(string input) 
    {
        var parts = input.Split(" | ");
        if (parts.Length != 2)
        {
            throw new ArgumentException("项目格式错误");
        }
        var conditionPart = parts[1];
        if (conditionPart.Length != 1)
        {
            throw new ArgumentException("项目格式错误，条件只能是一个符号");
        }
        Symbol condition = conditionPart[0];
        
        input = parts[0];
        var raw = input.Split(" -> ");
        if (raw.Length != 2)
        {
            throw new ArgumentException("项目格式错误");
        }
        if (raw[0].Length != 1)
        {
            throw new ArgumentException("项目格式错误，左侧只能有一个符号");
        }
        Symbol left = raw[0][0];
        List<Symbol> right = raw[1].Where(c => c != '.').Select(c => (Symbol)c).ToList(); // 去掉点
        if (right.Count != raw[1].Length - 1)
        {
            throw new ArgumentException("项目格式错误，右侧符号数量不正确");
        }
        var dotAt = raw[1].IndexOf('.');
        return new Item(new Rule(left, right), dotAt, condition);
    }
}

/// <summary>
/// LR(1)文法中的符号
/// </summary>
public struct Symbol
{
    /// <summary>
    /// 此符号的值，如果是大写字母，则为非终结符，否则为终结符
    /// </summary>
    public char Value { get; init; }
    public bool IsTerminal => char.IsLower(Value);

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        return this.Value == ((Symbol)obj).Value;        
    }
    
    public override int GetHashCode() => this.Value.GetHashCode();

    public override string ToString()
    {
        return this.Value.ToString();
    }

    public static implicit operator Symbol(char value) => new() { Value = value };
    public static implicit operator char(Symbol symbol) => symbol.Value;
}