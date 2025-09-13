# MiniCheckStep1

This project was inspired by **David J. Malan** and Harvard’s **CS50** course.  
As a computer science teacher, I decided to start building my own **mini check50-like system** that can automatically test and give feedback on student code.  
This repository is the first step in that journey.

## What’s here so far
- .NET 8 console application
- Basic user input (reads a name, prints a greeting)
- Git version control
- Published on GitHub

## How to run (VS Code terminal)
1. Open the built-in terminal in VS Code.
2. Run:
   dotnet run

```bash
dotnet run
```

### Normal mode
```bash
dotnet run
# Type: n
# Then enter a name (e.g., John)
Run test mode? (y/n): n
Enter your name: John
Hello, John!
```

### Test mode from file
This project reads test cases from `tests.json` (placed next to `Program.cs`):

```json
[
  { "input": "John",      "expected": "Hello, John!" },
  { "input": "   Bob   ", "expected": "Hello, Bob!"  },
  { "input": null,        "expected": "Hello, friend!" }
]
```

### Run tests by passing a file path (no prompt)
Use `--` to pass arguments to your app:

```bash
dotnet run -- tests.json
dotnet run -- tests_alt.json
```