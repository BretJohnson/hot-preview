namespace VisualTestUtils
{
    public class ImageSizeDifference : ImageDifference
    {
        private int _baselineWidth;
        private int _baselineHeight;
        private int _actualWidth;
        private int _actualHeight;

        public ImageSizeDifference(int baselineWidth, int baselineHeight, int actualWidth, int actualHeight)
        {
            _baselineWidth = baselineWidth;
            _baselineHeight = baselineHeight;
            _actualWidth = actualWidth;
            _actualHeight = actualHeight;
        }

        public override string Description =>
            $"size differs - baseline is {_baselineWidth}x{_baselineHeight} pixels, actual is {_actualWidth}x{_actualHeight} pixels";

        public static ImageSizeDifference? Compare(int baselineWidth, int baselineHeight, int actualWidth, int actualHeight) =>
            baselineWidth != actualWidth || baselineHeight != actualHeight
                ? new ImageSizeDifference(baselineWidth, baselineHeight, actualWidth, actualHeight)
                : null;
    }
}
