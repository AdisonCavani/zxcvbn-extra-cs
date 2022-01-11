using System;
using System.Collections.Generic;
using System.Linq;
using Zxcvbn.Matcher.Matches;
using Zxcvbn.Scoring;

namespace Zxcvbn
{
    /// <summary>
    /// Some useful shared functions used for evaluating passwords.
    /// </summary>
    internal static class PasswordScoring
    {
        private const int MinimumGuessesBeforeGrowingSequence = 10000;

        /// <summary>
        /// Caclulate binomial coefficient (i.e. nCk).
        /// </summary>
        /// <param name="n">n.</param>
        /// <param name="k">k.</param>
        /// <returns>Binomial coefficient.</returns>
        public static double Binomial(int n, int k)
        {
            if (k > n) return 0;
            if (k == 0) return 1;

            var r = 1.0;
            for (var d = 1; d <= k; ++d)
            {
                r *= n;
                r /= d;
                n -= 1;
            }

            return r;
        }

        /// <summary>
        /// Estimate the extra entropy in a token that comes from mixing upper and lowercase letters.
        /// This has been moved to a static function so that it can be used in multiple entropy calculations.
        /// </summary>
        /// <param name="word">The word to calculate uppercase entropy for.</param>
        /// <returns>An estimation of the entropy gained from casing in <paramref name="word"/>.</returns>
        public static double CalculateUppercaseEntropy(string word)
        {
            const string startUpper = "^[A-Z][^A-Z]+$";
            const string endUpper = "^[^A-Z]+[A-Z]$";
            const string allUpper = "^[^a-z]+$";
            const string allLower = "^[^A-Z]+$";

            if (System.Text.RegularExpressions.Regex.IsMatch(word, allLower)) return 0;

            // If the word is all uppercase add's only one bit of entropy, add only one bit for initial/end single cap only
            if (new[] { startUpper, endUpper, allUpper }.Any(re => System.Text.RegularExpressions.Regex.IsMatch(word, re))) return 1;

            var lowers = word.Count(c => c >= 'a' && c <= 'z');
            var uppers = word.Count(c => c >= 'A' && c <= 'Z');

            // Calculate numer of ways to capitalise (or inverse if there are fewer lowercase chars) and return lg for entropy
            return Math.Log(Enumerable.Range(0, Math.Min(uppers, lowers) + 1).Sum(i => Binomial(uppers + lowers, i)), 2);
        }

        /// <summary>
        /// Estimates the attempts required to guess the password.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <param name="password">The actual password.</param>
        /// <returns>The guesses estimate.</returns>
        public static double EstimateGuesses(Match match, string password)
        {
            if (match.Guesses != 0)
                return match.Guesses;

            var minGuesses = 1.0;
            if (match.Token.Length < password.Length)
            {
                minGuesses = match.Token.Length == 1 ? BruteForceGuessesCalculator.MinSubmatchGuessesSingleCharacter : BruteForceGuessesCalculator.MinSubmatchGuessesMultiCharacter;
            }

            var guesses = 0.0;

            switch (match.Pattern)
            {
                case "bruteforce":
                    guesses = BruteForceGuessesCalculator.CalculateGuesses(match as BruteForceMatch);
                    break;

                case "date":
                    guesses = DateGuessesCalculator.CalculateGuesses(match as DateMatch);
                    break;

                case "dictionary":
                    guesses = DictionaryGuessesCalculator.CalculateGuesses(match as DictionaryMatch);
                    break;

                case "regex":
                    guesses = RegexGuessesCalculator.CalculateGuesses(match as RegexMatch);
                    break;

                case "repeat":
                    guesses = RepeatGuessesCalculator.CalculateGuesses(match as RepeatMatch);
                    break;

                case "sequence":
                    guesses = SequenceGuessesCalculator.CalculateGuesses(match as SequenceMatch);
                    break;

                case "spatial":
                    guesses = SpatialGuessesCalculator.CalculateGuesses(match as SpatialMatch);
                    break;
            }

            match.Guesses = Math.Max(guesses, minGuesses);
            return match.Guesses;
        }

