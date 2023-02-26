using System.CommandLine;
using System.Reflection;
using System.Text;
using TweetDeleter;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;

var root = new RootCommand
{
	Description = "A humble tweet deleter.",
};

var consumerKeyOption = new Option<string?>(
	"--consumer-key",
	"Your API consumer key. Also called 'API key' by Twitter."
);
consumerKeyOption.AddAlias("-c");

var consumerSecretOption = new Option<string?>(
	"--consumer-secret",
	"Your API consumer secret. Also called 'API secret key' by Twitter."
);
consumerSecretOption.AddAlias("-C");

var accessTokenOption = new Option<string?>(
	"--access-token",
	"Your API access token. Omit to try PIN-based authentication."
);
accessTokenOption.AddAlias("-a");

var accessTokenSecretOption = new Option<string?>(
	"--access-token-secret",
	"Your API access token secret. Omit to try PIN-based authentication."
);
accessTokenSecretOption.AddAlias("-A");

var maxTweetAgeOption = new Option<int?>(
	"--max-tweet-age",
	"The age (in days) beyond which tweets are deleted. For instance, a value of 365 would delete all tweets older than a year. Set to 0 to delete all tweets."
);
maxTweetAgeOption.AddAlias("-M");

var tweetListFileOption = new Option<string?>(
	"--tweet-list",
	"A file with tweet IDs separated by newlines."
);
tweetListFileOption.AddAlias("-t");

var onlyTweetListOption = new Option<bool>(
	"--only-tweet-list",
	"Skip the normal deletion method and only process the tweet list."
);
onlyTweetListOption.AddAlias("-T");

var goAheadOption = new Option<bool>(
	"-y",
	"Specify this to skip all 'Are you sure?' questions."
);

root.AddOption(consumerKeyOption);
root.AddOption(consumerSecretOption);
root.AddOption(accessTokenOption);
root.AddOption(accessTokenSecretOption);
root.AddOption(maxTweetAgeOption);

root.AddOption(tweetListFileOption);
root.AddOption(onlyTweetListOption);

root.AddOption(goAheadOption);

root.SetHandler(async (consumerKey, consumerKeySecret, accessToken, accessTokenSecret, maxTweetAge, tweetIDListFile, onlyTweetList, goAhead) =>
	{
		var versionString = $"TweetDeleter v{Assembly.GetEntryAssembly()!.GetName().Version}";
		Console.WriteLine(versionString);
		Console.WriteLine(new string('-', versionString.Length));
		Console.WriteLine();

		consumerKey ??= InputStuff.InputString("Please enter your API consumer key:");
		consumerKeySecret ??= InputStuff.InputString("Please enter your API consumer key secret:");
		accessToken ??= InputStuff.InputString("Please enter your API access token: (leave empty to attempt PIN authentication)", true);
		accessTokenSecret ??= InputStuff.InputString("Please enter your API access token secret: (leave empty to attempt PIN authentication)", true);

		if (accessToken == "" || accessTokenSecret == "")
		{
			// this will throw in case of an error so we can just continue below
			var credentials = await Deleter.AuthenticateViaPIN(consumerKey, consumerKeySecret);

			Console.WriteLine("Authentication successful! Please write these keys down somewhere and restart this tool using them:");
			Console.WriteLine($"CONSUMER KEY (unchanged): {consumerKey}");
			Console.WriteLine($"CONSUMER KEY SECRET (unchanged): {consumerKeySecret}");
			Console.WriteLine($"ACCESS TOKEN (new): {credentials.AccessToken}");
			Console.WriteLine($"ACCESS TOKEN SECRET (new): {credentials.AccessTokenSecret}");

			Console.WriteLine("Press any key to exit.");
			Console.ReadKey(true);
			return;
		}

		var deleter = new Deleter(consumerKey, consumerKeySecret, accessToken, accessTokenSecret);
		await deleter.Authenticate(); // will throw if login fails

		if (maxTweetAge is null or < 0)
			maxTweetAge = InputStuff.InputInt("Please enter the maximum age (in days) of tweets that should be kept. Enter 0 to delete all tweets.", 0);

		DateTime deleteBeforeDate;
		if (maxTweetAge == 0)
		{
			deleteBeforeDate = DateTime.Now;
			Console.WriteLine("Selected Mode: Delete all tweets");
		}
		else
		{
			deleteBeforeDate = (DateTime.Now - TimeSpan.FromDays(Convert.ToDouble(maxTweetAge))).Date;
			Console.WriteLine($"Selected Mode: Delete all tweets made before {deleteBeforeDate.ToShortDateString()}");
		}

		Console.WriteLine();

		if (!onlyTweetList)
		{
			if (!goAhead)
			{
				Console.WriteLine("Does this look right to you? Press Enter to continue.");
				if (Console.ReadKey().Key != ConsoleKey.Enter)
					return;

				Console.WriteLine();
			}

			await deleter.DeleteTweets(deleteBeforeDate, goAhead);
		}

		if (tweetIDListFile != null)
			await deleter.DeleteTweetList(tweetIDListFile, deleteBeforeDate, goAhead);
	},
	consumerKeyOption, consumerSecretOption, accessTokenOption, accessTokenSecretOption, maxTweetAgeOption, tweetListFileOption, onlyTweetListOption, goAheadOption);

return await root.InvokeAsync(args);
