using System.Collections;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Shared.Localization;

namespace TeacherGroupsManager.Services.Tests;

internal sealed class TestLocalizer : IStringLocalizer<SharedResource>
{
    public static readonly TestLocalizer Instance = new();

    public LocalizedString this[string name] => new(name, name);
    public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
    public IStringLocalizer WithCulture(System.Globalization.CultureInfo culture) => this;
}


