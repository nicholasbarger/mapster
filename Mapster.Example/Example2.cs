using Mapster.Example.DomainModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapster.Example
{
    /// <summary>
    /// This set of simple examples shows basic ADO.net connections using inline sql to a Sql database.
    /// YOU MUST HAVE A VALID CONNECTIONSTRING AND LOCAL SQL EXPRESS DB TO TRY THIS.
    /// </summary>
    public class Example2
    {
        public Order Get(int id)
        {
            // Old-fashioned (but trusty and simple) ADO.NET
            var connectionString = GetConnectionString();
            var connection = new SqlConnection(connectionString);
            var sql = "SELECT * FROM Orders WHERE OrderId = @id";  // this is inline sql, but could also be stored procedure or dynamic

            var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();

            // Get data and fill datatable
            da.Fill(dt);

            // Map to object - this is the only pertinent part of the example
            // **************************************************************
            var order = Map<Order>.MapSingle(dt);
            // **************************************************************

            return order;
        }

        public Order GetEverything(int id)
        {
            // Old-fashioned (but trusty and simple) ADO.NET
            var connectionString = GetConnectionString();
            var connection = new SqlConnection(connectionString);

            // when returning multiple queries, make the first query the primary mapping and other queries secondary child data for collections
            var sql = "SELECT * FROM Orders WHERE OrderId = @id;";  // this is inline sql, but could also be stored procedure or dynamic
            sql += "SELECT * FROM OrderLineItems WHERE OrderId = @id";  // additional query for child details (line items)

            var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            var da = new SqlDataAdapter(cmd);
            var ds = new DataSet();

            // Get data and fill datatable
            da.Fill(ds);

            // Name tables
            ds.Tables[0].TableName = "Orders";  // matches our model or could use dbtable attribute to specify
            ds.Tables[1].TableName = "OrderLineItems"; // matches a collection property on our model or could use dbtable attribute to specify

            // Map to object - this is the only pertinent part of the example
            // **************************************************************
            var orders = Map<Order>.MapSingle(ds);
            // **************************************************************

            return orders;
        }

        public List<Order> Search(string orderNumber)
        {
            // Old-fashioned (but trusty and simple) ADO.NET
            var connectionString = GetConnectionString();
            var connection = new SqlConnection(connectionString);
            var sql = "SELECT * FROM Orders WHERE OrderNumber LIKE @orderNumber";  // this is inline sql, but could also be stored procedure or dynamic

            var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@orderNumber", '%' + orderNumber + '%');

            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();

            // Get data and fill datatable
            da.Fill(dt);

            // Map to object - this is the only pertinent part of the example
            // **************************************************************
            var order = Map<Order>.MapCollection(dt);
            // **************************************************************

            return order;
        }

        /// <summary>
        /// This is just a helper method for when running through unit tests, normally I wouldn't do this but I didn't
        /// want anyone to have to set up a database just to run the examples and tests.
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {
            return Mapster.Example.Properties.Settings.Default.LocalDataConnectionString;
        }
    }
}
