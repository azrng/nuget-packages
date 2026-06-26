using Azrng.Core.Exceptions;
using Azrng.Core.Helpers;
using FluentAssertions;
using System.Xml;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class IOHelperTests : IDisposable
{
    private readonly string _tempDir;

    public IOHelperTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"IOHelperTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, true); } catch { }
        }
    }

    #region 静态字段

    [Fact]
    public void ApplicationDataPath_ShouldNotBeNullOrEmpty()
    {
        IOHelper.ApplicationDataPath.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DesktopPath_ShouldNotBeNullOrEmpty()
    {
        IOHelper.DesktopPath.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region CheckExt

    [Theory]
    [InlineData("txt")]
    [InlineData("xml")]
    [InlineData("json")]
    [InlineData("cs")]
    public void CheckExt_ValidExtension_ReturnsTrue(string ext)
    {
        IOHelper.CheckExt(ext).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("t xt")]
    [InlineData("t*xt")]
    public void CheckExt_InvalidExtension_ReturnsFalse(string ext)
    {
        IOHelper.CheckExt(ext).Should().BeFalse();
    }

    #endregion

    #region CheckFolderPath

    [Theory]
    [InlineData("C:\\folder", true)]
    [InlineData("C:\\folder\\", true)]
    [InlineData("C:", true)]
    [InlineData("C:\\", true)]
    public void CheckFolderPath_ValidPaths_ReturnsTrue(string path, bool isAvailDiskRootPath)
    {
        IOHelper.CheckFolderPath(path, isAvailDiskRootPath).Should().BeTrue();
    }

    [Theory]
    [InlineData("C:", false)]
    [InlineData("C:\\", false)]
    public void CheckFolderPath_DiskRootPath_WhenNotAllowed_ReturnsFalse(string path, bool isAvailDiskRootPath)
    {
        IOHelper.CheckFolderPath(path, isAvailDiskRootPath).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("folder")]
    [InlineData("C:\\folder ")]
    [InlineData("C:\\folder.")]
    [InlineData("C:\\fol*der")]
    public void CheckFolderPath_InvalidPaths_ReturnsFalse(string path)
    {
        IOHelper.CheckFolderPath(path).Should().BeFalse();
    }

    #endregion

    #region CheckFilePath

    [Theory]
    [InlineData("C:\\1.txt")]
    [InlineData("C:\\folder\\1.txt")]
    [InlineData("C:\\folder\\1txt")]
    public void CheckFilePath_ValidPaths_ReturnsTrue(string path)
    {
        IOHelper.CheckFilePath(path).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("folder")]
    [InlineData("C:")]
    public void CheckFilePath_InvalidPaths_ReturnsFalse(string path)
    {
        IOHelper.CheckFilePath(path).Should().BeFalse();
    }

    [Fact]
    public void CheckFilePath_WithWildcardExt_ValidPathWithExtension_ReturnsTrue()
    {
        IOHelper.CheckFilePath("C:\\folder\\1.txt", "*").Should().BeTrue();
    }

    [Fact]
    public void CheckFilePath_WithWildcardExt_PathWithoutExtension_ReturnsFalse()
    {
        IOHelper.CheckFilePath("C:\\folder\\1txt", "*").Should().BeFalse();
    }

    [Fact]
    public void CheckFilePath_WithSpecificExt_MatchingExt_ReturnsTrue()
    {
        IOHelper.CheckFilePath("C:\\folder\\1.txt", "txt").Should().BeTrue();
    }

    [Fact]
    public void CheckFilePath_WithSpecificExt_NonMatchingExt_ReturnsFalse()
    {
        IOHelper.CheckFilePath("C:\\folder\\1.txt", "xml").Should().BeFalse();
    }

    [Fact]
    public void CheckFilePath_WithInvalidExt_ThrowsParameterException()
    {
        Action act = () => IOHelper.CheckFilePath("C:\\folder\\1.txt", "t xt");

        act.Should().Throw<ParameterException>();
    }

    #endregion

    #region RemoveReadonly

    [Fact]
    public void RemoveReadonly_File_RemovesReadonlyAttribute()
    {
        var filePath = Path.Combine(_tempDir, "readonly.txt");
        File.WriteAllText(filePath, "test");
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        IOHelper.RemoveReadonly(filePath, true);

        var attrs = File.GetAttributes(filePath);
        attrs.Should().NotHaveFlag(FileAttributes.ReadOnly);
    }

    [Fact]
    public void RemoveReadonly_FileDoesNotExist_DoesNotThrow()
    {
        var filePath = Path.Combine(_tempDir, "nonexistent.txt");

        Action act = () => IOHelper.RemoveReadonly(filePath, true);

        act.Should().NotThrow();
    }

    [Fact]
    public void RemoveReadonly_InvalidFilePath_ThrowsParameterException()
    {
        Action act = () => IOHelper.RemoveReadonly("", true);

        act.Should().Throw<ParameterException>();
    }

    [Fact]
    public void RemoveReadonly_Folder_RemovesAttributes()
    {
        var folderPath = Path.Combine(_tempDir, "readonlyfolder");
        Directory.CreateDirectory(folderPath);

        Action act = () => IOHelper.RemoveReadonly(folderPath, false);

        act.Should().NotThrow();
    }

    [Fact]
    public void RemoveReadonly_InvalidFolderPath_ThrowsParameterException()
    {
        Action act = () => IOHelper.RemoveReadonly("", false);

        act.Should().Throw<ParameterException>();
    }

    #endregion

    #region CreateFolder

    [Fact]
    public void CreateFolder_ValidPath_CreatesDirectory()
    {
        var folderPath = Path.Combine(_tempDir, "newfolder");

        IOHelper.CreateFolder(folderPath);

        Directory.Exists(folderPath).Should().BeTrue();
    }

    [Fact]
    public void CreateFolder_AlreadyExists_DoesNotThrow()
    {
        var folderPath = Path.Combine(_tempDir, "existingfolder");
        Directory.CreateDirectory(folderPath);

        Action act = () => IOHelper.CreateFolder(folderPath);

        act.Should().NotThrow();
        Directory.Exists(folderPath).Should().BeTrue();
    }

    [Fact]
    public void CreateFolder_InvalidPath_ThrowsParameterException()
    {
        Action act = () => IOHelper.CreateFolder("");

        act.Should().Throw<ParameterException>();
    }

    #endregion

    #region GetFolder

    [Fact]
    public void GetFolder_ValidFilePath_ReturnsFolderPath()
    {
        var result = IOHelper.GetFolder("C:\\folder\\file.txt");

        result.Should().Be("C:\\folder\\");
    }

    [Fact]
    public void GetFolder_FileInSubFolder_ReturnsFolderPath()
    {
        var result = IOHelper.GetFolder("C:\\a\\b\\c.txt");

        result.Should().Be("C:\\a\\b\\");
    }

    [Fact]
    public void GetFolder_InvalidPath_ThrowsParameterException()
    {
        Action act = () => IOHelper.GetFolder("");

        act.Should().Throw<ParameterException>();
    }

    #endregion

    #region IsFileLocked

    [Fact]
    public void IsFileLocked_UnlockedFile_ReturnsFalse()
    {
        var filePath = Path.Combine(_tempDir, "unlocked.txt");
        File.WriteAllText(filePath, "test");

        var result = IOHelper.IsFileLocked(filePath);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsFileLocked_LockedFile_ReturnsTrue()
    {
        var filePath = Path.Combine(_tempDir, "locked.txt");
        File.WriteAllText(filePath, "test");

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var result = IOHelper.IsFileLocked(filePath);

        result.Should().BeTrue();
    }

    #endregion

    #region XML 操作

    [Fact]
    public void CreateXml_WithXmlDoc_CreatesFile()
    {
        var filePath = Path.Combine(_tempDir, "test.xml");
        var xmlDoc = new XmlDocument();
        xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", ""));
        xmlDoc.AppendChild(xmlDoc.CreateElement("Root"));

        IOHelper.CreateXml(filePath, xmlDoc);

        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public void CreateXml_WithXmlDoc_FileAlreadyExists_DoesNotOverwrite()
    {
        var filePath = Path.Combine(_tempDir, "existing.xml");
        File.WriteAllText(filePath, "<OldRoot/>");

        var xmlDoc = new XmlDocument();
        xmlDoc.AppendChild(xmlDoc.CreateElement("NewRoot"));

        IOHelper.CreateXml(filePath, xmlDoc);

        var content = File.ReadAllText(filePath);
        content.Should().Contain("OldRoot");
    }

    [Fact]
    public void CreateXml_WithXmlDoc_InvalidPath_ThrowsParameterException()
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.AppendChild(xmlDoc.CreateElement("Root"));

        Action act = () => IOHelper.CreateXml("", xmlDoc);

        act.Should().Throw<ParameterException>();
    }

    [Fact]
    public void CreateXml_WithRootNodeName_CreatesFileWithCorrectStructure()
    {
        var filePath = Path.Combine(_tempDir, "created.xml");

        var result = IOHelper.CreateXml(filePath, "MyRoot");

        File.Exists(filePath).Should().BeTrue();
        result.Should().NotBeNull();
        result.DocumentElement!.Name.Should().Be("MyRoot");
    }

    [Fact]
    public void CreateXml_WithRootNodeName_InvalidPath_ThrowsParameterException()
    {
        Action act = () => IOHelper.CreateXml("", "Root");

        act.Should().Throw<ParameterException>();
    }

    [Fact]
    public void WriteXml_WritesContentToFile()
    {
        var filePath = Path.Combine(_tempDir, "write.xml");
        var xmlDoc = new XmlDocument();
        xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", ""));
        xmlDoc.AppendChild(xmlDoc.CreateElement("WriteRoot"));

        IOHelper.WriteXml(filePath, xmlDoc);

        File.Exists(filePath).Should().BeTrue();
        var readDoc = new XmlDocument();
        readDoc.Load(filePath);
        readDoc.DocumentElement!.Name.Should().Be("WriteRoot");
    }

    [Fact]
    public void WriteXml_InvalidPath_ThrowsParameterException()
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.AppendChild(xmlDoc.CreateElement("Root"));

        Action act = () => IOHelper.WriteXml("", xmlDoc);

        act.Should().Throw<ParameterException>();
    }

    [Fact]
    public void ReadXml_ValidFile_ReturnsXmlDocument()
    {
        var filePath = Path.Combine(_tempDir, "read.xml");
        var xmlDoc = new XmlDocument();
        xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", ""));
        xmlDoc.AppendChild(xmlDoc.CreateElement("ReadRoot"));
        xmlDoc.Save(filePath);

        var result = IOHelper.ReadXml(filePath);

        result.Should().NotBeNull();
        result.DocumentElement!.Name.Should().Be("ReadRoot");
    }

    [Fact]
    public void ReadXml_FileDoesNotExist_ThrowsIOException()
    {
        var filePath = Path.Combine(_tempDir, "nonexistent.xml");

        Action act = () => IOHelper.ReadXml(filePath);

        act.Should().Throw<IOException>();
    }

    [Fact]
    public void ReadXml_InvalidPath_ThrowsParameterException()
    {
        Action act = () => IOHelper.ReadXml("");

        act.Should().Throw<ParameterException>();
    }

    #endregion
}
