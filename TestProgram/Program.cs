using DataAccsess;

namespace TestProgram
{
    public class Program
    {
        static void Main(string[] args)
        {
            using var db = new DBLibraryManagementContext();
            Console.WriteLine(db.Students.ToList()[0].FullName);
        }
    }
}
