---
_layout: landing
---

# Hot Preview Documentation

Hot Preview lets you easily work on pages and controls in your app in isolation, making UI development and testing faster and more efficient for both humans and AI agents.

Previews are similar to stories in [Storybook](https://storybook.js.org/) for JavaScript and Previews in [SwiftUI/Xcode](https://developer.apple.com/documentation/xcode/previewing-your-apps-interface-in-xcode) and [Jetpack Compose/Android Studio](https://developer.android.com/develop/ui/compose/tooling/previews) — but for .NET UI.

## Quick Start

Get started with HotPreview in minutes:

1. **Install Hot Preview DevTools:**
   ```bash
   dotnet tool install -g HotPreview.DevTools
   ```

2. **Add package reference to your app:**
   ```xml
   <PackageReference Condition="$(Configuration) == 'Debug'" Include="HotPreview.App.Maui" Version="..." />
   ```

3. **Build and run your app in Debug mode**

## Features

- 🚀 **Streamlined Navigation** - Jump directly to specific UI pages without complex navigation
- 🔄 **Multi-state Testing** - Visualize components with different data inputs and states
- 📱 **Cross-platform Preview** - View UI on multiple platforms simultaneously
- 🤖 **AI Integration** - Built-in MCP server for agentic AI workflows *(Coming Soon)*

[Get Started →](docs/getting-started.md) | [API Reference →](~/api/HotPreview.yml)
