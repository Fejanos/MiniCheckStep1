using System;
using System.Text.Json;                 // JSON (de)serialization
using System.Text.RegularExpressions;    // regex for whitespace normalization

namespace MiniCheck
{
    // --------------------------------------------------------------------
    // Responsible for reading and parsing JSON test files.
    // --------------------------------------------------------------------
    public static class TestLoader
    {
        public static LoadResult Load(string path)
        {
            // 1) File existence check (fail early with clear error)
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File not found: " + path);
            }

            // 2) Read the whole file content into a string
            string json = File.ReadAllText(path);

            // 3) JSON options: property names are case-insensitive ("Input" vs "input")
            var opts = new JsonSerializerOptions();
            opts.PropertyNameCaseInsensitive = true;

            // 4) Try the SIMPLE ARRAY form first
            try
            {
                TestCase[]? arr = JsonSerializer.Deserialize<TestCase[]>(json, opts);
                if (arr != null)
                {
                    LoadResult res = new LoadResult();
                    res.Tests = arr;                   // use the array we just parsed
                    res.Options = new CompareOptions();// default compare options
                    return res;
                }
            }
            catch
            {
                // Ignore here; if this form fails, we try the extended object below.
            }

            // 5) Try the EXTENDED OBJECT form
            TestFile? obj = JsonSerializer.Deserialize<TestFile>(json, opts);
            if (obj == null)
            {
                throw new Exception("Invalid JSON format (neither array nor {tests, compare}).");
            }

            // 6) If "tests" or "compare" are missing, supply safe defaults
            TestCase[] tests;
            if (obj.Tests == null)
            {
                tests = new TestCase[0];
            }
            else
            {
                tests = obj.Tests;
            }

            CompareOptions options;
            if (obj.Compare == null)
            {
                options = new CompareOptions();
            }
            else
            {
                options = obj.Compare;
            }

            // 7) Return as a single result object
            LoadResult result = new LoadResult();
            result.Tests = tests;
            result.Options = options;
            return result;
        }
    }

    // --------------------------------------------------------------------
    // Normalizes strings before comparison, without changing what we print.
    // --------------------------------------------------------------------
    public static class TextComparer
    {
        public static string Prepare(string s, CompareOptions opt)
        {
            string r = s;

            // Optionally trim leading/trailing spaces
            if (opt.TrimOutput)
            {
                if (r != null) r = r.Trim();
            }

            // Optionally collapse whitespace runs to a single space and trim ends
            if (opt.NormalizeWhitespace)
            {
                if (r == null)
                {
                    r = "";
                }
                r = Regex.Replace(r, @"\s+", " ").Trim();
            }

            // Optionally lower-case using invariant culture (robust, locale-neutral)
            if (opt.IgnoreCase)
            {
                if (r != null)
                {
                    r = r.ToLowerInvariant();
                }
            }

            return r;
        }
    }

    // --------------------------------------------------------------------
    // Runs tests and prints a compact PASS/FAIL table.
    // Returns the number of tests that passed.
    // --------------------------------------------------------------------
    public sealed class TestRunner
    {
        private readonly Greeter _greeter;            // the logic we are testing
        private readonly CompareOptions _options;     // how we compare strings

        public TestRunner(Greeter greeter, CompareOptions options)
        {
            _greeter = greeter;
            _options = options;
        }

        public int Run(TestCase[]? tests)
        {
            // Handle the edge case "no tests" explicitly
            if (tests == null || tests.Length == 0)
            {
                Console.WriteLine("[INFO] No tests found.");
                return 0;
            }

            int passed = 0;
            PrintHeader();

            for (int i = 0; i < tests.Length; i++)
            {
                // Extract input/expected from the current test row
                string? input = tests[i].Input;       // may be null
                string expected = tests[i].Expected; // expected should be non-null

                // Produce actual output by calling business logic
                string actual = _greeter.Greet(input);

                // Normalize both sides according to compare options, then compare
                string left = TextComparer.Prepare(actual, _options);
                string right = TextComparer.Prepare(expected, _options);
                bool ok = left == right;

                if (ok)
                {
                    passed++;
                }

                // Prepare display value for "Input" column (show "null" literally)
                string inputDisp;
                if (input == null)
                {
                    inputDisp = "null";
                }
                else
                {
                    inputDisp = input;
                }

                // Print one row in a fixed-width table
                    Console.WriteLine(string.Format(
                    "{0,3} | {1,-12} | {2,-20} | {3,-20} | {4}",
                    i + 1,
                    Short(inputDisp),
                    Short(expected),
                    Short(actual),
                    ok ? "PASS" : "FAIL"
                ));
            }

            Console.WriteLine("Summary: " + passed + "/" + tests.Length + " passed");
            return passed;
        }

        // Prints the table header once
        private static void PrintHeader()
        {
            Console.WriteLine("Idx | Input        | Expected             | Actual               | Result");
            Console.WriteLine("----+--------------+----------------------+----------------------+--------");
        }

        // Shortens long text so the table stays aligned
        private static string Short(string? s)
        {
            if (s == null)
            {
                return "null";
            }

            if (s.Length <= 18)
            {
                return s;
            }

            return s.Substring(0, 15) + "...";
        }
    }
}
