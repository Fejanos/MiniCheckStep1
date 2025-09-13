using System;                   // basic types, Console, Environment
using System.Text.Json;         // JSON (de)serialization
using System.Text.RegularExpressions; // regex for whitespace normalization


// ----------------------------- TESTABLE LOGIC --------------------------

static string Greet(string? input)
{
    // Trim first (if not null) then check for null/whitespace
    string? trimmed = input?.Trim();
    string safeName = string.IsNullOrWhiteSpace(trimmed) ? "friend" : trimmed;
    return $"Hello, {safeName}!";
}

// --------------------------- JSON LOADING LOGIC ---------------------------

static (TestCase[] tests, CompareOptions options) LoadTestFile(string path)
{
    // Read file into a single string
    string json = File.ReadAllText(path);

    // Case-insensitive property names (so "Input" and "input" both work).
    var opts = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    // Try the simple array form first
    try
    {
        var arr = JsonSerializer.Deserialize<TestCase[]>(json, opts);

        if (arr is not null)
        {
            // If the file was the simple array form, use default compare options
            return (arr, new CompareOptions(
                ignoreCase: false,
                normalizeWhitespace: false,
                trimOutput: false
            ));
        }
    }
    catch (Exception )
    {
        // If both forms fail, we throw later
    }

    // Try the extended object form (with "tests" + optional "compare")
    var obj = JsonSerializer.Deserialize<TestFile>(json, opts) ?? throw new Exception("[WARN] Invalid JSON format.");

    // If Compare is missing -> default options
    var effective = obj.Compare ?? new CompareOptions();
    // If Tests is missing -> empty array
    var tests = obj.Tests ?? Array.Empty<TestCase>();
    return (tests, effective);
}

// ------------------------ OUTPUT NORMALIZATION (COMPARE) ------------------

static string PrepareForCompare(string s, CompareOptions opt)
{
    string r = s;

    if (opt.trimOutput)
    {
        r = r.Trim(); // remove leading/trailing whitespace
    }

    if (opt.normalizeWhitespace)
    {
        // Collapse all whitespace runs to a single space and trim ends
        r = Regex.Replace(r, @"\s+", " ").Trim();
    }

    if (opt.ignoreCase)
    {
        // Culture-invariant lowercase for robust case-insensitive compare
        r = r.ToLowerInvariant();
    }

    return r;
}

// ------------------------------ TEST RUNNER -------------------------------

static void RunTestsFromFile(string path, CompareOptions? overrideFromCli = null)
{
    // If file does not exists
    if (!File.Exists(path))
    {
        Console.WriteLine($"[ERROR] File not found: {path}");
        Environment.ExitCode = 2;
        return;
    }

    TestCase[] tests;
    CompareOptions fileOptions;

    try
    {
        (tests, fileOptions) = LoadTestFile(path);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Could not parse '{path}': {ex.Message}");
        Environment.ExitCode = 2; // parsing error
        return;
    }

    if (tests.Length == 0)
    {
        Console.WriteLine($"[INFO] No tests found.");
        Environment.ExitCode = 1; // treat "no tests" as failure in CI contexts
        return;
    }

    // Use CLI override if provided; otherwise the options coming from file
    var opt = overrideFromCli ?? fileOptions;

    int passed = 0;

    // Header
    Console.WriteLine("Idx | Input        | Expected             | Actual               | Result");
    Console.WriteLine("----+--------------+----------------------+----------------------+--------");

    for (int i = 0; i < tests.Length; i++)
    {
        // Produce actual output by calling our testable function
        string actual = Greet(tests[i].input);

        // Prepare both sides for comparison according to options
        string expForCmp = PrepareForCompare(tests[i].expected, opt);
        string actForCmp = PrepareForCompare(actual, opt);

        bool ok = expForCmp == actForCmp; // true if equal

        if (ok)
        {
            passed++;
        }

        // Null-coalescing '??' -> if input is null, show the literal "null" in the table
        string inputDisp = tests[i].input ?? "null";

        Console.WriteLine($"{i + 1,3} | {Short(inputDisp),-12} | {Short(tests[i].expected),-20} | {Short(actual),-20} | {(ok ? "PASS" : "FAIL")}"
       );
    }

    Console.WriteLine($"Summary: {passed}/{tests.Length} passed");

    // Exit codes: 0 = all pass; 1 = some fail; 2 = invocation/parsing error (handled above)
    Environment.ExitCode = (passed == tests.Length) ? 0 : 1;

    // Local helper: shorten long cells so the table stays aligned
    static string Short(string s) => s.Length <= 18 ? s : s.Substring(0, 15) + "...";
}


