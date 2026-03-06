namespace companyOSINT.Domain.Common;

/// <summary>
/// Statische Liste aller Software- und Tool-Namen, die vom DetectionEngine erkannt werden können.
/// Wird für Filter-Dropdowns verwendet, damit diese unabhängig vom DB-Inhalt befüllt sind.
/// </summary>
public static class KnownDetections
{
    public static readonly IReadOnlyList<string> SoftwareNames =
    [
        "Contao",
        "Drupal",
        "Joomla",
        "Magento",
        "nopCommerce",
        "OpenCart",
        "PrestaShop",
        "Shopware",
        "TYPO3",
        "WooCommerce",
        "WordPress",
    ];

    public static readonly IReadOnlyList<string> ToolNames =
    [
        "Cloudflare",
        "Facebook Pixel",
        "Google Analytics",
        "Google Fonts",
        "Matomo",
    ];
}
