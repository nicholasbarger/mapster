using Mapster.Example.DomainModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapster.Example
{
    /// <summary>
    /// This set of simple examples shows basic ADO.net connections using inline sql to a Sql Compact database.
    /// </summary>
    public class Example1
    {
        public Order Get(int id)
        {
            // Old-fashioned (but trusty and simple) ADO.NET
            var connectionString = GetConnectionString();
            var connection = new SqlCeConnection(connectionString);
            var sql = "SELECT * FROM Orders WHERE OrderId = @id";  // this is inline sql, but could also be stored procedure or dynamic

            var cmd = new SqlCeCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            var da = new SqlCeDataAdapter(cmd);
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
            var connection = new SqlCeConnection(connectionString);

            // setup dataset
            var ds = new DataSet();
            ds.Tables.Add("Orders");  // matches our model or could use dbtable attribute to specify
            ds.Tables.Add("OrderLineItems");  // matches a collection property on our model or could use dbtable attribute to specify

            // because sql compact does not support multi-select queries in a single call we need to do them one at a time
            var sql = "SELECT * FROM Orders WHERE OrderId = @id;";  // this is inline sql, but could also be stored procedure or dynamic
            var cmd = new SqlCeCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            var da = new SqlCeDataAdapter(cmd);
            da.Fill(ds.Tables["Orders"]);

            // make second sql call for child line items
            sql = "SELECT * FROM OrderLineItems WHERE OrderId = @id";  // additional query for child details (line items)
            cmd = new SqlCeCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            da = new SqlCeDataAdapter(cmd);
            da.Fill(ds.Tables["OrderLineItems"]);

            // Map to object - this is the only pertinent part of the example
            // **************************************************************
            var order = Map<Order>.MapSingle(ds);
            // **************************************************************

            return order;
        }

        public List<Order> Search(string orderNumber)
        {
            // Old-fashioned (but trusty and simple) ADO.NET
            var connectionString = GetConnectionString();
            var connection = new SqlCeConnection(connectionString);
            var sql = "SELECT * FROM Orders WHERE OrderNumber LIKE @orderNumber";  // this is inline sql, but could also be stored procedure or dynamic

            var cmd = new SqlCeCommand(sql, connection);
            cmd.Parameters.AddWithValue("@orderNumber", '%' + orderNumber + '%');

            var da = new SqlCeDataAdapter(cmd);
            var dt = new DataTable();

            // Get data and fill datatable
            da.Fill(dt);

            // Map to object - this is the only pertinent part of the example
            // **************************************************************
            var orders = Map<Order>.MapCollection(dt);
            // **************************************************************

            return orders;
        }

        /// <summary>
        /// This is just a helper method for when running through unit tests, normally I wouldn't do this but I didn't
        /// want anyone to have to set up a database just to run the examples and tests.
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {
            return Mapster.Example.Properties.Settings.Default.ExampleDataConnectionString;
        }
    }
}
