using System.Text.Json;


// Testable logic
static string Greet(string? input)
{
    // Trim first (if not null) then check for null/whitespace
    string? trimmed = input?.Trim();
    string safeName = string.IsNullOrWhiteSpace(trimmed) ? "friend" : trimmed;
    return $"Hello, {safeName}!";
}

// Read tests from JSON
static TestCase[] LoadTests(string path = "tests.json")
{
    try
    {
        // 1) Read the file
        string json = File.ReadAllText(path);

        // 2) JSON -> C# Array (TestCase[])
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        TestCase[]? cases = JsonSerializer.Deserialize<TestCase[]>(json, options);

        // 3) If null, then throw an exception
        if (cases is null)
        {
            throw new Exception("No test cases in JSON.");
        }
        return cases;
    }
    catch (Exception ex)
    {
        // Messages and a fallback empty array
        Console.WriteLine($"[WARN] Could not load {path}: {ex.Message}");
        return Array.Empty<TestCase>();
    }
}


// Test runner
static void RunTestsFromFile()
{
    // Default: "tests.json"
    var tests = LoadTests();

    if (tests.Length == 0)
    {
        Console.WriteLine("No tests found. Make sure tests.json exists next to Program.cs.");
        return;
    }

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

if (mode?.Trim().ToLowerInvariant() == "y")
{
    RunTestsFromFile();
    return; // stop after tests
}

Console.Write("Enter your name: ");
string? name = Console.ReadLine();
Console.WriteLine(Greet(name));


//-----------------------------------------------------------------------------------------------------------------------------------------//
// Test case for JSON
public readonly record struct TestCase(string? input, string expected);