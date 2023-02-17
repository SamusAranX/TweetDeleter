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

		IAuthenticatedUser authenticatedUser;
		try {
			authenticatedUser = await appClient.Users.GetAuthenticatedUserAsync();
			Console.WriteLine($"Authenticated as {authenticatedUser.ScreenName}");
		}
		catch (TwitterAuthException e)
		{
			Console.WriteLine("Couldn't authenticate. Please check whether the API is still online and if so, make sure your API keys are correct.");
			Console.WriteLine(e);
			return;
		}

		Console.WriteLine("Retrieving all tweets…");
		var timelineTweets = new List<ITweet>();
		var timelineIterator = appClient.Timelines.GetUserTimelineIterator(authenticatedUser.ScreenName);

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
			Console.WriteLine();
			Console.WriteLine($"{tweet.CreatedAt.LocalDateTime}: {tweet.FullText}");

			try
			{
				if (tweet.IsRetweet)
				{
					await tweet.DestroyRetweetAsync();
					Console.WriteLine("🔁❌ Deleted Retweet.");
				}
				else
				{
					await tweet.DestroyAsync();
					Console.WriteLine("💬❌ Deleted Tweet.");
				}
			}
			catch (TwitterException e)
			{
				Console.WriteLine("An error occurred trying to delete this tweet.");
				Console.WriteLine(e);
				return;
			}
		}

		Console.WriteLine("Deleted everything there was to delete.");
	}
}
