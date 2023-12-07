using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;

namespace Izm.Rumis.Infrastructure.Extensions
{
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Get rows from a stored procedure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="procedure">Procedure name</param>
        /// <returns></returns>
        public static IEnumerable<T> FromStoredProcedure<T>(this DatabaseFacade database, string procedure)
            where T : class, new()
        {
            return FromStoredProcedure<T>(database, procedure, new Dictionary<string, object>());
        }

        /// <summary>
        /// Get rows from a stored procedure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="procedure">Procedure name</param>
        /// <param name="parameters">Parameter list</param>
        /// <returns></returns>
        public static IEnumerable<T> FromStoredProcedure<T>(this DatabaseFacade database, string procedure, Dictionary<string, object> parameters)
            where T : class, new()
        {
            var set = GetDataSet(database, procedure, CommandType.StoredProcedure, parameters);

            if (set.Tables.Count == 0)
                return new List<T>();

            var data = MapDataTable<T>(set.Tables[0]);

            return data;
        }

        /// <summary>
        /// Execute a SQL query and get the resulting dataset.
        /// </summary>
        /// <param name="database"></param>
        /// <param name="sql">SQL query</param>
        /// <param name="commandType">Command type</param>
        /// <param name="parameters">Parameter list</param>
        /// <returns></returns>
        public static DataSet GetDataSet(this DatabaseFacade database, string sql, CommandType commandType, IDictionary<string, object> parameters)
        {
            var result = new DataSet();

            using (var cmd = database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = commandType;

                foreach (var pr in parameters)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@" + pr.Key;
                    p.Value = pr.Value ?? DBNull.Value;
                    cmd.Parameters.Add(p);
                }

                database.OpenConnection();

                var reader = cmd.ExecuteReader();

                // loop through all result sets (considering that it's possible to have more than one)
                do
                {
                    var tb = new DataTable();
                    tb.Load(reader);
                    result.Tables.Add(tb);

                } while (!reader.IsClosed);
            }

            return result;
        }

        private static IEnumerable<T> MapDataTable<T>(DataTable table) where T : class, new()
        {
            var list = new List<T>();
            var nameMap = new Dictionary<string, string>();
            var typeInfo = typeof(T);
            var props = typeInfo.GetProperties();

            foreach (var prop in props)
            {
                var colAttr = prop.GetCustomAttributes(typeof(ColumnAttribute), false);

                if (colAttr.Any())
                {
                    var colName = (colAttr.First() as ColumnAttribute).Name.ToLower();
                    nameMap.Add(colName, prop.Name.ToLower());
                }
            }

            foreach (DataRow row in table.Rows)
            {
                var model = new T();

                foreach (DataColumn col in table.Columns)
                {
                    var colName = col.ColumnName.ToLower();
                    var propName = nameMap.ContainsKey(colName) ? nameMap[colName] : colName;
                    var prop = props.FirstOrDefault(t => t.Name.ToLower() == propName);

                    if (prop != null)
                    {
                        prop.SetValue(model, row[col] is DBNull ? null : row[col]);
                    }
                }

                list.Add(model);
            }

            return list;
        }
    }
}
