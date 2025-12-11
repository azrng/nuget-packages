using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CommonCollect
{
    /*
    示例：计算图片hash
     var imageHasher = new ImageHasher(new ImageSharpTransformer());
    imageHasher.DifferenceHash256(s)

    //计算相似度
        var hasher = new ImageHasher(new ImageSharpTransformer());
            var sw = Stopwatch.StartNew();
            var hash = hasher.DifferenceHash256(txtPic.Text);
            var  匹配度 = ImageHasher.Compare(x.Value, hash)
     */

    /// <summary>
    /// 图片相似度公共类
    /// 理论介绍：https://segmentfault.com/a/1190000038308093
    /// </summary>
    public class ImageHasher
    {
        private readonly IImageTransformer _transformer;
        private float[][] _dctMatrix;
        private bool _isDctMatrixInitialized;
        private readonly object _dctMatrixLockObject = new object();
        private const ulong M1 = 6148914691236517205;
        private const ulong M2 = 3689348814741910323;
        private const ulong M4 = 1085102592571150095;
        private const ulong H01 = 72340172838076673;

        public ImageHasher() => this._transformer = (IImageTransformer)new ImageSharpTransformer();

        public ImageHasher(IImageTransformer transformer) => this._transformer = transformer;

        public ulong AverageHash64(string pathToImage)
        {
            using (FileStream sourceStream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read))
                return this.AverageHash64((Stream)sourceStream);
        }

        public ulong AverageHash64(Stream sourceStream)
        {
            byte[] source = this._transformer.TransformImage(sourceStream, 8, 8);
            int num1 = ((IEnumerable<byte>)source).Sum<byte>((Func<byte, int>)(b => (int)b)) / 64;
            ulong num2 = 0;
            for (int index = 0; index < 64; ++index)
            {
                if ((int)source[index] > num1)
                    num2 |= (ulong)(1L << index);
            }
            return num2;
        }

        public ulong MedianHash64(string pathToImage)
        {
            using (FileStream sourceStream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read))
                return this.MedianHash64((Stream)sourceStream);
        }

        public ulong MedianHash64(Stream sourceStream)
        {
            byte[] collection = this._transformer.TransformImage(sourceStream, 8, 8);
            List<byte> byteList = new List<byte>((IEnumerable<byte>)collection);
            byteList.Sort();
            byte num1 = (byte)(((int)byteList[31] + (int)byteList[32]) / 2);
            ulong num2 = 0;
            for (int index = 0; index < 64; ++index)
            {
                if ((int)collection[index] > (int)num1)
                    num2 |= (ulong)(1L << index);
            }
            return num2;
        }

        public ulong[] MedianHash256(string pathToImage)
        {
            using (FileStream sourceStream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read))
                return this.MedianHash256((Stream)sourceStream);
        }

        public ulong[] MedianHash256(Stream sourceStream)
        {
            byte[] collection = this._transformer.TransformImage(sourceStream, 16, 16);
            List<byte> byteList = new List<byte>((IEnumerable<byte>)collection);
            byteList.Sort();
            byte num1 = (byte)(((int)byteList[(int)sbyte.MaxValue] + (int)byteList[128]) / 2);
            ulong num2 = 0;
            ulong[] numArray = new ulong[4];
            for (int index1 = 0; index1 < 4; ++index1)
            {
                for (int index2 = 0; index2 < 64; ++index2)
                {
                    if ((int)collection[64 * index1 + index2] > (int)num1)
                        num2 |= (ulong)(1L << index2);
                }
                numArray[index1] = num2;
                num2 = 0UL;
            }
            return numArray;
        }

        /// <summary>
        /// 计算指定地址图片的hash64
        /// </summary>
        /// <param name="pathToImage"></param>
        /// <returns></returns>
        public ulong DifferenceHash64(string pathToImage)
        {
            using (FileStream sourceStream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read))
                return this.DifferenceHash64((Stream)sourceStream);
        }

        public ulong DifferenceHash64(Stream sourceStream)
        {
            byte[] numArray = this._transformer.TransformImage(sourceStream, 9, 8);
            ulong num1 = 0;
            int num2 = 0;
            for (int index1 = 0; index1 < 8; ++index1)
            {
                int num3 = index1 * 9;
                for (int index2 = 0; index2 < 8; ++index2)
                {
                    if ((int)numArray[num3 + index2] > (int)numArray[num3 + index2 + 1])
                        num1 |= (ulong)(1L << num2);
                    ++num2;
                }
            }
            return num1;
        }

        public ulong[] DifferenceHash256(string pathToImage)
        {
            using (FileStream sourceStream = new FileStream(pathToImage, FileMode.Open, FileAccess.Read))
                return this.DifferenceHash256((Stream)sourceStream);
        }

        public ulong[] DifferenceHash256(Stream sourceStream)
        {
            byte[] numArray1 = this._transformer.TransformImage(sourceStream, 17, 16);
            ulong[] numArray2 = new ulong[4];
            int num1 = 0;
            int index1 = 0;
            for (int index2 = 0; index2 < 16; ++index2)
            {
                int num2 = index2 * 17;
                for (int index3 = 0; index3 < 16; ++index3)
                {
                    if ((int)numArray1[num2 + index3] > (int)numArray1[num2 + index3 + 1])
                        numArray2[index1] |= (ulong)(1L << num1);
                    if (num1 == 63)
                    {
                        num1 = 0;
                        ++index1;
                    }
                    else
                        ++num1;
                }
            }
            return numArray2;
        }

        public ulong DctHash(Stream sourceStream)
        {
            object matrixLockObject = this._dctMatrixLockObject;
            bool lockTaken = false;
            try
            {
                Monitor.Enter(matrixLockObject, ref lockTaken);
                if (!this._isDctMatrixInitialized)
                {
                    this._dctMatrix = ImageHasher.GenerateDctMatrix(32);
                    this._isDctMatrixInitialized = true;
                }
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(matrixLockObject);
            }
            byte[] numArray = this._transformer.TransformImage(sourceStream, 32, 32);
            float[] image = new float[1024];
            for (int index = 0; index < 1024; ++index)
                image[index] = (float)numArray[index] / (float)byte.MaxValue;
            float[][] dct = ImageHasher.ComputeDct(image, this._dctMatrix);
            float[] collection = new float[64];
            for (int index1 = 0; index1 < 8; ++index1)
            {
                for (int index2 = 0; index2 < 8; ++index2)
                    collection[index1 + index2 * 8] = dct[index1 + 1][index2 + 1];
            }
            List<float> floatList = new List<float>((IEnumerable<float>)collection);
            floatList.Sort();
            float num1 = (float)(((double)floatList[31] + (double)floatList[32]) / 2.0);
            ulong num2 = 0;
            for (int index = 0; index < 64; ++index)
            {
                if ((double)collection[index] > (double)num1)
                    num2 |= (ulong)(1L << index);
            }
            return num2;
        }

        public ulong DctHash(string path)
        {
            using (FileStream sourceStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                return this.DctHash((Stream)sourceStream);
        }

        private static float[][] ComputeDct(float[] image, float[][] dctMatrix)
        {
            int length = dctMatrix.GetLength(0);
            float[][] b = new float[length][];
            for (int index = 0; index < length; ++index)
                b[index] = new float[length];
            for (int index1 = 0; index1 < length; ++index1)
            {
                for (int index2 = 0; index2 < length; ++index2)
                    b[index1][index2] = image[index2 + index1 * length];
            }
            return ImageHasher.Multiply(ImageHasher.Multiply(dctMatrix, b), ImageHasher.Transpose(dctMatrix));
        }

        private static float[][] GenerateDctMatrix(int size)
        {
            float[][] dctMatrix = new float[size][];
            for (int index = 0; index < size; ++index)
                dctMatrix[index] = new float[size];
            double num = Math.Sqrt(2.0 / (double)size);
            for (int index = 0; index < size; ++index)
                dctMatrix[0][index] = (float)Math.Sqrt(1.0 / (double)size);
            for (int index1 = 0; index1 < size; ++index1)
            {
                for (int index2 = 1; index2 < size; ++index2)
                    dctMatrix[index2][index1] = (float)(num * Math.Cos((double)((2 * index1 + 1) * index2) * Math.PI / (2.0 * (double)size)));
            }
            return dctMatrix;
        }

        private static float[][] Multiply(float[][] a, float[][] b)
        {
            int length = a[0].Length;
            float[][] numArray = new float[length][];
            for (int index = 0; index < length; ++index)
                numArray[index] = new float[length];
            for (int index1 = 0; index1 < length; ++index1)
            {
                for (int index2 = 0; index2 < length; ++index2)
                {
                    for (int index3 = 0; index3 < length; ++index3)
                        numArray[index1][index3] += a[index1][index2] * b[index2][index3];
                }
            }
            return numArray;
        }

        private static float[][] Transpose(float[][] mat)
        {
            int length = mat[0].Length;
            float[][] numArray = new float[length][];
            for (int index1 = 0; index1 < length; ++index1)
            {
                numArray[index1] = new float[length];
                for (int index2 = 0; index2 < length; ++index2)
                    numArray[index1][index2] = mat[index2][index1];
            }
            return numArray;
        }

        public static float Compare(ulong hash1, ulong hash2) => (float)(1.0 - (double)ImageHasher.HammingWeight(hash1 ^ hash2) / 64.0);

        /// <summary>
        /// 获取两个图片相似度
        /// </summary>
        /// <param name="hash1"></param>
        /// <param name="hash2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float Compare(ulong[] hash1, ulong[] hash2)
        {
            if (hash1.Length != hash2.Length)
                throw new ArgumentException("hash1 与 hash2长度不匹配");
            int length = hash1.Length;
            ulong num = 0;
            ulong[] numArray = new ulong[length];
            for (int index = 0; index < length; ++index)
                numArray[index] = hash1[index] ^ hash2[index];
            for (int index = 0; index < length; ++index)
                num += ImageHasher.HammingWeight(numArray[index]);
            return (float)(1.0 - (double)num / ((double)length * 64.0));
        }

        private static ulong HammingWeight(ulong hash)
        {
            hash -= hash >> 1 & 6148914691236517205UL;
            hash = (ulong)(((long)hash & 3689348814741910323L) + ((long)(hash >> 2) & 3689348814741910323L));
            hash = (ulong)((long)hash + (long)(hash >> 4) & 1085102592571150095L);
            return hash * 72340172838076673UL >> 56;
        }
    }

    public interface IImageTransformer
    {
        byte[] TransformImage(Stream stream, int width, int height);
    }

    public class ImageSharpTransformer : IImageTransformer
    {
        public byte[] TransformImage(Stream stream, int width, int height)
        {
            using Image<Rgba32> source = Image.Load<Rgba32>(stream);
            source.Mutate<Rgba32>((Action<IImageProcessingContext>)(x => x.Resize(new ResizeOptions()
            {
                Size = new Size()
                {
                    Width = width,
                    Height = height
                },
                Mode = ResizeMode.Stretch,
                Sampler = (IResampler)new BicubicResampler()
            }).Grayscale()));
            Memory<Rgba32> memory;
            source.DangerousTryGetSinglePixelMemory(out memory);
            Rgba32[] array = memory.ToArray();
            int length = width * height;
            byte[] numArray = new byte[length];
            for (int index = 0; index < length; ++index)
                numArray[index] = array[index].B;
            return numArray;
        }
    }
}