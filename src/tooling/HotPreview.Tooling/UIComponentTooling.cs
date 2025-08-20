using System;
using System.Collections.Generic;
using System.Linq;
using HotPreview.SharedModel;

namespace HotPreview.Tooling;

public class UIComponentTooling(UIComponentKind kind, string typeName, string? displayNameOverride, IReadOnlyList<PreviewTooling> previews) :
    UIComponentBase<PreviewTooling>(kind, displayNameOverride, previews)
{
    private Dictionary<string, List<PreviewTooling>>? _previewsBySimpleName;

    public override string Name => typeName;

    public override UIComponentBase<PreviewTooling> WithAddedPreview(PreviewTooling preview) =>
        new UIComponentTooling(Kind, typeName, DisplayNameOverride, GetUpdatedPreviews(preview));

    /// <summary>
    /// Populates the _previewsBySimpleName dictionary on first use.
    /// </summary>
    private void PopulatePreviewsBySimpleName()
    {
        if (_previewsBySimpleName is not null)
        {
            return;
        }

        _previewsBySimpleName = new Dictionary<string, List<PreviewTooling>>();

        foreach (PreviewTooling preview in Previews)
        {
            string simpleName = preview.Name.Split('.').Last();

            if (!_previewsBySimpleName.TryGetValue(simpleName, out List<PreviewTooling>? previewList))
            {
                previewList = new List<PreviewTooling>();
                _previewsBySimpleName[simpleName] = previewList;
            }

            previewList.Add(preview);
        }
    }

    /// <summary>
    /// Gets the shortest unique name for a preview within this UI component.
    /// Returns the simple name if unique, otherwise the shortest name that is unique at dot boundaries.
    /// This is used for snapshot file names, to keep them short but ensure there are no collisions.
    /// </summary>
    /// <param name="previewName">The full name of the preview.</param>
    /// <returns>The shortest unique name.</returns>
    /// <exception cref="ArgumentException">Thrown when the preview doesn't exist.</exception>
    public string GetPreviewShortName(string previewName)
    {
        if (GetPreview(previewName) is null)
        {
            throw new ArgumentException($"Preview '{previewName}' not found in UI component '{Name}'.", nameof(previewName));
        }

        PopulatePreviewsBySimpleName();

        string[] nameParts = previewName.Split('.');
        string simpleName = nameParts.Last();

        // If the simple name is unique, return it
        if (_previewsBySimpleName![simpleName].Count == 1)
        {
            return simpleName;
        }

        // Find the shortest name that is unique
        for (int segmentCount = 2; segmentCount <= nameParts.Length; segmentCount++)
        {
            string candidateName = string.Join(".", nameParts.Skip(nameParts.Length - segmentCount));

            // Check if this candidate name is unique among all previews with the same simple name
            List<PreviewTooling> previewsWithSameSimpleName = _previewsBySimpleName[simpleName];
            IEnumerable<PreviewTooling> conflictingPreviews = previewsWithSameSimpleName
                .Where(p => p.Name != previewName)
                .Where(p =>
                {
                    string[] otherParts = p.Name.Split('.');
                    if (otherParts.Length < segmentCount)
                    {
                        return false;
                    }
                    string otherCandidateName = string.Join(".", otherParts.Skip(otherParts.Length - segmentCount));
                    return otherCandidateName == candidateName;
                });

            if (!conflictingPreviews.Any())
            {
                return candidateName;
            }
        }

        // Fallback to full name if no shorter unique name found
        return previewName;
    }
}
