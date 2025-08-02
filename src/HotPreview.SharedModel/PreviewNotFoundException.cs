using System;

namespace HotPreview.SharedModel;

public class PreviewNotFoundException(string message) : Exception(message)
{
}
