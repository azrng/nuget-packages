namespace Common.Security.Test
{
    public class ShaHashTest
    {
        [Fact]
        public void Get_Sha1Hash_ReturnOk()
        {
            var str = "123456";
            var value = "7c4a8d09ca3762af61e59520943dc26494f8941b";
            Assert.True(ShaHelper.GetSha1Hash(str) == value);
        }

        [Fact]
        public void Get_HmacSha1Hash_ReturnOk()
        {
            var str = "987654321";
            var secret = "123456";
            var value = "145b9726076579a02b61b0085397100f9594f398";

            var result = ShaHelper.GetHmacSha1Hash(str, secret);
            Assert.True(result == value);
        }

        [Fact]
        public void Get_Sha256Hash_ReturnOk()
        {
            var str = "123456";
            var value = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92";
            Assert.True(ShaHelper.GetSha256Hash(str) == value);
        }

        [Fact]
        public void Get_HmacSha256Hash_ReturnOk()
        {
            var str = "987654321";
            var secret = "123456";
            var value = "e366e9660a2da262ef72049c830a029495ce8eeba5be544ba6d3328397958267";

            var result = ShaHelper.GetHmacSha256Hash(str, secret);
            Assert.True(result == value);
        }

        [Fact]
        public void Get_Sha512Hash_ReturnOk()
        {
            var str = "123456";
            var value =
                "ba3253876aed6bc22d4a6ff53d8406c6ad864195ed144ab5c87621b6c233b548baeae6956df346ec8c17f5ea10f35ee3cbc514797ed7ddd3145464e2a0bab413";
            Assert.True(ShaHelper.GetSha512Hash(str) == value);
        }

        [Fact]
        public void Get_HmacSha512Hash_ReturnOk()
        {
            var str = "987654321";
            var secret = "123456";
            var value =
                "5447ef22593e971cc3b7b1e33be8b63a5c47c1f3ceff94ea2d0a56e46894cbc46af30df2fd6ccae34dca1ab5ca24e0fba0247c452b21b237c40fc347d000537b";

            var result = ShaHelper.GetHmacSha512Hash(str, secret);
            Assert.True(result == value);
        }
    }
}