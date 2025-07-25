# Overview

Hot Preview lets you easily work on pages and controls in your app in isolation, without the
need to run the app, navigate to the page, and supply test data.

Previews are similar to stories in [Storybook](https://storybook.js.org/) for JavaScript and Previews in
[SwiftUI/Xcode](https://developer.apple.com/documentation/xcode/previewing-your-apps-interface-in-xcode)
and [Jetpack Compose/Android Studio](https://developer.android.com/develop/ui/compose/tooling/previews) —
but for .NET UI.

The framework itself is cross-platform, intended to work with most .NET UI platforms.
It has a platform-agnostic piece and a platform-specific piece, with the platform piece being pluggable.
Initial support is for .NET MAUI.

## Current Status

Hot Preview is in active development. It works and I encourage you to use it, but
expect to encounter some issues. Please report them and share other feedback.

## How to use

Quickstart:

- Install Hot Preview DevTools with `dotnet tool install -g HotPreview.DevTools`
- In your app, add a reference to the `HotPreview.App.<platform>` NuGet, e.g.:
    ```XML
    <PackageReference Condition="$(Configuration) == 'Debug'" Include="HotPreview.App.Maui" Version="..." />
    ```
- Build your app for Debug and run it

With that, you should see this:
- Building launches the Hot Preview DevTools app, if it's not already running
- When your app starts, it connects to the DevTools app
- DevTools shows a tree of your UI components and their previews. Initially, it will only have auto-generated previews, but you can add your own later.
- Click on UI components/previews in the tree to navigate directly to the page/control in your app

Previews are automatically created for:

- **Pages**: Derives (directly or indirectly) from `Microsoft.Maui.Controls.Page` and has a constructor that takes no parameters (no view model required) or with parameters that can be resolved via dependency injection (via ActivatorUtilities.CreateInstance).
- **Controls**: Derives from `Microsoft.Maui.Controls.View` (and isn't a page), again with a constructor that takes no parameters or with parameters that can be resolved via dependency injection (via ActivatorUtilities.CreateInstance).

That should get you started. Beyond that, you'll probably want to define previews yourself, which lets you:

- Support any UI component, regardless of constructor arguments (e.g., you can pass in a view model argument)
- Provide sample data
- Define multiple previews for a single UI component with different data
- Update global app state if needed for a particular preview

Defining your own previews is easy and is similar to what's done in SwiftUI and Jetpack Compose. To do it, add a static method to your UI component class (in code-behind with XAML) with the `[Preview]` attribute, like below. Instantiate the control, passing in a view model with sample data or whatever the constructor requires. These static `[Preview]` methods can actually go in any class, but by convention they are normally defined on the UI component class. `#if PREVIEWS` allows the preview code to be excluded from release builds.

```C#
#if PREVIEWS
    [Preview]
    public static ConfirmAddressView Preview() => new(PreviewData.GetPreviewProducts(1), new DeliveryTypeModel(),
        new AddressModel()
        {
            StreetOne = "21, Alex Davidson Avenue",
            StreetTwo = "Opposite Omegatron, Vicent Quarters",
            City = "Victoria Island",
            State = "Lagos State"
        });
#endif
```

You can define multiple methods for multiple previews, like:

```C#
#if PREVIEWS
    [Preview("0 cards")]
    public static CardView NoCards() => new(PreviewData.GetPreviewCards(0));

    [Preview("1 card")]
    public static CardView SingleCard() => new(PreviewData.GetPreviewCards(1));

    [Preview("2 cards")]
    public static CardView TwoCards() => new(PreviewData.GetPreviewCards(2));

    [Preview("6 cards")]
    public static CardView SixCards() => new(PreviewData.GetPreviewCards(6));
#endif
```

The `[Preview]` argument is the optional display name — without that, the name
is the method name.

Names really only matter when you have multiple previews. If there's just one,
then by convention it's named `Preview`, but it doesn't matter since the tooling
displays the UI component name instead.

## Building this repo and contributing updates

See [Contributing](CONTRIBUTING.md)
