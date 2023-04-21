using System.CommandLine;
using System.CommandLine.Binding;

namespace TweetDeleter;

internal sealed class TwitterCredentials
{
	public string? ConsumerKey { get; set; }
	public string? ConsumerKeySecret { get; set; }
	public string? AccessToken { get; set; }
	public string? AccessTokenSecret { get; set; }
}

internal sealed class TwitterCredentialsBinder : BinderBase<TwitterCredentials>
{
	private readonly Option<string?> _accessToken;
	private readonly Option<string?> _accessTokenSecret;
	private readonly Option<string?> _consumerKey;
	private readonly Option<string?> _consumerKeySecret;

	public TwitterCredentialsBinder(Option<string?> consumerKey, Option<string?> consumerSecret, Option<string?> accessToken, Option<string?> accessTokenSecret)
	{
		this._consumerKey = consumerKey;
		this._consumerKeySecret = consumerSecret;
		this._accessToken = accessToken;
		this._accessTokenSecret = accessTokenSecret;
	}

	protected override TwitterCredentials GetBoundValue(BindingContext bindingContext)
	{
		return new()
		{
			ConsumerKey = bindingContext.ParseResult.GetValueForOption(this._consumerKey),
			ConsumerKeySecret = bindingContext.ParseResult.GetValueForOption(this._consumerKeySecret),
			AccessToken = bindingContext.ParseResult.GetValueForOption(this._accessToken),
			AccessTokenSecret = bindingContext.ParseResult.GetValueForOption(this._accessTokenSecret),
		};
	}
}

internal sealed class ProgramOptions
{
	public int? MaxTweetAge { get; set; }
	public string? TweetListFile { get; set; }
	public bool OnlyTweetList { get; set; }
	public bool KeepMedia { get; set; }
	public bool GoAhead { get; set; }
	public bool TestTwitterAPI { get; set; }
}

internal sealed class ProgramOptionsBinder : BinderBase<ProgramOptions>
{
	private readonly Option<bool> _goAhead;
	private readonly Option<bool> _keepMedia;
	private readonly Option<int?> _maxTweetAge;
	private readonly Option<bool> _onlyTweetList;
	private readonly Option<string?> _tweetListFile;
	private readonly Option<bool> _testTwitterAPI;

	public ProgramOptionsBinder(Option<int?> maxTweetAge, Option<string?> tweetListFile, Option<bool> onlyTweetList, Option<bool> keepMedia, Option<bool> goAhead, Option<bool> testTwitterAPI)
	{
		this._maxTweetAge = maxTweetAge;
		this._tweetListFile = tweetListFile;
		this._onlyTweetList = onlyTweetList;
		this._keepMedia = keepMedia;
		this._goAhead = goAhead;
		this._testTwitterAPI = testTwitterAPI;
	}

	protected override ProgramOptions GetBoundValue(BindingContext bindingContext)
	{
		return new()
		{
			MaxTweetAge = bindingContext.ParseResult.GetValueForOption(this._maxTweetAge),
			TweetListFile = bindingContext.ParseResult.GetValueForOption(this._tweetListFile),
			OnlyTweetList = bindingContext.ParseResult.GetValueForOption(this._onlyTweetList),
			KeepMedia = bindingContext.ParseResult.GetValueForOption(this._keepMedia),
			GoAhead = bindingContext.ParseResult.GetValueForOption(this._goAhead),
			TestTwitterAPI = bindingContext.ParseResult.GetValueForOption(this._testTwitterAPI),
		};
	}
}
