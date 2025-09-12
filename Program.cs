// Testable logic
static string Greet(string? input)
{
    // Trim first (if not null) then check for null/whitespace
    string? trimmed = input?.Trim();
    string safeName = string.IsNullOrWhiteSpace(trimmed) ? "friend" : trimmed;
    return $"Hello, {safeName}!";
}

// Tiny test runner with 3 PASS/FAIL cases
static void RunTests()
{
    // (input, expected) pairs - note: null is intentional in the last case
    var tests = new (string? input, string expected)[]
    {
        ("John", "Hello, John!"),
        ("   Bob   ", "Hello, Bob!"),
        (null, "Hello, friend!")
    };

    int passed = 0;

    for (int i = 0; i < tests.Length; i++)
    {
        string actual = Greet(tests[i].input);
        bool ok = actual == tests[i].expected; // true if equal

        if (ok)
        {
            passed++;
        }

        Console.WriteLine($"#{i + 1} input={(tests[i].input ?? "null")}\texpected=\"{tests[i].expected}\"\tactual=\"{actual}\"\t{(ok ? "PASS" : "FAIL")}");
    }

    Console.WriteLine($"Summary: {passed}/{tests.Length} passed");
}

//----------------------------------------------------------------------MAIN----------------------------------------------------------------//

// Ask for test mode: if 'y' run tests, else normal greet
Console.Write("Run test mode? (y/n): ");
string? mode = Console.ReadLine();

if (mode?.Trim().ToLower() == "y")
{
    RunTests();
    return; // stop after tests
}

Console.Write("Enter your name: ");
string? name = Console.ReadLine();
Console.WriteLine(Greet(name));