using System.Text;

namespace Azrng.NTika.Parsers.Pdf.Test
{
    internal static class CreateTestPdf
    {
        public static byte[] CreateMinimalPdf(string text = "Hello PDF World")
        {
            var sb = new StringBuilder();

            // PDF Header
            sb.Append("%PDF-1.4\n");

            // Object 1: Catalog
            var obj1Offset = sb.Length;
            sb.Append("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

            // Object 2: Pages
            var obj2Offset = sb.Length;
            sb.Append("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

            // Object 3: Page
            var obj3Offset = sb.Length;
            sb.Append("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj\n");

            // Object 4: Content stream
            var content = $"BT\n/F1 12 Tf\n100 700 Td\n({text}) Tj\nET";
            var obj4Offset = sb.Length;
            sb.Append($"4 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n");
            sb.Append(content);
            sb.Append("\nendstream\nendobj\n");

            // Object 5: Font
            var obj5Offset = sb.Length;
            sb.Append("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");

            // Cross-reference table
            var xrefOffset = sb.Length;
            sb.Append("xref\n");
            sb.Append("0 6\n");
            sb.Append("0000000000 65535 f \n");
            sb.Append($"{obj1Offset:D10} 00000 n \n");
            sb.Append($"{obj2Offset:D10} 00000 n \n");
            sb.Append($"{obj3Offset:D10} 00000 n \n");
            sb.Append($"{obj4Offset:D10} 00000 n \n");
            sb.Append($"{obj5Offset:D10} 00000 n \n");

            // Trailer
            sb.Append("trailer\n<< /Size 6 /Root 1 0 R >>\n");
            sb.Append($"startxref\n{xrefOffset}\n");
            sb.Append("%%EOF");

            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }
}
