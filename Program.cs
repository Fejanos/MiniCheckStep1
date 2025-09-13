using System.Text.Json;
using System;


// Testable logic
static string Greet(string? input)
{
    // Trim first (if not null) then check for null/whitespace
    string? trimmed = input?.Trim();
    string safeName = string.IsNullOrWhiteSpace(trimmed) ? "friend" : trimmed;
    return $"Hello, {safeName}!";
}

// Read tests from JSON
static TestCase[] LoadTests(string path)
{
    try
    {
        // 1) Read the file
        string json = File.ReadAllText(path);

        // 2) JSON -> C# Array (TestCase[])
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

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
static void RunTestsFromFile(string path)
{
    // If file does not exists
    if (!File.Exists(path))
    {
        Console.WriteLine($"[ERROR] File not found: {path}");
        return;
    }

    // From args path
    var tests = LoadTests(path);

    if (tests.Length == 0)
    {
        Console.WriteLine($"[INFO] No tests found.");
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

// args from command line
// dotnet run -- tests.json

// At least one params.
if (args.Length > 0)
{
    string path = args[0]; // args[0] = "tests.json"
    Console.WriteLine($"[INFO] Running tests from file: {path}");
    RunTestsFromFile(path);
    return;
}

// No params.

// Ask for test mode: if 'y' run tests
Console.Write("Run test mode? (y/n): ");
string? mode = Console.ReadLine();

if (mode?.Trim().ToLowerInvariant() == "y")
{
    RunTestsFromFile("tests.json"); // default json
    return; // stop after tests
}

// else normal greet
Console.Write("Enter your name: ");
string? name = Console.ReadLine();
Console.WriteLine(Greet(name));


//-----------------------------------------------------------------------------------------------------------------------------------------//
// Test case for JSON
public readonly record struct TestCase(string? input, string expected);