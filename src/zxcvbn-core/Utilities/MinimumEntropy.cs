using System;
using System.Collections.Generic;
using System.Linq;
using Zxcvbn.Matcher.Matches;

namespace Zxcvbn;

/// <summary>
/// Estimates how long a password will take to crack under various conditions.
/// </summary>
internal static class MinimumEntropy
{
    /// <summary>
    /// Calculates the minimum password entropy.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <param name="matches">The matches.</param>
    /// <returns>A minimum password entropy.</returns>
    public static double EstimateMinimumEntropy(string password, IEnumerable<Match> matches)
    {
        var bruteforce_cardinality = PasswordScoring.PasswordCardinality(password);

        // Minimum entropy up to position k in the password
        var minimumEntropyToIndex = new double[password.Length];
        var bestMatchForIndex = new Match[password.Length];

        for (var k = 0; k < password.Length; k++)
        {
            // Start with bruteforce scenario added to previous sequence to beat
            minimumEntropyToIndex[k] = (k == 0 ? 0 : minimumEntropyToIndex[k - 1]) + Math.Log(bruteforce_cardinality, 2);

            // All matches that end at the current character, test to see if the entropy is less
            foreach (var match in matches.Where(m => m.j == k))
            {
                var candidate_entropy = (match.i <= 0 ? 0 : minimumEntropyToIndex[match.i - 1]) + match.Entropy;
                if (candidate_entropy < minimumEntropyToIndex[k])
                {
                    minimumEntropyToIndex[k] = candidate_entropy;
                    bestMatchForIndex[k] = match;
                }
            }
        }

        return password.Length == 0 ? 0 : minimumEntropyToIndex[password.Length - 1];
    }
}
