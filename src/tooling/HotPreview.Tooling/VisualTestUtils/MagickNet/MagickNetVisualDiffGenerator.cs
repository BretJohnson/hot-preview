// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageMagick;

namespace HotPreview.Tooling.VisualTestUtils.MagickNet
{
    /// <summary>
    /// Verify images using ImageMagick.
    /// </summary>
    public class MagickNetVisualDiffGenerator : IVisualDiffGenerator
    {
        private Channels _channelsToCompare;

        public MagickNetVisualDiffGenerator(Channels channelsToCompare = Channels.RGBA)
        {
            _channelsToCompare = channelsToCompare;
        }

        public ImageSnapshot GenerateDiff(ImageSnapshot baselineImage, ImageSnapshot actualImage)
        {
            var magickBaselineImage = new MagickImage(baselineImage.Data);
            var magickActualImage = new MagickImage(actualImage.Data);

            // Create a copy of the baseline image to use as the diff image
            MagickImage magickDiffImage = (MagickImage)magickBaselineImage.Clone();
            magickDiffImage.Format = MagickFormat.Png;

            // Use Composite with Difference operator to generate the diff
            magickDiffImage.Composite(magickActualImage, CompositeOperator.Difference, _channelsToCompare);

            return new ImageSnapshot(magickDiffImage.ToByteArray(), ImageSnapshotFormat.PNG);
        }
    }
}
