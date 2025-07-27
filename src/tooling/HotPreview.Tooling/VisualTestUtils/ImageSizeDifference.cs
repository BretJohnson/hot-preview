namespace HotPreview.Tooling.VisualTestUtils
{
    public class ImageSizeDifference : ImageDifference
    {
        private uint _baselineWidth;
        private uint _baselineHeight;
        private uint _actualWidth;
        private uint _actualHeight;

        public ImageSizeDifference(uint baselineWidth, uint baselineHeight, uint actualWidth, uint actualHeight)
        {
            _baselineWidth = baselineWidth;
            _baselineHeight = baselineHeight;
            _actualWidth = actualWidth;
            _actualHeight = actualHeight;
        }

        public override string Description =>
            $"size differs - baseline is {_baselineWidth}x{_baselineHeight} pixels, actual is {_actualWidth}x{_actualHeight} pixels";

        public static ImageSizeDifference? Compare(uint baselineWidth, uint baselineHeight, uint actualWidth, uint actualHeight)
        {
            if (baselineWidth != actualWidth || baselineHeight != actualHeight)
                return new ImageSizeDifference(baselineWidth, baselineHeight, actualWidth, actualHeight);
            else
                return null;
        }
    }
}
