namespace Light.Undefine.SourceCodeParsing
{
    /// <summary>
    /// This enum contains the different types a line of code can represent.
    /// </summary>
    public enum LineOfCodeType
    {
        SourceCode,
        IfDirective,
        ElseIfDirective,
        ElseDirective,
        EndIfDirective
    }
}