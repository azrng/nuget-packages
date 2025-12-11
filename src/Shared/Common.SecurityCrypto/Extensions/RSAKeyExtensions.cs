using Common.SecurityCrypto.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace Common.SecurityCrypto.Extensions
{
    public static class RSAKeyExtensions
    {
        internal static string ToJsonString(this RSA rsa, bool includePrivateParameters)
        {
            var rsaParameters = rsa.ExportParameters(includePrivateParameters);
            return JsonConvert.SerializeObject(new RSAParametersJson()
            {
                Modulus = rsaParameters.Modulus != null ? Convert.ToBase64String(rsaParameters.Modulus) : null,
                Exponent = rsaParameters.Exponent != null ? Convert.ToBase64String(rsaParameters.Exponent) : null,
                P = rsaParameters.P != null ? Convert.ToBase64String(rsaParameters.P) : null,
                Q = rsaParameters.Q != null ? Convert.ToBase64String(rsaParameters.Q) : null,
                DP = rsaParameters.DP != null ? Convert.ToBase64String(rsaParameters.DP) : null,
                DQ = rsaParameters.DQ != null ? Convert.ToBase64String(rsaParameters.DQ) : null,
                InverseQ = rsaParameters.InverseQ != null ? Convert.ToBase64String(rsaParameters.InverseQ) : null,
                D = rsaParameters.D != null ? Convert.ToBase64String(rsaParameters.D) : null
            });
        }

        internal static void FromJsonString(this RSA rsa, string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                throw new ArgumentNullException(nameof(jsonString));
            var parameters = new RSAParameters();
            try
            {
                var rsaParametersJson = JsonConvert.DeserializeObject<RSAParametersJson>(jsonString);
                parameters.Modulus = rsaParametersJson.Modulus != null ? Convert.FromBase64String(rsaParametersJson.Modulus) : null;
                parameters.Exponent = rsaParametersJson.Exponent != null ? Convert.FromBase64String(rsaParametersJson.Exponent) : null;
                parameters.P = rsaParametersJson.P != null ? Convert.FromBase64String(rsaParametersJson.P) : null;
                parameters.Q = rsaParametersJson.Q != null ? Convert.FromBase64String(rsaParametersJson.Q) : null;
                parameters.DP = rsaParametersJson.DP != null ? Convert.FromBase64String(rsaParametersJson.DP) : null;
                parameters.DQ = rsaParametersJson.DQ != null ? Convert.FromBase64String(rsaParametersJson.DQ) : null;
                parameters.InverseQ = rsaParametersJson.InverseQ != null ? Convert.FromBase64String(rsaParametersJson.InverseQ) : null;
                parameters.D = rsaParametersJson.D != null ? Convert.FromBase64String(rsaParametersJson.D) : null;
            }
            catch
            {
                throw new Exception("Invalid Json RSA key.");
            }
            rsa.ImportParameters(parameters);
        }

        public static void FromExtXmlString(this RSA rsa, string xmlString)
        {
            var parameters = new RSAParameters();
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlString);
            if (!xmlDocument.DocumentElement.Name.Equals("RSAKeyValue"))
                throw new Exception("Invalid Xml RSA Key");
            foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "D":
                        parameters.D = string.IsNullOrWhiteSpace(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;

                    case "DP":
                        parameters.DP = string.IsNullOrWhiteSpace(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;

                    case "DQ":
                        parameters.DQ = string.IsNullOrWhiteSpace(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;

                    case "Exponent":
                        parameters.Exponent = string.IsNullOrWhiteSpace(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;

                    case "InverseQ":
                        parameters.InverseQ = string.IsNullOrWhiteSpace(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;

                    case "Modulus":
                        parameters.Modulus = string.IsNullOrWhiteSpace(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;

                    case "P":
                        parameters.P = string.IsNullOrWhiteSpace(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;

                    case "Q":
                        parameters.Q = string.IsNullOrWhiteSpace(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;
                }
            }
            rsa.ImportParameters(parameters);
        }

        public static string ToExtXmlString(this RSA rsa, bool includePrivateParameters)
        {
            var rsaParameters = rsa.ExportParameters(includePrivateParameters);
            if (!includePrivateParameters)
                return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>", rsaParameters.Modulus != null ? Convert.ToBase64String(rsaParameters.Modulus) : (object)(string)null, rsaParameters.Exponent != null ? Convert.ToBase64String(rsaParameters.Exponent) : (object)(string)null);
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>", rsaParameters.Modulus != null ? Convert.ToBase64String(rsaParameters.Modulus) : (object)(string)null, rsaParameters.Exponent != null ? Convert.ToBase64String(rsaParameters.Exponent) : (object)(string)null, rsaParameters.P != null ? Convert.ToBase64String(rsaParameters.P) : (object)(string)null, rsaParameters.Q != null ? Convert.ToBase64String(rsaParameters.Q) : (object)(string)null, rsaParameters.DP != null ? Convert.ToBase64String(rsaParameters.DP) : (object)(string)null, rsaParameters.DQ != null ? Convert.ToBase64String(rsaParameters.DQ) : (object)(string)null, rsaParameters.InverseQ != null ? Convert.ToBase64String(rsaParameters.InverseQ) : (object)(string)null, rsaParameters.D != null ? Convert.ToBase64String(rsaParameters.D) : (object)(string)null);
        }

        public static void FromBase64StringByPrivateKey(this RSA rsa, string base64String)
        {
            var buffer = Convert.FromBase64String(base64String);
            var parameters = new RSAParameters();
            using (var binr = new BinaryReader(new MemoryStream(buffer)))
            {
                switch (binr.ReadUInt16())
                {
                    case 33072:
                        var num1 = (int)binr.ReadByte();
                        break;

                    case 33328:
                        var num2 = (int)binr.ReadInt16();
                        break;

                    default:
                        throw new Exception("Unexpected value read binr.ReadUInt16()");
                }
                if (binr.ReadUInt16() != 258)
                    throw new Exception("Unexpected version");
                parameters.Modulus = binr.ReadByte() <= 0 ? binr.ReadBytes(GetIntegerSize(binr)) : throw new Exception("Unexpected value read binr.ReadByte()");
                parameters.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                parameters.D = binr.ReadBytes(GetIntegerSize(binr));
                parameters.P = binr.ReadBytes(GetIntegerSize(binr));
                parameters.Q = binr.ReadBytes(GetIntegerSize(binr));
                parameters.DP = binr.ReadBytes(GetIntegerSize(binr));
                parameters.DQ = binr.ReadBytes(GetIntegerSize(binr));
                parameters.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }
            rsa.ImportParameters(parameters);
        }

        public static void FromBase64StringByPublicKey(this RSA rsa, string base64String)
        {
            var b = new byte[15]
            {
         48,
         13,
         6,
         9,
         42,
         134,
         72,
         134,
         247,
         13,
         1,
         1,
         1,
         5,
         0
            };
            var numArray1 = new byte[15];
            using (var input = new MemoryStream(Convert.FromBase64String(base64String)))
            {
                using (var binaryReader = new BinaryReader(input))
                {
                    switch (binaryReader.ReadUInt16())
                    {
                        case 33072:
                            var num1 = (int)binaryReader.ReadByte();
                            break;

                        case 33328:
                            var num2 = (int)binaryReader.ReadInt16();
                            break;

                        default:
                            return;
                    }
                    if (!CompareBytearrays(binaryReader.ReadBytes(15), b))
                        return;
                    switch (binaryReader.ReadUInt16())
                    {
                        case 33027:
                            var num3 = (int)binaryReader.ReadByte();
                            break;

                        case 33283:
                            var num4 = (int)binaryReader.ReadInt16();
                            break;

                        default:
                            return;
                    }
                    if (binaryReader.ReadByte() > 0)
                        return;
                    switch (binaryReader.ReadUInt16())
                    {
                        case 33072:
                            var num5 = (int)binaryReader.ReadByte();
                            break;

                        case 33328:
                            var num6 = (int)binaryReader.ReadInt16();
                            break;

                        default:
                            return;
                    }
                    var num7 = binaryReader.ReadUInt16();
                    byte num8 = 0;
                    byte num9;
                    switch (num7)
                    {
                        case 33026:
                            num9 = binaryReader.ReadByte();
                            break;

                        case 33282:
                            num8 = binaryReader.ReadByte();
                            num9 = binaryReader.ReadByte();
                            break;

                        default:
                            return;
                    }
                    var int32 = BitConverter.ToInt32(new byte[4]
                    {
            num9,
            num8,
             0,
             0
                    }, 0);
                    if (binaryReader.PeekChar() == 0)
                    {
                        var num10 = (int)binaryReader.ReadByte();
                        --int32;
                    }
                    var numArray2 = binaryReader.ReadBytes(int32);
                    if (binaryReader.ReadByte() != 2)
                        return;
                    var count = (int)binaryReader.ReadByte();
                    var numArray3 = binaryReader.ReadBytes(count);
                    var parameters = new RSAParameters()
                    {
                        Modulus = numArray2,
                        Exponent = numArray3
                    };
                    rsa.ImportParameters(parameters);
                }
            }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            if (binr.ReadByte() != 2)
                return 0;
            var num1 = binr.ReadByte();
            int integerSize;
            switch (num1)
            {
                case 129:
                    integerSize = binr.ReadByte();
                    break;

                case 130:
                    var num2 = binr.ReadByte();
                    integerSize = BitConverter.ToInt32(new byte[4]
                    {
            binr.ReadByte(),
            num2,
             0,
             0
                    }, 0);
                    break;

                default:
                    integerSize = num1;
                    break;
            }
            while (binr.ReadByte() == 0)
                --integerSize;
            binr.BaseStream.Seek(-1L, SeekOrigin.Current);
            return integerSize;
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            var index = 0;
            foreach (int num in a)
            {
                if (num != b[index])
                    return false;
                ++index;
            }
            return true;
        }
    }
}