        /// <summary>
        /// Identifies the most guessable match in the sequence.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="matches">The matches.</param>
        /// <param name="excludeAdditive">if set to <c>true</c>, will exclude additive matches (for unit testing only).</param>
        /// <returns>A summary on the most testable match.</returns>
        public static MostGuessableMatchResult MostGuessableMatchSequence(string password, IEnumerable<Match> matches, bool excludeAdditive = false)
        {
            var matchesByJ = Enumerable.Range(0, password.Length).Select(i => new List<Match>()).ToList();
            foreach (var m in matches)
                matchesByJ[m.j].Add(m);

            var optimal = new OptimalValues(password.Length);

            for (var k = 0; k < password.Length; k++)
            {
                foreach (var m in matchesByJ[k])
                {
                    if (m.i > 0)
                    {
                        foreach (var l in optimal.M[m.i - 1].Keys)
                        {
                            Update(password, optimal, m, l + 1, excludeAdditive);
                        }
                    }
                    else
                    {
                        Update(password, optimal, m, 1, excludeAdditive);
                    }
                }

                BruteforceUpdate(password, optimal, k, excludeAdditive);
            }

            var optimalMatchSequence = Unwind(optimal, password.Length);
            var optimalL = optimalMatchSequence.Count;

            double guesses;

            if (password.Length == 0)
                guesses = 1;
            else
                guesses = optimal.G[password.Length - 1][optimalL];

            return new MostGuessableMatchResult
            {
                Guesses = guesses,
                Password = password,
                Sequence = optimalMatchSequence,
                Score = 0,
            };
        }

        /// <summary>
        /// Calculate the cardinality of the minimal character sets necessary to brute force the password (roughly)
        /// (e.g. lowercase = 26, numbers = 10, lowercase + numbers = 36).
        /// </summary>
        /// <param name="password">THe password to evaluate.</param>
        /// <returns>An estimation of the cardinality of charactes for this password.</returns>
        public static int PasswordCardinality(string password)
        {
            var cl = 0;

            if (password.Any(c => c >= 'a' && c <= 'z')) cl += 26; // Lowercase
            if (password.Any(c => c >= 'A' && c <= 'Z')) cl += 26; // Uppercase
            if (password.Any(c => c >= '0' && c <= '9')) cl += 10; // Numbers
            if (password.Any(c => c <= '/' || (c >= ':' && c <= '@') || (c >= '[' && c <= '`') || (c >= '{' && c <= 0x7F))) cl += 33; // Symbols
            if (password.Any(c => c > 0x7F)) cl += 100; // 'Unicode' (why 100?)

            return cl;
        }

        private static void BruteforceUpdate(string password, OptimalValues optimal, int k, bool excludeAdditive)
        {
            Update(password, optimal, MakeBruteforceMatch(password, 0, k), 1, excludeAdditive);

            for (var i = 1; i <= k; i++)
            {
                var m = MakeBruteforceMatch(password, i, k);
                var obj = optimal.M[i - 1];

                foreach (var l in obj.Keys)
                {
                    var lastM = obj[l];
                    if (lastM.Pattern == "bruteforce")
                        continue;
                    Update(password, optimal, m, l + 1, excludeAdditive);
                }
            }
        }

        private static double Factorial(double n)
        {
            if (n < 2)
                return 1;
            var f = 1.0;

            for (var i = 2; i <= n; i++)
                f *= i;

            return f;
        }

        private static BruteForceMatch MakeBruteforceMatch(string password, int i, int j)
        {
            return new BruteForceMatch
            {
                Token = password.Substring(i, j - i + 1),
                i = i,
                j = j,
            };
        }

        private static List<Match> Unwind(OptimalValues optimal, int n)
        {
            var optimalMatchSequence = new List<Match>();
            var k = n - 1;
            var l = -1;
            var g = double.PositiveInfinity;

            foreach (var candidateL in optimal.G[k].Keys)
            {
                var candidateG = optimal.G[k][candidateL];

                if (candidateG < g)
                {
                    l = candidateL;
                    g = candidateG;
                }
            }

            while (k >= 0)
            {
                var m = optimal.M[k][l];
                optimalMatchSequence.Insert(0, m);
                k = m.i - 1;
                l--;
            }

            return optimalMatchSequence;
        }

        private static void Update(string password, OptimalValues optimal, Match m, int l, bool excludeAdditive)
        {
            var k = m.j;
            var pi = EstimateGuesses(m, password);
            if (l > 1)
                pi *= optimal.Pi[m.i - 1][l - 1];

            var g = Factorial(l) * pi;
            if (!excludeAdditive)
                g += Math.Pow(MinimumGuessesBeforeGrowingSequence, l - 1);

            foreach (var competingL in optimal.G[k].Keys)
            {
                var competingG = optimal.G[k][competingL];
                if (competingL > l)
                    continue;
                if (competingG <= g)
                    return;
            }

            optimal.G[k][l] = g;
            optimal.M[k][l] = m;
            optimal.Pi[k][l] = pi;
        }
    }
}
