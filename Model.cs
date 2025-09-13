using System;

namespace MiniCheck
{
    // --------------------------------------------------------------------
    // Single test case 
    // --------------------------------------------------------------------
    public class TestCase
    {
        public string? Input { get; set; }     // nullable: JSON may contain null
        public string? Expected { get; set; }  // should be present and non-null in valid JSON
    }

    // --------------------------------------------------------------------
    // Comparison options for robust matching:
    // - ignoreCase: compare without case sensitivity
    // - normalizeWhitespace: collapse multiple whitespaces to one (and trim ends)
    // - trimOutput: trim leading/trailing spaces before compare
    // --------------------------------------------------------------------
    public class CompareOptions
    {
        public bool IgnoreCase { get; set; }
        public bool NormalizeWhitespace { get; set; }
        public bool TrimOutput { get; set; }

        public CompareOptions()
        {
            // Defaults (can be overridden by JSON or CLI flags)
            IgnoreCase = false;
            NormalizeWhitespace = false;
            TrimOutput = false;
        }
    }

    // --------------------------------------------------------------------
    // Extended JSON root object:
    //   { "compare": { ... }, "tests": [ ... ] }
    // All properties are nullable so we can detect "missing" and set defaults.
    // --------------------------------------------------------------------
    public class TestFile
    {
        public TestCase[]? Tests { get; set; }       // nullable: section may be missing
        public CompareOptions? Compare { get; set; } // nullable: section may be missing
    }

    // --------------------------------------------------------------------
    // Helper container for what the loader returns:
    // - Tests: loaded list of test cases (possibly empty)
    // - Options: compare settings to use (defaults if none in file)
    // --------------------------------------------------------------------
    public class LoadResult
    {
        public TestCase[]? Tests { get; set; }
        public CompareOptions? Options { get; set; }
    }
}
