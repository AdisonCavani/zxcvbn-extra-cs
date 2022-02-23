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
 
<table>
    <thead>
        <tr>
            <th>Zxcvbn-CS</th>
            <th>Zxcvbn-JS</th>
            <th>KeePass</th>
            <th>KeePassXC</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td colspan=4><i>t</i></td>
        </tr>
        <tr>
            <td>4.7</td>
            <td>4.7</td>
            <td>5</td>
            <td>3.32</td>
        </tr>
        <tr>
            <td colspan=4><i>t4</i></td>
        </tr>
        <tr>
            <td>3.7</td>
            <td>8.492</td>
            <td>11</td>
            <td>4.7</td>
        </tr>
        <tr>
            <td colspan=4><i>t3XKczXFIOrqHRr_</i></td>
        </tr>
        <tr>
            <td>85.008</td>
            <td>90.578</td>
            <td>92</td>
            <td>91.96</td>
        </tr>
        <tr>
            <td colspan=4><i>t3XKczXFIOrqHRr_t3XKczXFIOrqHRr</i></td>
        </tr>
        <tr>
            <td>163.447</td>
            <td>174.587</td>
            <td>102</td>
            <td>179.85</td>
        </tr>
        <tr>
            <td colspan=4><i>+wq)tIw6gb4]Uh@"-E(=</i></td>
        </tr>
        <tr>
            <td>141.289</td>
            <td>115.203</td>
            <td>127</td>
            <td>120.62</td>
        </tr>
        <tr>
            <td colspan=4><i>zK_f7M\(#"W-?4AyN6g}</i></td>
        </tr>
        <tr>
            <td>131.397</td>
            <td>124.901</td>
            <td>131</td>
            <td>128.7</td>
        </tr>
        <tr>
            <td colspan=4><i>89673460696657893304</i></td>
        </tr>
        <tr>
            <td>12.07</td>
            <td>61.146</td>
            <td>60</td>
            <td>63.56</td>
        </tr>
        <tr>
            <td colspan=4><i>acegikmoqsuwy</i></td>
        </tr>
        <tr>
            <td>0</td>
            <td>55.027</td>
            <td>9</td>
            <td>4.7</td>
        </tr>
        <tr>
            <td colspan=4><i>Abracadabra</i></td>
        </tr>
        <tr>
            <td>13.519</td>
            <td>15.013</td>
            <td>8</td>
            <td>13.51</td>
        </tr>
        <tr>
            <td colspan=4><i>abraCadaBra</i></td>
        </tr>
        <tr>
            <td>18.585</td>
            <td>20.029</td>
            <td>10</td>
            <td>18.57</td>
        </tr>
        <tr>
            <td colspan=4><i>ab®a©@daBra</i></td>
        </tr>
        <tr>
            <td>53.436</td>
            <td>49.83</td>
            <td>19</td>
            <td>56</td>
        </tr>
        <tr>
            <td colspan=4><i>.Abracadabram!67</i></td>
        </tr>
        <tr>
            <td>33.229</td>
            <td>43.293</td>
            <td>41</td>
            <td>40.12</td>
        </tr>
        <tr>
            <td colspan=4><i>77starTrek-sta®w@rs!$</i></td>
        </tr>
        <tr>
            <td>47.098</td>
            <td>76.686</td>
            <td>63</td>
            <td>66.87</td>
        </tr>
        <tr>
         <td colspan=4><i>hyevwfzfgyrlyafozwatdhujxlyltfdr</i></td>
        </tr>
        <tr>
            <td>79.907</td>
            <td>134.373</td>
            <td>135</td>
            <td>138.67</td>
        </tr>
    </tbody>
</table>
