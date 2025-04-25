# Overview

The UI Preview Framework lets you easily work on pages/controls in your app
in isolation, without the need to run the app, navigate to the page, and
supply any test data.

It's similar to Preview in [SwiftUI/Xcode](https://developer.apple.com/documentation/xcode/previewing-your-apps-interface-in-xcode) and [Jetpack Compose/Android Studio](https://developer.android.com/develop/ui/compose/tooling/previews), for .NET UI, especially
when coupled with tooling support like what we're looking to bring to Visual Studio.

The framework itself is cross platform, intended to work with (most) any .NET UI platform -
it has a platform agnostic piece and platform specific piece, with the platform piece pluggable. Initial support is for .NET MAUI.

## How to use (MAUI version)

Add a reference to the `Microsoft.UIPreview.Maui` NuGet to your app (once that NuGet is published - it isn't yet so you'll need to use this repo instead).

Add this line to your `App` constructor (needed for now):

```
#if PREVIEWS
    MauiPreviewApplication.EnsureInitialized();
#endif
```

With just that, if you open your app in a tool that supports it (like what we're addning to Visual Studio) you we'll see previews automatically created for some of pages/controls.
Previews are automatically created for:

- Pages: Derives (directly or indirectly) from `Microsoft.Maui.Controls.Page` and has a constructor that takes no parameters (no view model required).,
- Controls: Dervies from `Microsoft.Maui.Controls.View` (and isn't a page), again with
a constructor that takes no parameters.

That should get you started. Beyond that, you'll probably want to define previews yourself,
which lets you:

- Support any UI component (not just with zero argument constructors)
- Provide sample data
- Define multiple previews for a single UI component

Defining your own previews isn't hard & is similar to what's done in SwiftUI and Jetpack Compose. To do it, add a static to your UI component class (in code behind with XAML) with the `[Preview]` attribute, like below. Instantiate the control, passing in a view model
with sample data or whatever the constructor requires.

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

The `[Preview]` argument is the optional display name - without that, the name
is method name.

Names really just matter when you have a multiple previews. If there's just one,
then by convention it's named `Preview`, but it doesn't matter as the tooling
displays the UI component name instead.

## How to build

In brief, run `init.ps1` then open the solution. For details see [Contibuting](Contributing.md)
