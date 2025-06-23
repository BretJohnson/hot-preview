// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using SkiaSharp;
using VisualTestUtils.ImageHash.HashAlgorithms;

namespace VisualTestUtils.ImageHash
{
    /// <summary>
    /// Verify images using SkiaSharp.
    /// </summary>
    public class ImageHashVisualComparer : IVisualComparer
    {
        private IHashAlgorithm _hashAlgorithm;
        private double _differenceThreshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageHashVisualComparer"/> class, defaulting
        /// to use the <see cref="DifferenceHash"/> algorithm.
        /// </summary>
        /// <param name="differenceThreshold">The maximum percent difference that is allowed between the baseline and actual snapshot images. Default value is .005, meaning the images must be at least 99.5% the same.).</param>
        public ImageHashVisualComparer(double differenceThreshold = 0.005)
            : this(new DifferenceHash(), differenceThreshold)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageHashVisualComparer"/> class.
        /// </summary>
        /// <param name="hashAlgorithm">The Hash Algorithm.</param>
        /// <param name="differenceThreshold">The maximum percent difference that is allowed between the baseline and actual snapshot images. Default value is .005, meaning the images must be at least 99.5% the same.).</param>
        public ImageHashVisualComparer(IHashAlgorithm hashAlgorithm, double differenceThreshold = 0.005)
        {
            _hashAlgorithm = hashAlgorithm;
            _differenceThreshold = differenceThreshold;
        }

        /// <inheritdoc/>
        public ImageDifference? Compare(ImageSnapshot baselineImage, ImageSnapshot actualImage)
        {
            SKBitmap baselineSKBitmap = SKBitmap.Decode(baselineImage.Data);
            SKBitmap actualSKBitmap = SKBitmap.Decode(actualImage.Data);

            ImageSizeDifference? imageSizeDifference = ImageSizeDifference.Compare(baselineSKBitmap.Width, baselineSKBitmap.Height, actualSKBitmap.Width, actualSKBitmap.Height);
            if (imageSizeDifference != null)
                return imageSizeDifference;

            // Get the similarity, 0-100 range (99.0 = 99% same)
            double similarity = CompareHash.Similarity(_hashAlgorithm.Hash(baselineSKBitmap), _hashAlgorithm.Hash(actualSKBitmap));

            // Convert to difference, 0-1 range (.01 = 1% different)
            double difference = (100.0 - similarity) / 100.0;
            if (difference > _differenceThreshold)
                return new ImagePercentageDifference(difference);
            else return null;
        }
    }
}
