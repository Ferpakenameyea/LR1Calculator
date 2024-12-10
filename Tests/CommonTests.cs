namespace Tests;

public class CommonTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Rule a = "A -> aAB";
        System.Console.WriteLine(a);
    }

    [Test]
    public void GetClosureTest()
    {
        List<Rule> rules = [
            "G -> S",
            "S -> E",
            "E -> E-T",
            "E -> T",
            "T -> T*F",
            "T -> F",
            "F -> i",
            "F -> (E)",
        ];

        Solution context = new(rules);
        ClosureGenerator generator = new(context);
        var closure = generator.GetClosure(Item.FromRule(rules[0]));
        closure.Dump();
    }

    [Test]
    public void PPTExample()
    {
        List<Rule> rules = [
            "G -> S",
            "S -> aAd",
            "S -> bAc",
            "S -> aec",
            "S -> bed",
            "A -> e"
        ];

        Solution context = new(rules);
        ClosureGenerator generator = new(context);
        var closure = generator.GetClosure(Item.FromRule(rules[0]));
        closure.Dump();
        HashSet<Item> expected = [
            "G -> .S | #",
            "S -> .aAd | #",
            "S -> .bAc | #",
            "S -> .aec | #",
            "S -> .bed | #",
        ];
        
        Assert.That(expected.SetEquals(closure.Items));
    }

    [Test]
    public void PPTExample2()
    {
        List<Rule> rules = [
            "G -> S",
            "S -> aAd",
            "S -> bAc",
            "S -> aec",
            "S -> bed",
            "A -> e"
        ];

        Solution context = new(rules);
        var result = context.Launch(0);
        Assert.That(context.Closures.Count == 12);
        result.Dump();
    }
}
