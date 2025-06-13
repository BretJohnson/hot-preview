// Copyright (c) Bret Johnson. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

public class ExamplesConstantTests
{
    [Fact]
    public void ExamplesConstant_ShouldBeDefinedInDebugConfiguration()
    {
        // This test verifies that the EXAMPLES constant is properly defined
        // when the ExampleFramework NuGet package is consumed
        
#if EXAMPLES
        // If we get here, EXAMPLES is defined
        Assert.True(true, "EXAMPLES constant is properly defined");
#else
        // This should only happen in Release builds or if the props file isn't working
        Assert.True(false, "EXAMPLES constant should be defined in Debug configuration but was not found");
#endif
    }
    
    [Fact]
    public void ExamplesConstant_ConfigurationTest()
    {
        // This test provides more detailed info about the configuration
#if DEBUG
        // We're in Debug mode
#if EXAMPLES
        Assert.True(true, "EXAMPLES constant is correctly defined in Debug configuration");
#else
        Assert.True(false, "EXAMPLES constant is missing in Debug configuration - props file may not be working");
#endif
#else
        // We're in Release mode - EXAMPLES might not be defined and that's OK
        Assert.True(true, "Test is running in Release configuration");
#endif
    }
}