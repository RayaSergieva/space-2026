namespace Space2026.Console;

/// <summary>
/// Small console input primitives shared by the menu: validated integer
/// prompts (with an Enter-for-default variant) and a masked password reader.
/// Kept separate so MissionConsole stays pure menu dispatch.
/// </summary>
internal static class ConsoleInput
{
    public static int ReadInt(int min, int max)
    {
        while (true)
        {
            if (int.TryParse(System.Console.ReadLine(), out var value) && value >= min && value <= max)
                return value;

            System.Console.Write($"Please enter a whole number between {min} and {max}: ");
        }
    }

    public static int ReadInt(int min, int max, int defaultValue)
    {
        while (true)
        {
            var input = (System.Console.ReadLine() ?? "").Trim();
            if (input.Length == 0)
                return defaultValue;

            if (int.TryParse(input, out var value) && value >= min && value <= max)
                return value;

            System.Console.Write($"Please enter a whole number between {min} and {max} (or Enter for {defaultValue}): ");
        }
    }

    public static string ReadPassword()
    {
        var password = new System.Text.StringBuilder();
        while (true)
        {
            var key = System.Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { System.Console.WriteLine(); break; }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0) password.Remove(password.Length - 1, 1);
                continue;
            }
            if (!char.IsControl(key.KeyChar)) password.Append(key.KeyChar);
        }
        return password.ToString();
    }
}