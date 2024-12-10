using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ORDERINGSYSTEM
{
    public static class DatabaseHelper
{
    public static string ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=KimchiDB;Trusted_Connection=True;";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}

}
