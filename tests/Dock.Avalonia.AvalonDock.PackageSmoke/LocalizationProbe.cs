using System;
using System.Collections;
using System.Globalization;
using System.Resources;
using Labels = AvalonDock.Properties.Resources;

namespace PackageConsumer;

internal static class LocalizationProbe
{
    internal static void Verify()
    {
        CultureInfo? previous = Labels.Culture;
        CultureInfo uiCulture = CultureInfo.CurrentUICulture;
        (string Culture, string Close)[] cultures =
        [
            ("cs-CZ", "Zavřít"), ("de", "Schließen"), ("es", "Cerrar"), ("fr", "Fermer"),
            ("hu", "Bezárás"), ("it", "Chiudi"), ("ja-JP", "閉じる"), ("nl-BE", "Sluiten"),
            ("pt-BR", "Fechar"), ("ro", "Închidere"), ("ru", "Закрыть"), ("sv", "Stäng"), ("zh-Hans", "关闭")
        ];
        try
        {
            foreach ((string culture, string close) in cultures)
            {
                Labels.Culture = CultureInfo.GetCultureInfo(culture);
                ResourceSet resources = Labels.ResourceManager.GetResourceSet(Labels.Culture, true, false)
                    ?? throw new Exception("The package is missing the " + culture + " satellite resources.");
                int count = 0;
                foreach (DictionaryEntry entry in resources)
                {
                    if (entry.Value is not string text || string.IsNullOrWhiteSpace(text))
                        throw new Exception("The packaged localized resource is not a nonempty string.");
                    count++;
                }
                if (count != 21 || Labels.Document_Close != close)
                    throw new Exception("The packaged satellite did not contain the complete " + culture + " resource set.");
            }
            Labels.Culture = CultureInfo.GetCultureInfo("fr-CA");
            if (Labels.Document_Close != "Fermer") throw new Exception("The packaged resources did not fall back to their parent culture.");
            Labels.Culture = CultureInfo.GetCultureInfo("pt-PT");
            if (Labels.Document_Close != "Close") throw new Exception("An unavailable locale did not use the packaged English fallback.");
            Labels.Culture = null;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-AT");
            if (Labels.Document_Close != "Schließen") throw new Exception("Null resource culture did not use the current UI culture.");
        }
        finally { Labels.Culture = previous; CultureInfo.CurrentUICulture = uiCulture; }
    }
}
