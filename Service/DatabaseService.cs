using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks;

namespace Service
{
    public class DatabaseService
    {
        private static string ReadConnectionString()

        {

            var cfg = new ConfigurationBuilder()

                .SetBasePath(AppContext.BaseDirectory)

                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)

                .Build();



            return cfg.GetConnectionString("DBLibraryManagement");

        }
        //private readonly string connectionString = "Server=localhost;Database=DBLibraryManagement;Trusted_Connection=True;";
        private readonly string connectionString = ReadConnectionString();


        public async Task<DataTable> ExecuteQueryAsync(string sqlQuery)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sqlQuery, connection);
            using var reader = await command.ExecuteReaderAsync();

            var dataTable = new DataTable();
            dataTable.Load(reader);

            return dataTable;
        }
    }
}
