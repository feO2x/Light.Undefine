// ReSharper disable CommentTypo
namespace Light.Undefine.SourceCodeParsing
{
    /// <summary>
    /// This enum contains the different types a line of code can represent.
    /// </summary>
    public enum LineOfCodeType
    {
        /// <summary>
        /// Indicates that a line of code should be treated as source code (i.e. it contains no preprocessor directive).
        /// </summary>
        SourceCode,

        /// <summary>
        /// Indicates that a line of code contains a #if directive.
        /// </summary>
        IfDirective,

        /// <summary>
        /// Indicates that a line of code contains a #elif directive.
        /// </summary>
        ElseIfDirective,

        /// <summary>
        /// Indicates that a line of code contains a #else directive.
        /// </summary>
        ElseDirective,

        /// <summary>
        /// Indicates that a line of code contains a #endif directive.
        /// </summary>
        EndIfDirective
    }
}