namespace TeamTools.TSQL.Linter.CommandLine.Config
{
    public enum OutputFileFormat
    {
        /// <summary>
        /// Как выхлоп в консоль, только в файл.
        /// </summary>
        PlainText,

        /// <summary>
        /// Кастомный JSON, из которого возможно получить JSLint или SonarQube отчет.
        /// </summary>
        JSON,

        /// <summary>
        /// Отчет, подготовленный для загрузки в SonarQube.
        /// </summary>
        SonarQubeReport,
    }
}
