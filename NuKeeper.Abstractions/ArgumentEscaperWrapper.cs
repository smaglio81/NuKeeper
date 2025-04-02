using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using McMaster.Extensions.CommandLineUtils;

namespace NuKeeper.Abstractions
{
    /// <summary>
    /// ArgumentEscaper.EscapeAndConcatenate has an issue where it adds an extra to end of file paths:
    ///
    /// Example:
    ///   C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\ -> "C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\\"
    ///
    /// The quotes are expected, but the extra backslash is not.
    /// 
    /// https://natemcmaster.github.io/CommandLineUtils/v2.5/api/McMaster.Extensions.CommandLineUtils.ArgumentEscaper.html
    ///
    /// This wrapper was built to fix this issue.
    /// </summary>
    public static class ArgumentEscaperWrapper
    {
        static readonly Regex DoubleBackslashQuote = new Regex("\\\\\"");

        /// <summary>
        /// Undo the processing which took place to create string[] args in Main, so that the next process will receive the same string[] args.
        /// 
        /// https://natemcmaster.github.io/CommandLineUtils/v2.5/api/McMaster.Extensions.CommandLineUtils.ArgumentEscaper.html
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <returns>A single string of escaped arguments</returns>
        public static string EscapeAndConcatenate(IEnumerable<string> args)
        {
            var result = ArgumentEscaper.EscapeAndConcatenate(args);
            result = DoubleBackslashQuote.Replace(result, "\"");        //  Changes \\" -> \"
            return result;
        }
    }
}
