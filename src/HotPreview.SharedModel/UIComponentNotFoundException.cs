using System;

namespace HotPreview.SharedModel;

public class UIComponentNotFoundException(string message) : Exception(message)
{
}
