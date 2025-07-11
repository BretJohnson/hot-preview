using ImageMagick;

namespace HotPreview.Tooling.VisualTestUtils.MagickNet
{
    /// <summary>
    /// Verify images using ImageMagick.
    /// </summary>
    public class MagickNetVisualComparer : IVisualComparer
    {
        private ErrorMetric _errorMetric;
        private double _differenceThreshold;
        private Channels _channelsToCompare;

        /// <summary>
        /// Initializes a new instance of the <see cref="MagickNetVisualComparer"/> class.
        /// </summary>
        /// <param name="errorMetric">Error metric.</param>
        /// <param name="differenceThreshold">The maximum percent difference that is allowed between the baseline and actual snapshot images. Default value is .005, meaning the images must be at least 99.5% the same.).</param>
        public MagickNetVisualComparer(ErrorMetric errorMetric = ErrorMetric.Fuzz, double differenceThreshold = 0.005, Channels channelsToCompare = Channels.RGBA)
        {
            _errorMetric = errorMetric;
            _differenceThreshold = differenceThreshold;
            _channelsToCompare = channelsToCompare;
        }

        /// <inheritdoc/>
        public ImageDifference? Compare(ImageSnapshot baselineImage, ImageSnapshot actualImage)
        {
            var magickBaselineImage = new MagickImage(baselineImage.Data);
            var magickActualImage = new MagickImage(actualImage.Data);

            ImageSizeDifference? imageSizeDifference = ImageSizeDifference.Compare(magickBaselineImage.Width, magickBaselineImage.Height, magickActualImage.Width, magickActualImage.Height);
            if (imageSizeDifference != null)
                return imageSizeDifference;

            double distortionDifference = magickBaselineImage.Compare(magickActualImage, _errorMetric, _channelsToCompare);
            if (distortionDifference > _differenceThreshold)
                return new ImagePercentageDifference(distortionDifference);

            return null;
        }
    }
}
