namespace Azrng.NTika.Parsers.Csv
{
    public class CsvConfig
    {
        public char Delimiter { get; set; } = ',';
        public bool HasHeaders { get; set; } = true;

        public CsvConfig Clone()
        {
            return new CsvConfig
            {
                Delimiter = Delimiter,
                HasHeaders = HasHeaders
            };
        }
    }
}
