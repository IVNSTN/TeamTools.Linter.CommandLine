using System;

namespace TeamTools.Common.Linting
{
    public interface ILinterRule
    {
        event EventHandler<RuleViolationEventDto> ViolationCallback;

        void Subscribe(ViolationCallbackEvent callback);

        void HandleLineError(int line, int pos, string details = default);

        void HandleFileError(string details = default);
    }
}
