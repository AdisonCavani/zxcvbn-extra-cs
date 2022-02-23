namespace Zxcvbn
{
    /// <summary>
    /// Represents the password strenght in bits.
    /// </summary>
    internal class Entropy
    {
        /// <summary>
        /// Gets or sets a calculated estimate of how many bits of entropy the password covers.
        /// </summary>
        public double Value { get; set; }
    }
}
