using System.Runtime.InteropServices;

namespace HotPreview.Tooling.VisualTestUtils
{
    public class VisualRegressionTester
    {
        private readonly string _snapshotsDirectory;
        private readonly string _snapshotsDiffDirectory;
        private readonly IVisualComparer _visualComparer;
        private readonly IVisualDiffGenerator _visualDiffGenerator;
        private readonly bool _isCI;

        /// <summary>
        /// Initialize visual regression testing, configured as specified.
        /// </summary>
        /// <param name="testRootDirectory">The root directory for the tests. This directory should have a "snapshots" subdirectory with the baseline images.</param>
        /// <param name="visualComparer">The instance of <see cref="IVisualComparer"/> that will be used for image comparison.</param>
        /// <param name="visualDiffGenerator">The instance of <see cref="IVisualDiffGenerator"/> that will be used for generating image diff.</param>
        /// <param name="ciArtifactsDirectory">If running in CI, this should be set to the CI artifacts directory. When running locally, it can be null (the default). If specified, the "snapshots-diff" subdirectory will be created here,
        /// containing the actual and diff images for any failed tests. This allows the CI to upload the images as artifacts, making it easy to see what changed.</param>
        public VisualRegressionTester(string testRootDirectory, IVisualComparer visualComparer, IVisualDiffGenerator visualDiffGenerator, string? ciArtifactsDirectory = null)
        {
            _snapshotsDirectory = Path.Combine(testRootDirectory, "snapshots");
            _visualComparer = visualComparer;
            _visualDiffGenerator = visualDiffGenerator;

            _isCI = ciArtifactsDirectory != null;
            if (_isCI)
                _snapshotsDiffDirectory = Path.Combine(ciArtifactsDirectory!, "snapshots-diff");
            else
                _snapshotsDiffDirectory = Path.Combine(testRootDirectory, "snapshots-diff");
        }

        /// <summary>
        /// Verify that the actual image matches the baseline snapshot. If not, fail the test.
        /// </summary>
        /// <param name="name">Name for the snapshot. This will be used as the file name for the baseline image.</param>
        /// <param name="actualImage">The actual image to compare against the baseline.</param>
        /// <param name="environmentName">Optional environment name (e.g. "windows", "macos", "android"). If specified, the baseline image will be stored in a subdirectory with this name.</param>
        /// <param name="testContext">Optional client provied test context, used to attach screenshots/diff images to failed tests if supported by client test framework</param>
        public virtual void VerifyMatchesSnapshot(string name, ImageSnapshot actualImage, string? environmentName = null,
            ITestContext? testContext = null)
        {
            string imageFileName = $"{name}{actualImage.Format.GetFileExtension()}";

            string snapshotsEnvironmentDirectory = GetEnvironmentDirectory(this._snapshotsDirectory, environmentName);
            string baselineImagePath = Path.Combine(snapshotsEnvironmentDirectory, imageFileName);

            if (!File.Exists(baselineImagePath))
            {
                if (_isCI)
                {
                    Fail($"Baseline snapshot doesn't exist: {baselineImagePath}");
                }
                else
                {
                    // When running locally, if the baseline doesn't exist, create it
                    Directory.CreateDirectory(snapshotsEnvironmentDirectory);
                    actualImage.Save(snapshotsEnvironmentDirectory, name);
                    return;
                }
            }

            ImageSnapshot baselineImage = new ImageSnapshot(baselineImagePath);

            string diffEnvironmentDirectory = GetEnvironmentDirectory(this._snapshotsDiffDirectory, environmentName);
            string diffDirectoryImagePath = Path.Combine(diffEnvironmentDirectory, imageFileName);
            string diffDirectoryDiffImagePath = Path.Combine(diffEnvironmentDirectory, $"{name}-diff{actualImage.Format.GetFileExtension()}");

            ImageDifference? imageDifference = _visualComparer.Compare(baselineImage, actualImage);
            if (imageDifference != null)
            {
                Directory.CreateDirectory(diffEnvironmentDirectory);
                actualImage.Save(diffEnvironmentDirectory, name);

                ImageSnapshot diffImage = _visualDiffGenerator.GenerateDiff(baselineImage, actualImage);
                diffImage.Save(diffEnvironmentDirectory, name + "-diff");

                testContext?.AddTestAttachment(diffDirectoryImagePath);
                testContext?.AddTestAttachment(diffDirectoryDiffImagePath);

                string message = $"Snapshot comparison failed for '{name}': {imageDifference.Description}";
                if (!_isCI)
                {
                    message += $"\n\nActual image: {diffDirectoryImagePath}";
                    message += $"\nDiff image: {diffDirectoryDiffImagePath}";
                    message += $"\nBaseline image: {baselineImagePath}";
                    message += $"\n\nTo update the baseline, delete it and re-run the test.";
                }

                Fail(message);
            }
        }

        /// <summary>
        /// Verify that the actual image matches the baseline snapshot, cropping the image first. If not, fail the test.
        /// </summary>
        /// <param name="name">Name for the snapshot. This will be used as the file name for the baseline image.</param>
        /// <param name="actualImage">The actual image to compare against the baseline.</param>
        /// <param name="imageEditorFactory">Factory to create an image editor, used to crop the image</param>
        /// <param name="cropX">Crop rectangle left</param>
        /// <param name="cropY">Crop rectangle top</param>
        /// <param name="cropWidth">Crop rectangle width</param>
        /// <param name="cropHeight">Crop rectangle height</param>
        /// <param name="environmentName">Optional environment name (e.g. "windows", "macos", "android"). If specified, the baseline image will be stored in a subdirectory with this name.</param>
        /// <param name="testContext">Optional client provied test context, used to attach screenshots/diff images to failed tests if supported by client test framework</param>
        public void VerifyMatchesSnapshot(string name, ImageSnapshot actualImage, IImageEditorFactory imageEditorFactory, int cropX, int cropY, uint cropWidth, uint cropHeight, string? environmentName = null, ITestContext? testContext = null)
        {
            IImageEditor imageEditor = imageEditorFactory.CreateImageEditor(actualImage);
            imageEditor.Crop(cropX, cropY, cropWidth, cropHeight);
            ImageSnapshot croppedImage = imageEditor.GetUpdatedImage();

            VerifyMatchesSnapshot(name, croppedImage, environmentName, testContext);
        }

        private static string GetEnvironmentDirectory(string baseDirectory, string? environmentName)
        {
            if (environmentName == null)
            {
                // If no environment name is specified, use the current platform as the default
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    environmentName = "windows";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    environmentName = "macos";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    environmentName = "linux";
                else
                    environmentName = "unknown";
            }

            return Path.Combine(baseDirectory, environmentName);
        }

        public void Fail(string message)
        {
            // For multiline messages, ensure they start on a new line to be better formatted in VS test explorer results
            if (message.Contains('\n') && !message.StartsWith("\n"))
            {
                message = "\n" + message;
            }

            throw new VisualTestFailedException(message);
        }
    }
}