// ---------------------------- CLI ARG PARSER ------------------------------
// Parse command-line args into optional file path and compare flags.
// Contract:
//   - first arg that DOES NOT start with "--" is the file path (e.g., "tests.json")
//   - flags supported: --ignore-case, --normalize-whitespace, --trim-output
// Returns null if no flags were provided (no override)
static CompareOptions? ParseCliOptions(string[] args, out string? filePath)
{
    // Our tiny parser recognizes three boolean flags and one positional file path
    bool ignore = false, normWs = false, trimOut = false;
    filePath = null;

    foreach (var a in args)
    {
        if (a.StartsWith("--"))
        {
            // Normalize to lower-case, culture-invariant, and trim surrounding spaces
            switch (a.Trim().ToLowerInvariant())
            {
                case "--ignore-case":
                    ignore = true;
                    break;
                case "--normalize-whitespace":
                    normWs = true;
                    break;
                case "--trim-output":
                    trimOut = true;
                    break;
            }
        }
        else if (filePath is null)
        {
            // The first non-flag token is treated as file path
            filePath = a;
        }
    }

    // If at least one flag was provided, return an override object; else return null
    if (ignore || normWs || trimOut)
    {
        return new CompareOptions(ignore, normWs, trimOut);
    }

    return null; // no CLI options -> no override
}

//----------------------------------------------------------------------MAIN----------------------------------------------------------------//


// CLI usage examples (from VS Code terminal):
//   dotnet run -- tests.json
//   dotnet run -- tests.json --ignore-case --normalize-whitespace --trim-output
// If no args are provided, the app asks interactively.

var cliOpt = ParseCliOptions(args, out var pathFromCli);
if (!string.IsNullOrWhiteSpace(pathFromCli))
{
    Console.WriteLine($"[INFO] Running tests from: {pathFromCli}");
    if (cliOpt is not null)
    {
        Console.WriteLine($"[INFO] Compare opts: ignoreCase={cliOpt.ignoreCase}, normalizeWhitespace={cliOpt.normalizeWhitespace}, trimOutput={cliOpt.trimOutput}");
    }

    // IMPORTANT: run even if there are no flags; flags just override compare behavior
    RunTestsFromFile(pathFromCli, cliOpt);
    return;
}

// No CLI params -> ask the user whether to test or to greet
// Ask for test mode: if 'y' run tests
Console.Write("Run test mode? (y/n): ");
string? mode = Console.ReadLine();

if (mode?.Trim().ToLowerInvariant() == "y")
{
    // Default test file next to Program.cs
    RunTestsFromFile("tests.json"); // you can override compare via flags in CLI mod
    return;
}

// Normal greeting mode (no tests)
Console.Write("Enter your name: ");
string? name = Console.ReadLine();
Console.WriteLine(Greet(name));


// ------------------------------ DATA MODELS --------------------------------------------------//

public readonly record struct TestCase(string? input, string expected);

// Compare options (reference-type record)
public record CompareOptions(
    bool ignoreCase = false,
    bool normalizeWhitespace = false,
    bool trimOutput = false
);

// Extended JSON root object (only used when the file is in the "object" form)
public class TestFile
{
    // Nullable because the property might be missing in the JSON; we handle null with '??' when loading
    public TestCase[]? Tests { get; set; }
    
    // Nullable for the same reason; defaults are provided on load if null
    public CompareOptions? Compare { get; set; }
}