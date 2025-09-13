using System;

namespace MiniCheck
{
    // --------------------------------------------------------------------
    // Turns optional user input into a properly formatted greeting.
    // --------------------------------------------------------------------
    public sealed class Greeter
    {
        public string Greet(string? input)
        {
            // 1: Trim spaces if input is not null; keep null otherwise
            string? trimmed; // string? because it may remain null
            if (input == null)
            {
                trimmed = null;
            }
            else
            {
                trimmed = input.Trim();
            }

            // 2: If trimmed is null/empty/whitespace → default to "friend"
            string safeName;
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                safeName = "friend";
            }
            else
            {
                safeName = trimmed;
            }

            // 3: Return the final greeting string
            return "Hello, " + safeName + "!";
        }
    }
}
