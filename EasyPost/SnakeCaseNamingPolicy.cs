/*
 * Licensed under The MIT License (MIT)
 * 
 * Copyright (c) 2014 EasyPost
 * Copyright (C) 2017 AMain.com, Inc.
 * All Rights Reserved
 */

using System.Text;
using System.Text.Json;

namespace EasyPost
{
    /// <summary>
    /// Naming policy for System.Text.Json to implement snake_case
    /// </summary>
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        private enum SeparatedCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }

        /// <summary>
        /// Converts the name for serialization or deserialization from the C# name  to the JSON name
        /// </summary>
        /// <param name="name">C# property name to convert</param>
        /// <returns>JSON name</returns>
        public override string ConvertName(string name) => ToSeparatedCase(name, '_');

        /// <summary>
        /// Convert a string to casing separated at the capital letters
        /// </summary>
        /// <param name="s">String to convert</param>
        /// <param name="separator">Separator to use</param>
        /// <returns>Converted string</returns>
        private static string ToSeparatedCase(
            string s,
            char separator)
        {
            if (string.IsNullOrEmpty(s)) {
                return s;
            }

            var sb = new StringBuilder();
            var state = SeparatedCaseState.Start;

            for (var i = 0; i < s.Length; i++) {
                if (s[i] == ' ') {
                    if (state != SeparatedCaseState.Start) {
                        state = SeparatedCaseState.NewWord;
                    }
                } else if (char.IsUpper(s[i])) {
                    switch (state) {
                        case SeparatedCaseState.Upper:
                            var hasNext = (i + 1 < s.Length);
                            if (i > 0 && hasNext) {
                                var nextChar = s[i + 1];
                                if (!char.IsUpper(nextChar) && nextChar != separator) {
                                    sb.Append(separator);
                                }
                            }

                            break;
                        case SeparatedCaseState.Lower:
                        case SeparatedCaseState.NewWord:
                            sb.Append(separator);
                            break;
                    }

                    var c = char.ToLowerInvariant(s[i]);
                    sb.Append(c);
                    state = SeparatedCaseState.Upper;
                } else if (s[i] == separator) {
                    sb.Append(separator);
                    state = SeparatedCaseState.Start;
                } else {
                    if (state == SeparatedCaseState.NewWord) {
                        sb.Append(separator);
                    }
                    sb.Append(s[i]);
                    state = SeparatedCaseState.Lower;
                }
            }

            return sb.ToString();
        }
    }
}