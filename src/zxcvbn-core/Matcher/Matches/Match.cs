﻿using System;

namespace Zxcvbn.Matcher.Matches
{
    /// <summary>
    /// A match identified by zxcvbn.
    /// </summary>
    public abstract class Match
    {
        /// <summary>
        /// Gets the entropy that this portion of the password covers using the current pattern matching technique.
        /// </summary>
        public double Entropy { get; internal set; }

        /// <summary>
        /// Gets the number of guesses associated with this match.
        /// </summary>
        public double Guesses { get; internal set; }

        /// <summary>
        /// Gets log10(number of guesses) associated with this match.
        /// </summary>
        public double GuessesLog10 => Math.Log10(Guesses);

        /// <summary>
        /// Gets the start index in the password string of the matched token.
        /// </summary>
        public int i { get; internal set; }

        /// <summary>
        /// Gets the end index in the password string of the matched token.
        /// </summary>
        public int j { get; internal set; }

        /// <summary>
        /// Gets the name of the pattern matcher used to generate this match.
        /// </summary>
        public abstract string Pattern { get; }

        /// <summary>
        /// Gets the portion of the password that was matched.
        /// </summary>
        public string Token { get; internal set; }
    }
}
