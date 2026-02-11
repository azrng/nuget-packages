namespace Common.Core.Test.Extension;

/// <summary>
/// ByteExtensions字节数组扩展方法的单元测试
/// </summary>
public class ByteExtensionsTest
{
    #region ToHexString Tests

    /// <summary>
    /// 测试ToHexString方法：将字节数组转换为十六进制字符串
    /// </summary>
    [Fact]
    public void ToHexString_ValidBytes_ReturnsCorrectHexString()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0xFF,
                        0xAA,
                        0x00,
                        0x10
                    };

        // Act
        var result = bytes.ToHexString();

        // Assert
        Assert.Equal("FFAA0010", result);
    }

    /// <summary>
    /// 测试ToHexString方法：空字节数组应返回空字符串
    /// </summary>
    [Fact]
    public void ToHexString_EmptyBytes_ReturnsEmptyString()
    {
        // Arrange
        var bytes = Array.Empty<byte>();

        // Act
        var result = bytes.ToHexString();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// 测试ToHexString方法：单个字节的转换
    /// </summary>
    [Fact]
    public void ToHexString_SingleByte_ReturnsCorrectHexString()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0xAB
                    };

        // Act
        var result = bytes.ToHexString();

        // Assert
        Assert.Equal("AB", result);
    }

    #endregion

    #region ToBase64 Tests

    /// <summary>
    /// 测试ToBase64方法：将字节数组转换为Base64字符串
    /// </summary>
    [Fact]
    public void ToBase64_ValidBytes_ReturnsCorrectBase64()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0x01,
                        0x02,
                        0x03
                    };

        // Act
        var result = bytes.ToBase64();

        // Assert
        Assert.Equal("AQID", result);
    }

    /// <summary>
    /// 测试ToBase64方法：空字节数组的转换
    /// </summary>
    [Fact]
    public void ToBase64_EmptyBytes_ReturnsEmptyString()
    {
        // Arrange
        var bytes = Array.Empty<byte>();

        // Act
        var result = bytes.ToBase64();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region ToStream Tests

    /// <summary>
    /// 测试ToStream方法：将字节数组转换为流
    /// </summary>
    [Fact]
    public void ToStream_ValidBytes_ReturnsMemoryStream()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0x01,
                        0x02,
                        0x03,
                        0x04
                    };

        // Act
        var result = bytes.ToStream();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MemoryStream>(result);
        var memoryStream = (MemoryStream)result;
        Assert.Equal(bytes, memoryStream.ToArray());
    }

    /// <summary>
    /// 测试ToStream方法：空字节数组的转换
    /// </summary>
    [Fact]
    public void ToStream_EmptyBytes_ReturnsEmptyStream()
    {
        // Arrange
        var bytes = Array.Empty<byte>();

        // Act
        var result = bytes.ToStream();

        // Assert
        Assert.NotNull(result);
        var memoryStream = (MemoryStream)result;
        Assert.Empty(memoryStream.ToArray());
    }

    #endregion

    #region ToInt32 Tests

    /// <summary>
    /// 测试ToInt32方法：将4字节数组转换为int
    /// </summary>
    [Fact]
    public void ToInt32_ValidBytes_ReturnsCorrectInt()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0x01,
                        0x00,
                        0x00,
                        0x00
                    };

        // Act
        var result = bytes.ToInt32();

        // Assert
        Assert.Equal(1, result);
    }

    /// <summary>
    /// 测试ToInt32方法：字节数组长度小于4时应返回0
    /// </summary>
    [Fact]
    public void ToInt32_LessThan4Bytes_ReturnsZero()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0x01,
                        0x02,
                        0x03
                    };

        // Act
        var result = bytes.ToInt32();

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// 测试ToInt32方法：字节数组长度大于4时使用前4个字节
    /// </summary>
    [Fact]
    public void ToInt32_MoreThan4Bytes_UsesFirst4Bytes()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0x0A,
                        0x00,
                        0x00,
                        0x00,
                        0xFF,
                        0xFF
                    };

        // Act
        var result = bytes.ToInt32();

        // Assert
        Assert.Equal(10, result);
    }

    /// <summary>
    /// 测试ToInt32方法：空字节数组应返回0
    /// </summary>
    [Fact]
    public void ToInt32_EmptyBytes_ReturnsZero()
    {
        // Arrange
        var bytes = Array.Empty<byte>();

        // Act
        var result = bytes.ToInt32();

        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region GetFileSuffix Tests

    /// <summary>
    /// 测试GetFileSuffix方法：识别PNG文件格式
    /// </summary>
    [Fact]
    public void GetFileSuffix_PngBytes_ReturnsPngSuffix()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0x89,
                        0x50,
                        0x4E,
                        0x47,
                        0x0D,
                        0x0A,
                        0x1A,
                        0x0A
                    };

        // Act
        var result = bytes.GetFileSuffix();

        // Assert
        Assert.Equal(".png", result);
    }

    /// <summary>
    /// 测试GetFileSuffix方法：识别JPEG文件格式
    /// </summary>
    [Fact]
    public void GetFileSuffix_JpegBytes_ReturnsJpegSuffix()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0xFF,
                        0xD8,
                        0xFF
                    };

        // Act
        var result = bytes.GetFileSuffix();

        // Assert
        Assert.Equal(".jpg", result);
    }

    /// <summary>
    /// 测试GetFileSuffix方法：空字节数组应返回null
    /// </summary>
    [Fact]
    public void GetFileSuffix_EmptyBytes_ReturnsNull()
    {
        // Arrange
        var bytes = Array.Empty<byte>();

        // Act
        var result = bytes.GetFileSuffix();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetContentType Tests

    /// <summary>
    /// 测试GetContentType方法：PNG文件返回正确的MIME类型
    /// </summary>
    [Fact]
    public void GetContentType_PngBytes_ReturnsImagePng()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0x89,
                        0x50,
                        0x4E,
                        0x47,
                        0x0D,
                        0x0A,
                        0x1A,
                        0x0A
                    };

        // Act
        var result = bytes.GetContentType();

        // Assert
        Assert.Equal("image/png", result);
    }

    /// <summary>
    /// 测试GetContentType方法：JPEG文件返回正确的MIME类型
    /// </summary>
    [Fact]
    public void GetContentType_JpegBytes_ReturnsImageJpeg()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0xFF,
                        0xD8,
                        0xFF
                    };

        // Act
        var result = bytes.GetContentType();

        // Assert
        Assert.Equal("image/jpg", result);
    }

    #endregion

    #region GetRandomFileName Tests

    /// <summary>
    /// 测试GetRandomFileName方法：生成随机文件名
    /// </summary>
    [Fact]
    public void GetRandomFileName_PngBytes_ReturnsFileNameWithExtension()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0x89,
                        0x50,
                        0x4E,
                        0x47,
                        0x0D,
                        0x0A,
                        0x1A,
                        0x0A
                    };

        // Act
        var result = bytes.GetRandomFileName();

        // Assert
        Assert.NotNull(result);
        Assert.EndsWith(".png", result);
    }

    /// <summary>
    /// 测试GetRandomFileName方法：每次生成的文件名应不同
    /// </summary>
    [Fact]
    public void GetRandomFileName_SameBytes_GeneratesDifferentNames()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0x89,
                        0x50,
                        0x4E,
                        0x47
                    };

        // Act
        var result1 = bytes.GetRandomFileName();
        var result2 = bytes.GetRandomFileName();

        // Assert
        Assert.NotEqual(result1, result2);
    }

    #endregion

    #region ToFileByBytes Tests

    /// <summary>
    /// 测试ToFileByBytes方法：将字节数组保存为文件
    /// </summary>
    [Fact]
    public void ToFileByBytes_ValidData_CreatesFile()
    {
        // Arrange
        var bytes = new byte[]
                    {
                        0x01,
                        0x02,
                        0x03,
                        0x04
                    };
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            bytes.ToFileByBytes(tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var savedBytes = File.ReadAllBytes(tempFile);
            Assert.Equal(bytes, savedBytes);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    /// <summary>
    /// 测试ToFileByBytes方法：覆盖已存在的文件
    /// </summary>
    [Fact]
    public void ToFileByBytes_ExistingFile_OverwritesFile()
    {
        // Arrange
        var originalBytes = new byte[]
                            {
                                0x01,
                                0x02
                            };
        var newBytes = new byte[]
                       {
                           0x03,
                           0x04
                       };
        var tempFile = Path.GetTempFileName();

        try
        {
            File.WriteAllBytes(tempFile, originalBytes);

            // Act
            newBytes.ToFileByBytes(tempFile);

            // Assert
            var savedBytes = File.ReadAllBytes(tempFile);
            Assert.Equal(newBytes, savedBytes);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    #endregion
}