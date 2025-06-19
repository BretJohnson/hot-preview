﻿using System;

namespace PreviewFramework.SharedModel;

public class UIComponentReflection : UIComponentBase<PreviewReflection>
{
    internal UIComponentReflection(Type type, UIComponentKind kind, string? displayName) : base(kind, displayName)
    {
        Type = type;
    }

    public Type Type { get; }

    public override string Name => Type.FullName;

}
