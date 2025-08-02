## UI Component Preview Instructions (Hot Preview)

**When adding or modifying UI components, follow this workflow:**

1. **Ensure Preview Exists**: Before implementing a new UI component, verify that a corresponding preview exists. If no preview exists, create one or more that demonstrate the component's key features and states. Use this format for the preview methods:

```csharp
#if PREVIEWS
[Preview]
public static MyComponent MyPreview() => new MyComponent(<my test data>);
#endif
```

If the UI component has a constructor that takes no parameters, or has a constructor with parameters that can be resolved via dependency injection, you can omit the preview method and the tooling will automatically generate one for you.

2. **Capture Preview Screenshot**: After implementing or updating the component, use the `get_preview_snapshot` MCP tool with the exact preview name as the parameter to capture a visual snapshot of the component. The preview name is the fully qualified method name, including the namespace and class name, for the [Preview] method. For an auto-generated preview, the name is the fully qualified name of the UI component class.

3. **Visual Analysis**: Carefully examine the returned screenshot and verify:
  - The component renders correctly without visual errors
  - Layout and spacing appear as intended
  - Colors, fonts, and styling match the design requirements
  - Interactive elements (buttons, inputs, etc.) are properly positioned
  - The component displays correctly in its intended context

4. **Iterate if Needed**: If the visual analysis reveals issues such as:
  - Misaligned elements
  - Incorrect styling
  - Missing visual elements
  - Layout problems
  - Rendering errors

  Then make the necessary code corrections and repeat steps 2-4 until the component appears correctly.

**Important**: Always capture and review the visual preview before considering a component implementation complete. The screenshot serves as visual validation that your code changes produce the intended result.RetryClaude can make mistakes. Please double-check responses.
