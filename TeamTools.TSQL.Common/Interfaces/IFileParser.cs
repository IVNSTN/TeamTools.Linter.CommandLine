namespace TeamTools.Common.Linting
{
    public interface IFileParser<TP>
    {
        TP Parse(ILintingContext context);
    }
}
