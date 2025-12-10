
namespace Common.Core.Test.Extension
{
    public class MimeExtensionsTest
    {
        [Fact]
        public void TestGetMimeType()
        {
            // Test with empty file name
            var mimeType = string.Empty.GetMimeType();
            Assert.Null(mimeType);

            // Test with null file name
            string nullStr = null;
            mimeType = nullStr.GetMimeType();
            Assert.Null(mimeType);
        }

        [Fact]
        public void Jpg_Test()
        {
            var fileName = "11.jpg";
            var mime = fileName.GetMimeType();
            Assert.NotEmpty(mime);
        }

        [Fact]
        public void Pdf_Test()
        {
            var fileName = "test.file.pdf";
            var mimeType = fileName.GetMimeType();
            Assert.Equal("application/pdf", mimeType);
        }

        [Fact]
        public void Jpeg_Test()
        {
            var fileName = "image.JPEG";
            var mimeType = fileName.GetMimeType();
            Assert.Equal("image/jpeg", mimeType);
        }
    }
}