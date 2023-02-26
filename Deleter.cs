using System.Diagnostics;
using Tweetinvi;
using Tweetinvi.Core.Exceptions;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace TweetDeleter;

internal class Deleter
{
	private readonly TwitterClient _appClient;
	private bool _authenticated;
	private IAuthenticatedUser? _authenticatedUser;

	public Deleter(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret)
	{
		var userCredentials = new TwitterCredentials(consumerKey, consumerKeySecret, accessToken, accessTokenSecret);
		this._appClient = new TwitterClient(userCredentials);
	}

	public static async Task<ITwitterCredentials> AuthenticateViaPIN(string consumerKey, string consumerKeySecret)
	{
		Console.WriteLine("Requesting authentication URL…");
		var appClient = new TwitterClient(consumerKey, consumerKeySecret);

		IAuthenticationRequest authRequest;
		try
		{
			authRequest = await appClient.Auth.RequestAuthenticationUrlAsync();

			Console.WriteLine($"Opening Authentication URL: {authRequest.AuthorizationURL}");
			Process.Start(new ProcessStartInfo(authRequest.AuthorizationURL) { UseShellExecute = true });
		}
		catch (TwitterException e)
		{
			Console.WriteLine("An error occurred trying to retrieve an authentication URL for PIN-based OAuth.");
			throw;
		}

		var pin = InputStuff.InputString("Please enter the PIN and hit Enter:");

		ITwitterCredentials credentials;
		try
		{
			credentials = await appClient.Auth.RequestCredentialsFromVerifierCodeAsync(pin, authRequest);
		}
		catch (TwitterException e)
		{
			Console.WriteLine("An error occurred trying to retrieve user credentials.");
			throw;
		}

		return credentials;
	}

	public async Task Authenticate()
	{
		Console.WriteLine("Authenticating…");

		try
		{
			this._authenticatedUser = await this._appClient.Users.GetAuthenticatedUserAsync();
			Console.WriteLine($"Authenticated as {this._authenticatedUser.ScreenName}");
		}
		catch (TwitterAuthException e)
		{
			Console.WriteLine("Couldn't authenticate. Please check whether the API is still online and if so, make sure your API keys are correct.");
			Console.WriteLine(e);
			throw;
		}

		this._authenticated = true;
	}

	private static void ConfirmDeletion(bool goAhead)
	{
		if (goAhead)
			return;

		Console.WriteLine("----------");
		Console.WriteLine("PLEASE NOTE: This is your last chance to back out.");
		Console.WriteLine("TO START IRREVOCABLY DELETING TWEETS, press Enter.");
		Console.WriteLine("TO EXIT AND LEAVE YOUR TWEETS ALONE, press any other button or close this window.");
		Console.WriteLine("----------");

		if (Console.ReadKey().Key != ConsoleKey.Enter)
			return;

		Console.WriteLine();
	}

	/// <summary>
	/// A centralized function that handles exceptions thrown by API methods.
	/// </summary>
	/// <param name="e">The TwitterException that was thrown.</param>
	/// <param name="tweetID">
	/// The ID of the tweet this exception was thrown for. This is printed in case the error is a Not
	/// Found error.
	/// </param>
	/// <returns>A boolean that signifies whether to exit or not.</returns>
	private static bool HandleTwitterException(ITwitterException e, long tweetID)
	{
		switch (e.StatusCode)
		{
			case 404:
				Console.WriteLine($"{tweetID}: ❌ Not found");
				break;
			case 429:
				Console.WriteLine("⚠️ Too many requests! Please wait a bit before running this tool again.");
				return true;
			default:
				Console.WriteLine(e);
				break;
		}

		return false;
	}

