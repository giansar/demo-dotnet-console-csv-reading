using TinyCsvParser.Mapping;

namespace demo_dotnet_console_csv_reading
{
    public class CsvMappingProvince : CsvMapping<Province>
    {
        public CsvMappingProvince() : base()
        {
            MapProperty(0, x => x.id);
            MapProperty(1, x => x.name);
        }
    }
}