using System;                     // basic runtime types (Console, Exception, etc.)

namespace MiniCheck
{
    // --------------------------------------------------------------------
    // CLI options container:
    // - FilePath: optional path to a JSON test file
    // - CompareOverride: optional compare settings passed from CLI flags
    // - HasFile(): helper to check if a file path was actually provided
    // --------------------------------------------------------------------
    public class CliOptions
    {
        public string? FilePath { get; set; }            // nullable: may not be provided
        public CompareOptions? CompareOverride { get; set; }  // nullable: flags may be absent

        public bool HasFile()
        {
            return !string.IsNullOrWhiteSpace(FilePath);
        }
    }

    // --------------------------------------------------------------------
    // Minimal CLI parser.
    // Rules:
    //   - The FIRST token that does NOT start with "--" is treated as file path.
    //   - Supported flags:
    //       --ignore-case
    //       --normalize-whitespace
    //       --trim-output
    //   - Accept a common typo like "--tests.json" and treat it as a file name.
    // --------------------------------------------------------------------
    public static class CliParser
    {
        public static CliOptions Parse(string[] args)
        {
            bool ignore = false;
            bool norm = false;
            bool trim = false;
            string? file = null;

            // Iterate over all arguments passed after the `--` separator.
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];

                // Flags start with "--"
                if (a.StartsWith("--"))
                {
                    // Normalize to lower-case, culture-invariant, and trim surrounding spaces
                    switch (a.Trim().ToLowerInvariant())
                    {
                        case "--ignore-case":
                            ignore = true;
                            break;
                        case "--normalize-whitespace":
                            norm = true;
                            break;
                        case "--trim-output":
                            trim = true;
                            break;
                    }
                }
                else
                {
                    // First non-flag token becomes the file path
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        file = a;
                    }
                }
            }

            // Build a CompareOptions override only if at least one flag was set
            CompareOptions? compare = null; // nullable: only created if any flag present
            if (ignore || norm || trim)
            {
                compare = new CompareOptions();
                compare.IgnoreCase = ignore;
                compare.NormalizeWhitespace = norm;
                compare.TrimOutput = trim;
            }

            // Return parsed options
            CliOptions opts = new CliOptions();
            opts.FilePath = file;
            opts.CompareOverride = compare;
            return opts;
        }
    }

    // --------------------------------------------------------------------
    // Application entry point.
    // --------------------------------------------------------------------
    public static class Program
    {
        public static int Main(string[] args)
        {
            Greeter greeter = new Greeter();       // business logic (what we test)
            CliOptions cli = CliParser.Parse(args);// parse CLI args into a structured object

            // 1) Non-interactive mode (a file path was passed on CLI)
            if (cli.HasFile())
            {
                string? pathMaybe = cli.FilePath;
                if (string.IsNullOrWhiteSpace(pathMaybe))
                {
                    Console.WriteLine("[ERROR] Empty file path.");
                    return 2;
                }
                string path = pathMaybe; // now guaranteed non-null/non-empty

                try
                {
                    // Load tests + compare options from the given file
                    LoadResult lr = TestLoader.Load(path);

                    // Decide which compare options to use:
                    // - CLI override wins if present
                    // - otherwise options from the file
                    CompareOptions options;
                    if (cli.CompareOverride != null)
                    {
                        options = cli.CompareOverride;
                        Console.WriteLine("[INFO] Compare opts: ignoreCase=" + options.IgnoreCase
                                          + ", normalizeWhitespace=" + options.NormalizeWhitespace
                                          + ", trimOutput=" + options.TrimOutput);
                    }
                    else
                    {
                        options = lr.Options;
                    }

                    Console.WriteLine("[INFO] Running tests from: " + path);

                    // Run tests and compute how many passed
                    TestRunner runner = new TestRunner(greeter, options);
                    int passed = runner.Run(lr.Tests);

                    // Return appropriate exit code
                    if (lr.Tests != null && lr.Tests.Length > 0 && passed == lr.Tests.Length)
                    {
                        return 0;
                    }
                    return 1;
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    Console.WriteLine("[ERROR] " + ex.Message);
                    return 2;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] Could not parse '" + path + "': " + ex.Message);
                    return 2;
                }
            }

            // 2) Interactive mode (no CLI file given): ask user if we should run tests
            Console.Write("Run test mode? (y/n): ");
            string? mode = Console.ReadLine(); // string? -> the user may press Enter without input
            if (mode != null)
            {
                mode = mode.Trim().ToLowerInvariant();
            }

            if (mode == "y")
            {
                try
                {
                    // Default test file next to Program.cs
                    LoadResult lr = TestLoader.Load("tests.json");
                    TestRunner runner = new TestRunner(greeter, lr.Options);
                    int passed = runner.Run(lr.Tests);

                    if (lr.Tests != null && lr.Tests.Length > 0 && passed == lr.Tests.Length)
                    {
                        return 0;
                    }
                    return 1;
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    Console.WriteLine("[ERROR] " + ex.Message);
                    return 2;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ERROR] Could not parse 'tests.json': " + ex.Message);
                    return 2;
                }
            }

            // 3) Normal greeting flow
            Console.Write("Enter your name: ");
            string? name = Console.ReadLine();
            Console.WriteLine(greeter.Greet(name));
            return 0;
        }
    }
}