	public async Task DeleteTweets(DateTime deleteBeforeDate, bool goAhead)
	{
		if (!this._authenticated)
			return;

		var tweetCount = this._authenticatedUser!.StatusesCount;

		var p = new GetUserTimelineParameters(this._authenticatedUser!.ScreenName)
		{
			IncludeRetweets = true,
			ExcludeReplies = false,
		};

		Console.WriteLine("Retrieving all tweets…");
		var timelineTweets = new List<ITweet>();
		var timelineIterator = this._appClient.Timelines.GetUserTimelineIterator(p);

		while (!timelineIterator.Completed)
		{
			var page = await timelineIterator.NextPageAsync();
			timelineTweets.AddRange(page);

			Console.WriteLine("Adding tweets to list…");
		}

		Console.WriteLine("All done.");

		timelineTweets = timelineTweets.FindAll(t => t.CreatedAt.LocalDateTime < deleteBeforeDate);

		if (timelineTweets.Count == 0)
		{
			Console.WriteLine("There are no tweets that are old enough to be deleted. If there are still older tweets on your profile, you might have to delete them manually. Twitter doesn't allow going that far back with the API.");
			return;
		}

		var earliestTweet = timelineTweets.Last();
		if (timelineTweets.Count == 1)
			Console.WriteLine("Found one deletable tweet:");
		else
			Console.WriteLine($"Found a total of {timelineTweets.Count} deletable tweets, with the earliest being this one:");

		Console.WriteLine($"{earliestTweet.CreatedAt.LocalDateTime}: {earliestTweet.FullText}");
		Console.WriteLine();
		Console.WriteLine("Your actual earliest tweet might be even older, but Twitter's API doesn't let you go that far back in one go. Try running this app again after waiting a bit if you want to delete more tweets.");
		Console.WriteLine();

		ConfirmDeletion(goAhead);

		if (timelineTweets.Count == 1)
			Console.WriteLine("Deleting one tweet…");
		else
			Console.WriteLine($"Deleting {timelineTweets.Count} tweets…");

		foreach (var tweet in timelineTweets)
		{
			Console.WriteLine();
			Console.WriteLine($"{tweet.CreatedAt.LocalDateTime}: {tweet.FullText}");

			try
			{
				if (tweet.IsRetweet)
				{
					await tweet.DestroyRetweetAsync();
					Console.WriteLine("🔁🚮 Deleted Retweet.");
				}
				else
				{
					await tweet.DestroyAsync();
					Console.WriteLine("💬🚮 Deleted Tweet.");
				}
			}
			catch (TwitterException e)
			{
				Console.WriteLine("An error occurred trying to delete this tweet:");
				if (HandleTwitterException(e, tweet.Id))
					return;
			}
		}

		Console.WriteLine("Deleted everything there was to delete.");
	}

	public async Task DeleteTweetList(string tweetsFile, DateTime deleteBeforeDate, bool goAhead)
	{
		if (!this._authenticated)
			return;

		var tweetIDs = new List<long>();
		try
		{
			var lines = await File.ReadAllLinesAsync(tweetsFile);
			tweetIDs.AddRange(lines.Select(long.Parse));
		}
		catch (IOException e)
		{
			Console.WriteLine("Can't read file with tweet IDs:");
			Console.WriteLine(e);
			return;
		}
		catch (FormatException e)
		{
			Console.WriteLine("Tweet ID file contains non-number characters:");
			Console.WriteLine(e);
			return;
		}

		Console.WriteLine($"File contained {tweetIDs.Count} tweet IDs.");
		Console.WriteLine("Loading tweets…");

		var tweetList = new List<ITweet>();
		var loadedTweetNum = 0;
		foreach (var tweetID in tweetIDs)
		{
			if (tweetList.Any(t => t.Id == tweetID))
				Console.WriteLine($"{tweetID}: ❌ Duplicate, skipping");

			try
			{
				var tweet = await this._appClient.Tweets.GetTweetAsync(tweetID);

				loadedTweetNum++;

				if (tweet.CreatedAt.LocalDateTime >= deleteBeforeDate)
				{
					Console.WriteLine($"{tweetID}: ⏳ Not old enough");
					continue;
				}

				tweetList.Add(tweet);
				Console.WriteLine($"{tweetID}: ✅ Loaded");
			}
			catch (TwitterException e)
			{
				if (HandleTwitterException(e, tweetID))
					return;
			}
		}

		if (tweetList.Count == 0)
		{
			Console.WriteLine("There are no tweets that are old enough to be deleted. If there are still older tweets on your profile you want to delete, please copy their IDs into the tweet ID text file.");
			return;
		}

		if (loadedTweetNum == tweetList.Count)
			Console.WriteLine($"Out of {tweetIDs.Count} listed tweets, {loadedTweetNum} could be loaded, all of which are eligible for deletion.");
		else
			Console.WriteLine($"Out of {tweetIDs.Count} listed tweets, {loadedTweetNum} could be loaded, {tweetList.Count} of which are eligible for deletion.");

		ConfirmDeletion(goAhead);

		if (tweetList.Count == 1)
			Console.WriteLine("Deleting tweet…");
		else if (tweetList.Count > 1)
			Console.WriteLine("Deleting tweets…");

		var deletedTweets = 0;
		foreach (var tweet in tweetList)
		{
			try
			{
				Console.WriteLine();
				Console.WriteLine($"{tweet.CreatedAt.LocalDateTime}: {tweet.FullText}");

				if (tweet.IsRetweet)
				{
					await tweet.DestroyRetweetAsync();
					Console.WriteLine("🔁🚮 Deleted Retweet.");
				}
				else
				{
					await tweet.DestroyAsync();
					Console.WriteLine("💬🚮 Deleted Tweet.");
				}

				deletedTweets++;
			}
			catch (TwitterException e)
			{
				Console.WriteLine("An error occurred trying to delete this tweet:");
				if (HandleTwitterException(e, tweet.Id))
					return;
			}
		}

		if (deletedTweets == tweetList.Count)
			Console.WriteLine("Deleted all eligible tweets.");
		else if (deletedTweets < tweetList.Count)
			Console.WriteLine($"Deleted {deletedTweets} out of {tweetList.Count} tweets.");
		else
			Console.WriteLine($"Hmm. We somehow deleted {deletedTweets} out of {tweetList.Count} tweets, which is more than were eligible for deletion. This shouldn't have happened.");
	}
}
