static string Greet(string? input)
{
    // Input check and trim
    string safeName = string.IsNullOrEmpty(input) ? "friend" : input.Trim();
    return $"Hello, {safeName}";
}


//----------------------------------------------------------------------MAIN----------------------------------------------------------------//
Console.WriteLine("Enter your name: ");
string? name = Console.ReadLine();
Console.WriteLine(Greet(name));