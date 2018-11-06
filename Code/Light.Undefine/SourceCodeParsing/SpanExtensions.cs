using System;

namespace Light.Undefine.SourceCodeParsing
{
    /// <summary>
    /// Provides extension methods for <see cref="ReadOnlySpan{T}" />.
    /// </summary>
    public static class SpanExtensions
    {
        /// <summary>
        /// Tries to return the next line from the specified text, starting from the given index, stopping at the next
        /// <see cref="Environment.NewLine" /> characters (which are included) or at the end of the span.
        /// </summary>
        /// <param name="text">The multi-line text where the next line should be extracted from.</param>
        /// <param name="startIndex">The index where to start the next line.</param>
        /// <param name="nextLine">
        /// This value is set to a span starting from the start index and going to the next <see cref="Environment.NewLine" />
        /// characters (which are included), or to the end of the <paramref name="text" /> span.
        /// </param>
        /// <returns>True if a new line could be found, else false.</returns>
        public static bool TryGetNextLine(this in ReadOnlySpan<char> text, int startIndex, out ReadOnlySpan<char> nextLine)
        {
            if (startIndex >= text.Length)
            {
                nextLine = default;
                return false;
            }

            nextLine = text.Slice(startIndex);
            var newLineIndex = nextLine.IndexOf(Environment.NewLine.AsSpan());
            if (newLineIndex == -1)
                return true;

            nextLine = nextLine.Slice(0, newLineIndex + Environment.NewLine.Length);
            return true;
        }
    }
}