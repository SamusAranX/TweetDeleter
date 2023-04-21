using System.CommandLine;
using System.CommandLine.Binding;
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

var consumerKeySecretOption = new Option<string?>(
	"--consumer-secret",
	"Your API consumer secret. Also called 'API secret key' by Twitter."
);
consumerKeySecretOption.AddAlias("-C");

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

var keepMediaOption = new Option<bool>(
	"--keep-media",
	"Don't delete tweets containing media."
);
keepMediaOption.AddAlias("-k");

var goAheadOption = new Option<bool>(
	"-y",
	"Specify this to skip all 'Are you sure?' questions."
);

var testTwitterAPIOption = new Option<bool>(
	"--twitter",
	"Test whether Twitter API access still works."
);

root.AddOption(consumerKeyOption);
root.AddOption(consumerKeySecretOption);
root.AddOption(accessTokenOption);
root.AddOption(accessTokenSecretOption);

root.AddOption(maxTweetAgeOption);
root.AddOption(tweetListFileOption);
root.AddOption(onlyTweetListOption);
root.AddOption(keepMediaOption);
root.AddOption(goAheadOption);
root.AddOption(testTwitterAPIOption);

root.SetHandler(async (twitterCredentials, programOptions) =>
	{
		var versionString = $"TweetDeleter v{Assembly.GetEntryAssembly()!.GetName().Version}";
		Console.WriteLine(versionString);
		Console.WriteLine(new string('-', versionString.Length));
		Console.WriteLine();

		twitterCredentials.ConsumerKey ??= InputStuff.InputString("Please enter your API consumer key:");
		twitterCredentials.ConsumerKeySecret ??= InputStuff.InputString("Please enter your API consumer key secret:");
		twitterCredentials.AccessToken ??= InputStuff.InputString("Please enter your API access token: (leave empty to attempt PIN authentication)", true);
		twitterCredentials.AccessTokenSecret ??= InputStuff.InputString("Please enter your API access token secret: (leave empty to attempt PIN authentication)", true);

		if (twitterCredentials.AccessToken == "" || twitterCredentials.AccessTokenSecret == "")
		{
			// this will throw in case of an error so we can just continue below
			var credentials = await Deleter.AuthenticateViaPIN(twitterCredentials.ConsumerKey, twitterCredentials.ConsumerKeySecret);

			Console.WriteLine("Authentication successful! Please write these keys down somewhere and restart this tool using them:");
			Console.WriteLine($"CONSUMER KEY (unchanged): {twitterCredentials.ConsumerKey}");
			Console.WriteLine($"CONSUMER KEY SECRET (unchanged): {twitterCredentials.ConsumerKeySecret}");
			Console.WriteLine($"ACCESS TOKEN (new): {credentials.AccessToken}");
			Console.WriteLine($"ACCESS TOKEN SECRET (new): {credentials.AccessTokenSecret}");

			Console.WriteLine("Press any key to exit.");
			Console.ReadKey(true);
			return;
		}

		var deleter = new Deleter(twitterCredentials);
		await deleter.Authenticate(); // will throw if login fails

		if (programOptions.TestTwitterAPI)
		{
			await deleter.TestAPIAccess();
			return;
		}

		var maxTweetAge = programOptions.MaxTweetAge;
		if (maxTweetAge is null or < 0)
			maxTweetAge = InputStuff.InputInt("Please enter the maximum age (in days) of tweets that should be kept. Enter 0 to delete all tweets.", 0);

		var keepMedia = programOptions.KeepMedia;
		if (!keepMedia)
		{
			var keepMediaStr = InputStuff.InputString("Enter 'y' if you want to keep tweets containing media around.");
			keepMedia = keepMediaStr.Equals("y", StringComparison.InvariantCultureIgnoreCase);
		}

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

		var tweetIDListFile = programOptions.TweetListFile;
		var onlyTweetList = programOptions.OnlyTweetList;
		var goAhead = programOptions.GoAhead;

		if (!onlyTweetList)
		{
			if (!goAhead)
			{
				Console.WriteLine("Does this look right to you? Press Enter to continue.");
				if (Console.ReadKey().Key != ConsoleKey.Enter)
					return;

				Console.WriteLine();
			}

			await deleter.DeleteTweets(deleteBeforeDate, keepMedia, goAhead);
		}

		if (tweetIDListFile != null)
			await deleter.DeleteTweetList(tweetIDListFile, deleteBeforeDate, keepMedia, goAhead);
	},
	new TwitterCredentialsBinder(consumerKeyOption, consumerKeySecretOption, accessTokenOption, accessTokenSecretOption), 
	new ProgramOptionsBinder(maxTweetAgeOption, tweetListFileOption, onlyTweetListOption, keepMediaOption, goAheadOption, testTwitterAPIOption));

return await root.InvokeAsync(args);
