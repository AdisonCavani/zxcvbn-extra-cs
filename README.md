Zxcvbn C#/.NET
==============

[![NuGet](https://img.shields.io/nuget/v/zxcvbn-extra)](https://www.nuget.org/packages/zxcvbn-extra)
[![License](https://img.shields.io/github/license/AdisonCavani/zxcvbn-extra-cs)](https://github.com/AdisonCavani/zxcvbn-extra-cs/blob/master/LICENSE)
[![.NET](https://github.com/AdisonCavani/zxcvbn-extra-cs/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/AdisonCavani/zxcvbn-extra-cs/actions/workflows/dotnet.yml)


This is a fork of [Zxcvbn-cs](https://github.com/trichards57/zxcvbn-cs) - a port of the [Zxcvbn](https://github.com/dropbox/zxcvbn) JavaScript password strength estimation library to .NET, written in C#.

From the `Zxcvbn` readme:

> `zxcvbn` is a password strength estimator inspired by password crackers. Through pattern matching and conservative estimation, it recognizes and weighs 30k common passwords, > common names and surnames according to US census data, popular English words from Wikipedia and US television and movies, and other common patterns like dates, repeats (`aaa`), sequences (`abcd`), keyboard patterns (`qwertyuiop`), and l33t speak.

> Consider using zxcvbn as an algorithmic alternative to password composition policy — it is more secure, flexible, and usable when sites require a minimal complexity score in place of annoying rules like "passwords must contain three of {lower, upper, numbers, symbols}".

> * __More secure__: policies often fail both ways, allowing weak passwords (`P@ssword1`) and disallowing strong passwords.
> * __More flexible__: zxcvbn allows many password styles to flourish so long as it detects sufficient complexity — passphrases are rated highly given enough uncommon words, keyboard patterns are ranked based on length and number of turns, and capitalization adds more complexity when it's unpredictaBle.
> * __More usable__: zxcvbn is designed to power simple, rule-free interfaces that give instant feedback. In addition to strength estimation, zxcvbn includes minimal, targeted verbal feedback that can help guide users towards less guessable passwords.

> For further detail and motivation, please refer to the USENIX Security '16 [paper and presentation](https://www.usenix.org/conference/usenixsecurity16/technical-sessions/presentation/wheeler).

This fork aims to restore old feature - calculating password `entropy`. This feature was removed in [Zxcvbn 4.0.1 release](https://github.com/dropbox/zxcvbn/releases/tag/4.0.1). In my opinion, `entropy` is more useful and helpful to end-user than `Guesses` or `GuessesLog10`. You can build your own scoring system based on entropy. This can be helpful to create a password-strenght color bar.

## Using `Zxcvbn-cs`

The included Visual Studio project will create a single assembly, Zxcvbn.dll, which is all that is
required to be included in your project.

To evaluate a password:

``` C#
using Zxcvbn;

var result = Zxcvbn.Core.EvaluatePassword("p@ssw0rd");
```

`EvaluatePassword` takes an optional second parameter that contains an enumerable of
user data strings to also match the password against.

## Interpreting Results

The `Result` structure returned from password evaluation is interpreted the same way as with JS `Zxcvbn`.

- `result.CalcTime` - how long it took zxcvbn to calculate an answer, in milliseconds
- `result.CrackTime` - dictionary of back-of-the-envelope crack time estimations, in seconds, based on a few scenarios:
     * OfflineFastHashing1e10PerSecond - offline attack with user-unique salting but a fast hash function like SHA-1, SHA-256 or MD5. A wide range of reasonable numbers anywhere from one billion - one trillion guesses per second, depending on number of cores and machines. Ballparking at 10B/sec
     * OfflineSlowHashing1e4PerSecond - offline attack. assumes multiple attackers, proper user-unique salting, and a slow hash function w/ moderate work factor, such as bcrypt, scrypt, PBKDF2
     * OnlineNoThrottling10PerSecond - online attack on a service that doesn't ratelimit, or where an attacker has outsmarted ratelimiting
     * OnlineThrottling100PerHour - online attack on a service that ratelimits password auth attempts
- `result.CrackTimeDisplay` - same keys as result.CrackTime, with friendlier display string values: 'less than a second', '3 hours', 'centuries', etc.
- `result.Entropy` - password entropy in bits
- `result.Feedback` - the password that was used to generate these results
     * Warning - explains what's wrong, eg. 'this is a top-10 common password'. Sometimes an empty string
     * Suggestions - a possibly-empty IList<string> of suggestions to help choose a less guessable password, eg. 'Add another word or two'
- `result.Guesses` - estimated guesses needed to crack password
- `result.GuessesLog10` - order of magnitude of result.Guesses
- `result.MatchSequence` - the IEnumerable<Match> list of patterns that zxcvbn based the guess calculation on
- `result.Password` - the password that was used to generate these results
 

| result.Score | Description  | Guesses |
| :----------: | :----------- | :------:|
| 0 | Too guessable: risky password  | < 10^3 |
| 1 | Very guessable: protection from throttled online attacks | < 10^6 |
| 2 | Somewhat guessable: protection from unthrottled online attacks | < 10^8 |
| 3 | Safely unguessable: moderate protection from offline slow-hash scenario | < 10^10 |
| 4 | Very unguessable: strong protection from offline slow-hash scenario | >= 10^10 |

## Zxcvbn-CS vs Zxcvbn-JS vs KeePass vs KeePassXC
This table shows entropy results from different programs

| Password                         | Zxcvbn-JS | KeePass | KeePassXC | Zxcvbn-CS |
|----------------------------------|:---------:|:-------:|:---------:|-----------|
| t                                | 4.7       | 4.7     | 5         | 3.32      |
| t4                               | 3.7       | 8.492   | 11        | 4.7       |
| t3XKczXFIOrqHRr_                 | 85.008    | 90.578  | 92        | 91.96     |
| t3XKczXFIOrqHRr_t3XKczXFIOrqHRr  | 163.447   | 174.587 | 102       | 179.85    |
| +wq)tIw6gb4]Uh@"-E(=             | 141.289   | 115.203 | 127       | 120.62    |
| zK_f7M\(#"W-?4AyN6g}             | 131.397   | 124.901 | 131       | 128.7     |
| 89673460696657893304             | 12.07     | 61.146  | 60        | 63.56     |
| acegikmoqsuwy                    | 0         | 55.027  | 9         | 4.7       |
| Abracadabra                      | 13.519    | 15.013  | 8         | 13.51     |
| abraCadaBra                      | 18.585    | 20.029  | 10        | 18.57     |
| ab®a©@daBra                      | 53.436    | 49.83   | 19        | 56        |
| .Abracadabram!67                 | 33.229    | 43.293  | 41        | 40.12     |
| 77starTrek-sta®w@rs!$            | 47.098    | 76.686  | 63        | 66.87     |
| hyevwfzfgyrlyafozwatdhujxlyltfdr | 79.907    | 134.373 | 135       | 138.67    |
