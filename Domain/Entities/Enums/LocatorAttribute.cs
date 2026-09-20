using Ardalis.SmartEnum;

namespace Domain.Entities.Enums;

public sealed class LocatorAttribute : SmartEnum<LocatorAttribute>
{
    public static readonly LocatorAttribute Id = new(nameof(Id), 0, mobileSupported: true);
    public static readonly LocatorAttribute Name = new(nameof(Name), 1, mobileSupported: false);
    public static readonly LocatorAttribute Class = new(nameof(Class), 2, mobileSupported: true);
    public static readonly LocatorAttribute Css = new(nameof(Css), 3, mobileSupported: false);
    public static readonly LocatorAttribute XPath = new(nameof(XPath), 4, mobileSupported: true);
    public static readonly LocatorAttribute Text = new(nameof(Text), 5, mobileSupported: false);
    public static readonly LocatorAttribute AccessibilityId = new(nameof(AccessibilityId), 6, mobileSupported: true);

    public bool MobileSupported { get; }

    private LocatorAttribute(string name, int value, bool mobileSupported)
        : base(name, value)
    {
        MobileSupported = mobileSupported;
    }
}