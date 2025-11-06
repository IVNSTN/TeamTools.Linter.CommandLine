namespace TeamTools.Common.Linting
{
    public enum Severity
    {
        /// <summary>
        /// Rule is disabled
        /// </summary>
        None = 0,

        /// <summary>
        /// Recommendation, hint
        /// </summary>
        Info = 1,

        /// <summary>
        /// Stopper but not error
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Violation is fatal
        /// </summary>
        Error = 3,
    }
}
