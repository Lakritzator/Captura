using System;
using System.Drawing;
using Captura.Base.Images;
using Moq;
using Screna.ImageProviders;
using Tests.Fixtures;
using Xunit;

namespace Tests
{
    [Collection(nameof(Tests))]
    public class ImageProviderTests
    {
        readonly MoqFixture _moq;

        public ImageProviderTests(MoqFixture moq)
        {
            _moq = moq;
        }

        [Fact]
        public void OverlayImageProviderNull()
        {
            var overlay = _moq.GetOverlayMock().Object;

            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new OverlayedImageProvider(null, point => point, overlay)) { }
            });
        }

        [Fact]
        public void OverlaysNull()
        {
            var imageProvider = _moq.GetImageProviderMock().Object;

            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new OverlayedImageProvider(imageProvider, point => point, null)) { }
            });
        }

        [Fact]
        public void OverlaysTransformNull()
        {
            var imageProvider = _moq.GetImageProviderMock().Object;
            var overlay = _moq.GetOverlayMock().Object;

            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new OverlayedImageProvider(imageProvider, null, overlay)) { }
            });
        }

        [Fact]
        public void WindowProviderNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (new WindowProvider(null, false, out var _)) { }
            });
        }

        [Fact]
        public void RegionImageSize()
        {
            var rect = new Rectangle(0, 0, 100, 100);

            using (var imgProvider = new RegionProvider(rect, false))
            {
                Assert.Equal(imgProvider.Width, rect.Width);
                Assert.Equal(imgProvider.Height, rect.Height);

                using (var img = imgProvider.Capture())
                {
                    Assert.Equal(img.Width, rect.Width);
                    Assert.Equal(img.Height, rect.Height);
                }
            }
        }

        [Fact]
        public void RegionImageSizeOdd()
        {
            var rect = new Rectangle(0, 0, 101, 53);

            using (var imgProvider = new RegionProvider(rect, false))
            {
                Assert.True(imgProvider.Width % 2 == 0);
                Assert.True(imgProvider.Height % 2 == 0);

                using (var img = imgProvider.Capture())
                {
                    Assert.Equal(img.Width, imgProvider.Width);
                    Assert.Equal(img.Height, imgProvider.Height);

                    Assert.True(img.Width % 2 == 0);
                    Assert.True(img.Height % 2 == 0);
                }
            }
        }

        [Fact]
        public void OverlayedSize()
        {
            var imgProvider = _moq.GetImageProviderMock().Object;
            var overlay = _moq.GetOverlayMock().Object;

            using (var provider = new OverlayedImageProvider(imgProvider, point => point, overlay))
            {
                Assert.Equal(provider.Width, imgProvider.Width);
                Assert.Equal(provider.Height, imgProvider.Height);

                using (var img = provider.Capture())
                {
                    Assert.Equal(provider.Width, img.Width);
                    Assert.Equal(provider.Height, img.Height);
                }
            }
        }

        [Fact]
        public void CaptureOverlayedImage()
        {
            var imgProviderMock = _moq.GetImageProviderMock();
            var overlayMock = _moq.GetOverlayMock();

            using (var provider = new OverlayedImageProvider(imgProviderMock.Object, point => point, overlayMock.Object))
            {
                using (provider.Capture())
                {
                    imgProviderMock.Verify(imageProvider => imageProvider.Capture(), Times.Once);
                    overlayMock.Verify(overlay => overlay.Draw(It.IsAny<IEditableFrame>(), It.IsAny<Func<Point, Point>>()), Times.Once);
                }
            }

            imgProviderMock.Verify(imageProvider => imageProvider.Dispose(), Times.Once);
            overlayMock.Verify(overlay => overlay.Dispose(), Times.Once);
        }
    }
}