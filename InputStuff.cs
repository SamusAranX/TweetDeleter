namespace TweetDeleter;

public static class InputStuff
{
	public static string InputString(string prompt = "", bool allowEmpty = false)
	{
		if (prompt.Trim() != "")
			Console.WriteLine(prompt);

		bool valid;
		string? inputStr;
		do
		{
			inputStr = Console.ReadLine();
			valid = (inputStr != null && inputStr.Trim().Length > 0) || allowEmpty;

			if (!valid)
				Console.WriteLine("Please enter a valid string.");
		}
		while (!valid);

		return inputStr!;
	}

	public static int InputInt(string prompt = "", int min = int.MinValue, int max = int.MaxValue)
	{
		if (prompt.Trim() != "")
		{
			if (min != int.MinValue && max != int.MaxValue)
				prompt += $" ({min}-{max})";

			Console.WriteLine(prompt);
		}

		bool parsed;
		int inputInt;
		do
		{
			var input = Console.ReadLine();
			parsed = int.TryParse(input, out inputInt);
			if (!parsed)
				Console.WriteLine("That was not an integer.");
			else if (inputInt < min || inputInt > max)
				Console.WriteLine("The entered value was outside of the expected range.");
		}
		while (!parsed || inputInt < min || inputInt > max);

		return inputInt;
	}
}
