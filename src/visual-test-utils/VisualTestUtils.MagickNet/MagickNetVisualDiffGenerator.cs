// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageMagick;

namespace VisualTestUtils.MagickNet
{
    /// <summary>
    /// Verify images using ImageMagick.
    /// </summary>
    public class MagickNetVisualDiffGenerator : IVisualDiffGenerator
    {
        private ErrorMetric _errorMetric;
        private Channels _channelsToCompare;

        public MagickNetVisualDiffGenerator(ErrorMetric error = ErrorMetric.Fuzz, Channels channelsToCompare = Channels.RGBA)
        {
            _errorMetric = error;
            _channelsToCompare = channelsToCompare;
        }

        public ImageSnapshot GenerateDiff(ImageSnapshot baselineImage, ImageSnapshot actualImage)
        {
            var magickBaselineImage = new MagickImage(baselineImage.Data);
            var magickActualImage = new MagickImage(actualImage.Data);

            var magickDiffImage = new MagickImage();
            magickDiffImage.Format = MagickFormat.Png;

            magickBaselineImage.Compare(magickActualImage, _errorMetric, magickDiffImage, _channelsToCompare);

            return new ImageSnapshot(magickDiffImage.ToByteArray(), ImageSnapshotFormat.PNG);
        }
    }
}
