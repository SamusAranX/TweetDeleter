using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;

namespace TweetDeleter;

internal class Logic
{
	public static async Task DeleteTweets(string consumerKey, string consumerKeySecret, string accessToken, string accessTokenSecret, DateTime deleteBeforeDate, bool goAhead)
	{
		Console.WriteLine("Logging in…");
		var userCredentials = new TwitterCredentials(consumerKey, consumerKeySecret, accessToken, accessTokenSecret);
		var appClient = new TwitterClient(userCredentials);

		var authenticatedUser = await appClient.Users.GetAuthenticatedUserAsync();
		Console.WriteLine($"Authenticated as {authenticatedUser.ScreenName}");

		Console.WriteLine("Preparing to retrieve tweets…");
		var timelineTweets = new List<ITweet>();
		var timelineIterator = appClient.Timelines.GetUserTimelineIterator(authenticatedUser.ScreenName);

		Console.WriteLine("Retrieving tweets…");
		while (!timelineIterator.Completed)
		{
			var page = await timelineIterator.NextPageAsync();
			timelineTweets.AddRange(page);
			Console.WriteLine($"Retrieved {timelineTweets.Count} tweets…");
		}

		Console.WriteLine("All done.");

		if (timelineTweets.Count == 0)
		{
			Console.WriteLine("There are no tweets to delete.");
			return;
		}

		var earliestTweet = timelineTweets.Last();
		if (timelineTweets.Count == 1)
			Console.WriteLine("Retrieved one tweet:");
		else
			Console.WriteLine($"Retrieved a total of {timelineTweets.Count} tweets, with the earliest being this one:");

		Console.WriteLine($"{earliestTweet.CreatedAt.LocalDateTime}: {earliestTweet.FullText}");
		Console.WriteLine();
		Console.WriteLine("Your actual earliest tweet might be even older, but Twitter's API doesn't let you go that far back in one go. Try running this app again after waiting a bit if you want to delete more tweets.");
		Console.WriteLine();

		if (!goAhead)
		{
			Console.WriteLine("----------");
			Console.WriteLine("PLEASE NOTE: This is your last chance to back out.");
			Console.WriteLine("TO START IRREVOCABLY DELETING TWEETS, press Enter.");
			Console.WriteLine("TO EXIT AND LEAVE YOUR TWEETS ALONE, press any other button or close this window.");
			Console.WriteLine("----------");

			if (Console.ReadKey().Key != ConsoleKey.Enter)
				return;

			Console.WriteLine();
		}

		if (timelineTweets.Count == 1)
			Console.WriteLine("Deleting one tweet…");
		else
			Console.WriteLine($"Deleting {timelineTweets.Count} tweets…");

		foreach (var tweet in timelineTweets)
		{
			if (tweet.CreatedAt.LocalDateTime >= deleteBeforeDate)
				continue;

			Console.WriteLine();
			Console.WriteLine($"{tweet.CreatedAt.LocalDateTime}: {tweet.FullText}");

			try
			{
				await tweet.DestroyAsync();
				Console.WriteLine("❌ Deleted.");
			}
			catch (TwitterException ex)
			{
				Console.WriteLine("An error occurred trying to delete this tweet.");
				Console.WriteLine(ex);
			}
		}
	}
}
