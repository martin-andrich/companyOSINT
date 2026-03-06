namespace companyOSINT.Worker.Detection;

public static class Descriptors
{
    // ── Software ────────────────────────────────────────────────────────

    public static readonly DetectorDescriptor WordPress = new(
        "WordPress", DetectorKind.Software,
        [
            new(RuleType.MetaGenerator, "WordPress"),
            new(RuleType.HtmlContains, "/wp-content/"),
            new(RuleType.HtmlContains, "/wp-includes/"),
            new(RuleType.HeaderContains, "wp-json", HeaderName: "Link"),
            new(RuleType.HeaderContains, "WP Engine", HeaderName: "X-Powered-By"),
        ]);

    public static readonly DetectorDescriptor[] All =
    [
        WordPress,

        new("WooCommerce", DetectorKind.Software,
        [
            new(RuleType.MetaGenerator, "WooCommerce"),
            new(RuleType.HtmlContains, "woocommerce"),
            new(RuleType.HtmlContains, "wc-blocks"),
            new(RuleType.HtmlContains, "/wc-ajax="),
        ], RequiresParent: WordPress),

        new("Joomla", DetectorKind.Software,
        [
            new(RuleType.MetaGenerator, "Joomla"),
            new(RuleType.HtmlContains, "/media/jui/"),
            new(RuleType.HtmlContains, "/media/system/js/"),
            new(RuleType.HeaderContains, "Joomla", HeaderName: "X-Content-Encoded-By"),
        ]),

        new("Drupal", DetectorKind.Software,
        [
            new(RuleType.MetaGenerator, "Drupal"),
            new(RuleType.HeaderExists, "", HeaderName: "X-Drupal-Cache"),
            new(RuleType.HeaderExists, "", HeaderName: "X-Drupal-Dynamic-Cache"),
            new(RuleType.HeaderContains, "Drupal", HeaderName: "X-Generator"),
            new(RuleType.HtmlContains, "drupal.js"),
            new(RuleType.HtmlContains, "/sites/default/files/"),
            new(RuleType.HtmlContains, "Drupal.settings"),
        ]),

        new("TYPO3", DetectorKind.Software,
        [
            new(RuleType.MetaGenerator, "TYPO3"),
            new(RuleType.HtmlContains, "typo3temp/"),
            new(RuleType.HtmlContains, "typo3conf/"),
        ]),

        new("Contao", DetectorKind.Software,
        [
            new(RuleType.MetaGenerator, "Contao"),
            new(RuleType.HtmlContains, "/assets/contao/"),
            new(RuleType.HtmlContains, "/bundles/contaocore/"),
        ]),

        new("Magento", DetectorKind.Software,
        [
            new(RuleType.HeaderExists, "", HeaderName: "X-Magento-Vary"),
            new(RuleType.HeaderExists, "", HeaderName: "X-Magento-Tags"),
            new(RuleType.HeaderExists, "", HeaderName: "X-Magento-Cache-Control"),
            new(RuleType.HtmlContains, "Magento_"),
            new(RuleType.HtmlContains, "mage/cookies"),
            new(RuleType.HtmlContains, "/static/frontend/Magento/"),
        ]),

        new("Shopware", DetectorKind.Software,
        [
            new(RuleType.MetaGenerator, "Shopware"),
            new(RuleType.HeaderExists, "", HeaderName: "sw-context-token"),
            new(RuleType.HeaderExists, "", HeaderName: "sw-invalidation-states"),
            new(RuleType.HtmlContains, "/bundles/storefront/"),
            new(RuleType.HtmlContains, "/themes/Frontend/Responsive/"),
        ]),

        new("PrestaShop", DetectorKind.Software,
        [
            new(RuleType.MetaGenerator, "PrestaShop"),
            new(RuleType.HeaderContains, "PrestaShop", HeaderName: "Powered-By"),
            new(RuleType.HtmlContains, "prestashop"),
        ]),

        new("OpenCart", DetectorKind.Software,
        [
            new(RuleType.MetaGenerator, "OpenCart"),
            new(RuleType.HtmlContains, "index.php?route=common/home"),
            new(RuleType.HtmlContains, "catalog/view/theme"),
        ]),

        new("nopCommerce", DetectorKind.Software,
        [
            new(RuleType.MetaGenerator, "nopCommerce"),
            new(RuleType.HtmlContains, "nopCommerce"),
            new(RuleType.HtmlContains, "addproducttocart_"),
        ]),

        // ── Tools ───────────────────────────────────────────────────────

        new("Google Analytics", DetectorKind.Tool,
        [
            new(RuleType.HtmlContains, "google-analytics.com/analytics.js"),
            new(RuleType.HtmlContains, "googletagmanager.com/gtag"),
            new(RuleType.HtmlContains, "gtag('config'"),
        ]),

        new("Google Fonts", DetectorKind.Tool,
        [
            new(RuleType.HtmlContains, "fonts.googleapis.com"),
            new(RuleType.HtmlContains, "fonts.gstatic.com"),
        ]),

        new("Cloudflare", DetectorKind.Tool,
        [
            new(RuleType.HeaderExists, "", HeaderName: "cf-ray"),
            new(RuleType.HeaderExists, "", HeaderName: "cf-cache-status"),
            new(RuleType.HeaderContains, "cloudflare", HeaderName: "server"),
        ]),

        new("Matomo", DetectorKind.Tool,
        [
            new(RuleType.HtmlContains, "matomo.js"),
            new(RuleType.HtmlContains, "piwik.js"),
            new(RuleType.HtmlContains, "_paq"),
        ]),

        new("Facebook Pixel", DetectorKind.Tool,
        [
            new(RuleType.HtmlContains, "connect.facebook.net/fbevents.js"),
            new(RuleType.HtmlContains, "fbq('init'"),
        ]),
    ];
}
