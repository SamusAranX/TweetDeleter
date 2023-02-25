# TweetDeleter

### How does it work?

First, you'll need a full set of API keys. Then, grab the newest release, run it, and enter all the values it asks for. Hit Enter one final time and watch as all of your old tweets are deleted.

You can also put a bunch of tweet IDs into a text file, separated by newlines, and use this tool to delete *just* those tweets.

### I want details!

Fine. You can also run this from the command line and specify your keys like this:

```
> .\TweetDeleter.exe -h
Description:
  A humble tweet deleter.

Usage:
  TweetDeleter [options]

Options:
  -c, --consumer-key <consumer-key>                Your API consumer key. Also called 'API key' by Twitter. []
  -C, --consumer-secret <consumer-secret>          Your API consumer secret. Also called 'API secret key' by Twitter. []
  -a, --access-token <access-token>                Your API access token. []
  -A, --access-token-secret <access-token-secret>  Your API access token secret. []
  --max-tweet-age <max-tweet-age>                  The age (in days) beyond which tweets are deleted. For instance, a
                                                   value of 365 would delete all tweets older than a year. Set to 0 to
                                                   delete all tweets. [default: -1]
  -t, --tweet-list <tweet-list>                    A file with tweet IDs separated by newlines. []
  -T, --only-tweet-list                            Skip the normal deletion method and only process the tweet list.
                                                   [default: False]
  -y                                               Specify this to skip all 'Are you sure?' questions.
  --version                                        Show version information
  -?, -h, --help                                   Show help and usage information
```

Optionally specify `-y` to make this program fully automatic.