# MiniCheckStep1

Inspired by **David J. Malan** and Harvard’s **CS50**.  
As a computer science teacher, I’m building a mini **check50-like** tool to automatically test and give feedback on student code.  
This repository is the very first step in that journey.

---

## What’s here so far
- .NET 8 console application
- Testable greeting logic: `Greet(string?)`
- **File-based tests** via `tests.json`
- **CLI argument** to run tests without prompts
- Optional **compare options** (case/whitespace/trim) from JSON or CLI flags
- Compact **PASS/FAIL** table + **exit codes**

---

## How to run (VS Code terminal)

Open the built-in terminal in VS Code (shortcut: `` Ctrl+` ``).

### Normal (interactive) mode
```bash
dotnet run
# Type: n  → normal greeting
# Example:
# Run test mode? (y/n): n
# Enter your name: John
# Hello, John!
```

### Test mode (prompted)
```bash
dotnet run
# Type: y  → runs tests from tests.json (default path)
```

### Run tests directly from a file (no prompt)
Use a space after `--` to pass arguments to your app.
```bash
dotnet run -- tests.json
dotnet run -- tests_alt.json
```

### Override compare options via CLI flags
```bash
dotnet run -- tests.json --ignore-case --normalize-whitespace --trim-output
```
---

### JSON formats
Simple array (supported)
```bash
[
  { "input": "John",      "expected": "Hello, John!" },
  { "input": "   Bob   ", "expected": "Hello, Bob!"  },
  { "input": null,        "expected": "Hello, friend!" }
]
```
Extended object (also supported)
```bash
{
  "compare": {
    "ignoreCase": true,
    "normalizeWhitespace": true,
    "trimOutput": true
  },
  "tests": [
    { "input": "john",    "expected": "hello, john!" },
    { "input": "  eve  ", "expected": "HELLO, EVE!"  },
    { "input": "   ",     "expected": "hello, friend!" }
  ]
}
```

---

### Exit codes
- `0` : All tests passed
- `1` : At least one test failed (or no tests found)
- `2` : Invocation/parsing error (e.g., file not found, invalid JSON)

---

### Common pitfalls
- ✅ `dotnet run -- tests.json`
- ❌ `dotnet run --tests.json` (no space → treated like a flag, not a file)
- Place `tests.json` next to `Program.cs` (or pass a correct path)

### Roadmap
- ✅ Testable `Greet(string?)`
- ✅ Test mode (3 PASS/FAIL cases)
- ✅ File-based tests (`tests.json`)
- ✅ CLI args + compare options + exit codes
- ✅ Split into multiple files/classes (clean OOP structure)
- ❌ Add unit tests (`xUnit`) for Greeter and CLI parsing
- ❌ Support testing other functions (e.g., `Sum`, `IsPrime`) via config
- ❌ HTML/CSV report export