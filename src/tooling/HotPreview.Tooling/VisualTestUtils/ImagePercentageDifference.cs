namespace HotPreview.Tooling.VisualTestUtils
{
    public class ImagePercentageDifference : ImageDifference
    {
        private double _percentage;

        public ImagePercentageDifference(double percentage)
        {
            _percentage = percentage;
        }

        public override string Description =>
            string.Format("{0:0.00}% difference", _percentage * 100.0);
    }
}
