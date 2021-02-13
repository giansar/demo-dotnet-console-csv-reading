using System;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace demo_dotnet_console_csv_reading
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
            String connectionString = configuration.GetConnectionString("postgresql");

            CsvParserOptions parserOptions = new CsvParserOptions(false, ',');
            CsvReaderOptions csvReaderOptions = new CsvReaderOptions(new[] { Environment.NewLine });
            CsvMappingProvince mappingProvince = new CsvMappingProvince();
            CsvParser<Province> parser = new CsvParser<Province>(parserOptions, mappingProvince);

            CsvMappingResult<Province>[] result = parser.ReadFromFile(@args[0], Encoding.ASCII).ToArray();
            Console.WriteLine("Line count = " + result.Length);

            Task<string> task = InsertIntoTable(connectionString, result);
            Console.WriteLine(task.Result);
        }

        public static async Task<string> InsertIntoTable(String connectionString, CsvMappingResult<Province>[] result)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Postgresql version = " + connection.ServerVersion);
                    String sql = "INSERT INTO province (id, name) SELECT * FROM unnest(@i, @n) AS a";
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.Add(new NpgsqlParameter<int[]>("i", result.Select(e => e.Result.id).ToArray()));
                        command.Parameters.Add(new NpgsqlParameter<string[]>("n", result.Select(e => e.Result.name).ToArray()));
                        int i = await command.ExecuteNonQueryAsync();
                        return "Inserted rows = " + i;
                    }
                }
            }
            catch (System.Exception e)
            {
                return e.Message;
            }
        }
    }
}
