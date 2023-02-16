using System.CommandLine;
using System.Text;
using TweetDeleter;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;

var root = new RootCommand
{
	Description = "A humble tweet deleter.",
};

var consumerKeyOption = new Option<string>(
	"--consumer-key",
	"Your API consumer key. Also called 'API key' by Twitter."
);
consumerKeyOption.AddAlias("-c");

var consumerSecretOption = new Option<string>(
	"--consumer-secret",
	"Your API consumer secret. Also called 'API secret key' by Twitter."
);
consumerSecretOption.AddAlias("-C");

var accessTokenOption = new Option<string>(
	"--access-token",
	"Your API access token."
);
accessTokenOption.AddAlias("-a");

var accessTokenSecretOption = new Option<string>(
	"--access-token-secret",
	"Your API access token secret."
);
accessTokenSecretOption.AddAlias("-A");

var maxTweetAgeOption = new Option<int>(
	"--max-tweet-age",
	description: "The age (in days) beyond which tweets are deleted. For instance, a value of 365 would delete all tweets older than a year. Set to 0 to delete all tweets.",
	getDefaultValue: () => -1
);

var goAheadOption = new Option<bool>(
	"-y",
	"Specify this to skip all 'Are you sure?' questions."
);

root.AddOption(consumerKeyOption);
root.AddOption(consumerSecretOption);
root.AddOption(accessTokenOption);
root.AddOption(accessTokenSecretOption);
root.AddOption(maxTweetAgeOption);
root.AddOption(goAheadOption);

root.SetHandler(async (consumerKey, consumerKeySecret, accessToken, accessTokenSecret, maxTweetAge, goAhead) =>
	{
		if (maxTweetAge < 0)
			maxTweetAge = InputStuff.InputInt("Please enter the maximum age (in days) of tweets that should be kept. Enter 0 to delete all tweets.", 0);

		DateTime deleteBeforeDate;
		if (maxTweetAge == 0)
		{
			deleteBeforeDate = DateTime.Now;
			Console.WriteLine("Selected Mode: Delete all tweets");
		}
		else
		{
			deleteBeforeDate = (DateTime.Now - TimeSpan.FromDays(maxTweetAge)).Date;
			Console.WriteLine($"Selected Mode: Delete all tweets made before {deleteBeforeDate.ToShortDateString()}");
		}

		Console.WriteLine();

		if (!goAhead)
		{
			Console.WriteLine("Does this look right to you? Press Enter to continue.");
			if (Console.ReadKey().Key != ConsoleKey.Enter)
				return;

			Console.WriteLine();
		}

		await Logic.DeleteTweets(consumerKey, consumerKeySecret, accessToken, accessTokenSecret, deleteBeforeDate, goAhead);
	},
	consumerKeyOption, consumerSecretOption, accessTokenOption, accessTokenSecretOption, maxTweetAgeOption, goAheadOption);

return await root.InvokeAsync(args);
