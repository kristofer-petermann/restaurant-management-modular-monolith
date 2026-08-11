using Base.Domain;

namespace WebApp.Helpers;

public static class SummaryTranslationUpdater
{
    public static LangStr Apply(string? value, LangStr? currentSummary)
    {
        // Empty input clears the current culture translation by writing an empty value.
        var valueToSet = string.IsNullOrWhiteSpace(value) ? string.Empty : value;

        currentSummary ??= new LangStr(valueToSet);
        currentSummary.SetTranslation(valueToSet);

        return currentSummary;
    }
}

