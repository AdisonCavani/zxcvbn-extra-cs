using System;

namespace Zxcvbn
{
    /// <summary>
    /// Estimates password entropy.
    /// </summary>
    internal static class EntropyEstimates
    {
        /// <summary>
        /// Calculates the password entropy
        /// </summary>
        /// <param name="minEntropy">The entropy.</param>
        /// <returns>A password entropy object.</returns>
        public static Entropy EstimateEntropy(double minEntropy)
        {
            return new Entropy
            {
                Value = Math.Round(minEntropy, 3);,
            };
        }
    }
}
