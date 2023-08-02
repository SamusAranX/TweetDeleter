# TweetDeleter

### How does it work?

**Update (08/2023):** Seems Twitter finally killed the delete endpoint for good. This app will no longer work unless someone figures out a way to abuse the official app API.

First, you'll need a set of API keys.

#### If you only have a consumer key/secret pair

Download the newest release and run it, specifying only the keys you have. A browser window will open, asking you to log into Twitter. Log in using the account you want to delete tweets from, then copy the PIN back into the tool window. If all goes well, all four required keys will be output and the tool will exit. Write the keys down somewhere and use them when restarting the tool.

#### If you have a full set of keys

Download the newest release, run it, and enter all the values it asks for. Hit Enter one final time and watch as all of your old tweets are deleted.

You can also put a bunch of tweet IDs into a text file, separated by newlines, and use this tool to delete *just* those tweets.

Note that by necessity, this tool sends a lot of API requests, so if there's a lot of tweets to delete, you may run into the request limit. If you do, just wait a bit and run it again.

### I want details!

Fine. You can also run this from the command line and specify your keys like this:

```
> .\TweetDeleter.exe -h
Description:
  A humble tweet deleter.

Usage:
  TweetDeleter [options]

Options:
  -c, --consumer-key <consumer-key>                Your API consumer key. Also called 'API key' by Twitter.
  -C, --consumer-secret <consumer-secret>          Your API consumer secret. Also called 'API secret key' by Twitter.
  -a, --access-token <access-token>                Your API access token. Omit to try PIN-based authentication.
  -A, --access-token-secret <access-token-secret>  Your API access token secret. Omit to try PIN-based authentication.
  -M, --max-tweet-age <max-tweet-age>              The age (in days) beyond which tweets are deleted. For instance, a
                                                   value of 365 would delete all tweets older than a year. Set to 0 to
                                                   delete all tweets.
  -t, --tweet-list <tweet-list>                    A file with tweet IDs separated by newlines.
  -T, --only-tweet-list                            Skip the normal deletion method and only process the tweet list.
  -k, --keep-media                                 Don't delete tweets containing media.
  -y                                               Specify this to skip all 'Are you sure?' questions.
  --version                                        Show version information
  -?, -h, --help                                   Show help and usage information
```

Optionally specify `-y` to make this program fully automatic.