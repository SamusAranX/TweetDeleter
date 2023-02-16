# TweetDeleter

### How does it work?

First, you'll need a full set of API keys. Then, grab the newest release, run it, and enter all the values it asks for. Hit Enter one final time and watch as all of your old tweets are deleted.

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
  --max-tweet-age <max-tweet-age>                  The age (in days) beyond which tweets are deleted. For instance, a value of 365 would delete all tweets older than a year. Set to 0 to delete all
                                                   tweets. [default: -1]
  -y                                               Specify this to skip all 'Are you sure?' questions.
  --version                                        Show version information
  -?, -h, --help                                   Show help and usage information
```

Optionally specify `-y` to make this program fully automatic.