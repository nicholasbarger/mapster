using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mapster
{
    #region Core Mapster Functionality

    /// <summary>
    /// A simple mapper that uses reflection to get data from database fields and map them to a model.
    /// </summary>
    /// <typeparam name="T">A generic that supports a new constructor.</typeparam>
    public class Map<T> where T : new()
    {
        #region Public mapping actions

        /// <summary>
        /// Map a collection of models from a datatable with one or many rows.
        /// </summary>
        /// <param name="dt">The data from the database.</param>
        /// <returns>A collection of specified type.</returns>
        public static List<T> MapCollection(DataTable dt)
        {
            // check that dt is not null, else return without mapping
            if (dt == null || dt.Rows.Count == 0)
            {
                return default(List<T>);
            }

            // create collection
            List<T> result = new List<T>();

            // loop through datatable rows
            foreach (DataRow row in dt.Rows)
            {
                result.Add(MapSingle(row));
            }

            // return collection
            return result;
        }

        /// <summary>
        /// Map a single model from a dataset with multiple child datatables.
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static T MapSingle(DataSet ds)
        {
            // check that the ds is not null, else return without mapping
            if (ds == null || ds.Tables.Count == 0)
            {
                return default(T);
            }

            // get first datatable as primary
            var primaryDatatable = ds.Tables[0];

            // map collection to primary table
            T result = MapSingle(primaryDatatable);

            // get the list of properties which are collections
            var collectionProperties = GetCollectionFieldProperties<T>();

            // loop through properties which are collections to check if data is provided for them through a matching dt
            foreach (var propertyItem in collectionProperties)
            {
                // note: these are named a little funny because of the tuple, should probably make a small model for it
                var property = propertyItem.Item1;
                var dbTableName = propertyItem.Item2;

                // check if a matching dt exists based on dbTableName
                if (ds.Tables.Contains(dbTableName))
                {
                    // map table to collection (i'm using reflection here because I don't know what the propertyType
                    // will be until runtime when we loop through the object and can't call the generic in MapCollection
                    var itemTypeInCollection = property.PropertyType.GetGenericArguments()[0];
                    var classInfo = typeof(Map<>).MakeGenericType(new[] { itemTypeInCollection });
                    var methodInfo = classInfo.GetMethod("MapCollection", new[] { typeof(DataTable) });

                    var instanceOfClass = Activator.CreateInstance(classInfo);
                    var childCollection = methodInfo.Invoke(instanceOfClass, new[] { ds.Tables[dbTableName] });

                    // set childCollection mapped models to property value
                    property.SetValue(result, childCollection);
                }
            }

            // return collection
            return result;
        }

        /// <summary>
        /// Map a single model from a datatable with exactly one row.
        /// </summary>
        /// <param name="dt">The database data.</param>
        /// <returns>A specified model.</returns>
        public static T MapSingle(DataTable dt)
        {
            // check that dt is not null, else return without mapping
            if (dt == null || dt.Rows.Count == 0)
            {
                return default(T);
            }

            // check that dt has a single row, else throw error
            if (dt.Rows.Count > 1)
            {
                throw new ArgumentException("Too many rows were passed to map a single element.");
            }

            // get single row to simplify work
            var row = dt.Rows[0];

            // single row mapping
            return MapSingle(row);
        }

        /// <summary>
        /// Map a single model to a datatable row.
        /// </summary>
        /// <param name="row">The database data.</param>
        /// <returns>A single model of specified type.</returns>
        public static T MapSingle(DataRow row)
        {
            // check that row is not null
            if (row == null)
            {
                return default(T);
            }

            // setup new instance
            T result = new T();
            
            // map
            MapUsingReflection(row, result);

            return result;
        }

        #endregion

        #region Helper methods which can also be used for inspecting models

        /// <summary>
        /// Get the property fields which are collections using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Tuple<PropertyInfo, string>> GetCollectionFieldProperties<T>()
        {
            var result = new List<Tuple<PropertyInfo, string>>();

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                // check if the property is a collection
                bool implementICollection = property.PropertyType.GetInterfaces()
                            .Any(x => x.IsGenericType &&
                            x.GetGenericTypeDefinition() == typeof(ICollection<>));

                // get table data for collection
                if (implementICollection)
                {
                    // get the db table name
                    var dbfield = GetDbTableNameFromProperty(property);

                    // add to result collection
                    result.Add(property, dbfield);
                }
            }

            return result;
        }

        /// <summary>
        /// Get the property fields using reflection.
        /// </summary>
        /// <typeparam name="T">The specified model to iterate over.</typeparam>
        /// <returns>A list of property fields and database field names.</returns>
        public static List<Tuple<PropertyInfo, string>> GetDataFieldProperties<T>()
        {
            var result = new List<Tuple<PropertyInfo, string>>();

            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                // check if marked NoDb, skip if found
                var nodbAttribute = property.GetCustomAttribute(typeof(NoDbAttribute)) as NoDbAttribute;
                if (nodbAttribute != null)
                {
                    break;
                }

                // check if marked DbTable, skip if found
                var dbTableAttribute = property.GetCustomAttribute(typeof(DbTableAttribute)) as DbTableAttribute;
                if (dbTableAttribute != null)
                {
                    break;
                }

                // get the db field name
                var dbfield = GetDbFieldNameFromProperty(property);

                // add to collection
                result.Add(property, dbfield);
            }

            return result;
        }

        /// <summary>
        /// Gets the list of database field names from a model.
        /// </summary>
        /// <typeparam name="T">The specified model to iterate over.</typeparam>
        /// <returns>A list of database field names.</returns>
        public static List<string> GetDatabaseFieldNames<T>()
        {
            var result = new List<string>();
            var properties = GetDataFieldProperties<T>();
            foreach (var property in properties)
            {
                result.Add(property.Item2);
            }
            return result;
        }

        #endregion

        #region Private methods 

        /// <summary>
        /// Gets the field name for use with database.
        /// </summary>
        /// <param name="property">The property to inspect.</param>
        /// <returns>The database field name retrieved.</returns>
        private static string GetDbFieldNameFromProperty(PropertyInfo property)
        {
            // get field name based on property name
            var dbfield = property.Name;

            // check if it contains an attribute called "DbField"
            var overrideAttribute = property.GetCustomAttribute(typeof(DbFieldAttribute)) as DbFieldAttribute;
            if (overrideAttribute != null)
            {
                dbfield = overrideAttribute.FieldName;
            }
            return dbfield;
        }

        /// <summary>
        /// Gets the table name for use with database.
        /// </summary>
        /// <param name="property">The property to inspect.</param>
        /// <returns>The database table name retrieved.</returns>
        private static string GetDbTableNameFromProperty(PropertyInfo property)
        {
            // get table name based on property name
            var dbTableName = property.Name;

            // check if it contains an attribute called "DbField"
            var overrideAttribute = property.GetCustomAttribute(typeof(DbTableAttribute)) as DbTableAttribute;
            if (overrideAttribute != null)
            {
                dbTableName = overrideAttribute.TableName;
            }
            return dbTableName;
        }

        /// <summary>
        /// Loop through the properties and attempt to retrieve data from the specified data row.
        /// </summary>
        /// <param name="row">The datatable row holding the data.</param>
        /// <param name="propertyInstance">The instance of the property.</param>
        private static void MapUsingReflection(DataRow row, T propertyInstance)
        {
            var properties = GetDataFieldProperties<T>();
            foreach (var property in properties)
            {
                // retrieve field from datatable
                SetPropertyValueFromDatabase(row, propertyInstance, property.Item1, property.Item2);
            }            
        }

        /// <summary>
        /// Assign value to the property from the database.
        /// </summary>
        /// <param name="row">The datatable row holding the data.</param>
        /// <param name="propertyInstance">The instance of the property.</param>
        /// <param name="property">The property information.</param>
        /// <param name="dbfield">The fieldname for the database field we're interested in.</param>
        private static void SetPropertyValueFromDatabase(DataRow row, T propertyInstance, PropertyInfo property, string dbfield)
        {
            // check the value is not empty
            if (row[dbfield] != DBNull.Value)
            {
                // attempt to convert based on the datatype
                var value = Convert.ChangeType(row[dbfield], property.PropertyType);                

                // set value
                property.SetValue(propertyInstance, value, null);
            }
        }

        #endregion
    }

    #endregion

    #region Custom Attributes

    /// <summary>
    /// Specifies the database field name when not using convention naming on a model.
    /// </summary>
    public class DbFieldAttribute : Attribute
    {
        public DbFieldAttribute(string fieldName)
        {
            this.FieldName = fieldName;
        }

        public string FieldName { get; set; }
    }

    /// <summary>
    /// Specifies the database table name on a collection when deep retrieval and not using convention naming on a model.
    /// </summary>
    public class DbTableAttribute : Attribute
    {
        public DbTableAttribute(string tableName)
        {
            this.TableName = tableName;
        }

        public string TableName { get; set; }
    }

    /// <summary>
    /// Specifies that the field should be skipped when attempting to bind it to a database field.
    /// </summary>
    public class NoDbAttribute : Attribute
    {
    }

    #endregion  

    #region Internal Helper Extension Methods

    /// <summary>
    /// Some helpful extension methods to shorthand Tuple tasks.
    /// </summary>
    public static class TupleExtensions
    {
        public static bool ContainsKey(this List<Tuple<string, string>> collection, string key)
        {
            if (collection != null)
            {
                return collection.Any(a => a.Item1 == key);
            }
            else
            {
                return false;
            }
        }

        public static void Add(this List<Tuple<string, string>> collection, string key, string value)
        {
            collection.Add(new Tuple<string, string>(key, value));
        }

        public static void Add<T, S>(this List<Tuple<T, S>> collection, T t, S s)
        {
            collection.Add(new Tuple<T, S>(t, s));
        }
    }

    #endregion
}
