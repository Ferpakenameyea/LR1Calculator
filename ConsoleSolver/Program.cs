public class Program
{
    public static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("Welcome to LR(1) console solution!");
        System.Console.WriteLine("This is only a closure calculator for compiler work.");
        System.Console.Write("Input the ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.Write("Rules set size");
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine(":");

        int size;
        while (!int.TryParse(Console.ReadLine(), out size) || size == 0 || size > 20 || size < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Ah-oh, that's not a valid number. Try again, a valid number is an integer between 1 and 20:");
            Console.ForegroundColor = ConsoleColor.Green;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.Write("Input the ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("Rules");
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("How to input (example):");
        Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("A -> BC");
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("Only one non-terminal at left, and more than zero symbols at right, an arrow -> to seperate");
        Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine("Don't use space in left and right part, but there are spaces at the start and the end of the arrow");
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("Wrong:");
        Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine("A->BC");
        System.Console.WriteLine("A ->BC");
        System.Console.WriteLine("A-> B  C");
        System.Console.WriteLine("Ab -> BC");
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("Correct:");
        System.Console.WriteLine("A -> BC");
        System.Console.WriteLine("B -> abC");
        List<Rule> rules = [];
        for (int i = 0; i < size ; i++)
        {
            try
            {
                var rule = (Rule)(Console.ReadLine() ?? "");
                rules.Add(rule);
                System.Console.WriteLine("OK!");
            } catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Ah-oh, that's not a valid rule.");
                Console.ForegroundColor = ConsoleColor.Green;
                i--;
            }
        }
        System.Console.WriteLine("Your rules:");
        foreach (var rule in rules)
        {
            System.Console.WriteLine(rule);
        }

        System.Console.WriteLine("Which is that start? (begins from 0):");
        int start;
        while (!int.TryParse(Console.ReadLine(), out start) || start < 0 || start >= size)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Ah-oh, that's not a valid number. Try again, a valid number is an integer between 0 and your rules set size(exclusive)");
            Console.ForegroundColor = ConsoleColor.Green;
        }
        Solution solution = new(rules);
        try
        {
            var result = solution.Launch(start);
            result.Dump();
        } catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("Something went wrong, try again");
            System.Console.WriteLine(e);
        }

        Console.ResetColor();
    }
}