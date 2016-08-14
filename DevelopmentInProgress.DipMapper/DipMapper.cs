//-----------------------------------------------------------------------
// <copyright file="DipMapper.cs" company="Development In Progress Ltd">
//     Copyright © 2016. All rights reserved.
// </copyright>
// <author>Grant Colley</author>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DevelopmentInProgress.DipMapper.Test")]

namespace DevelopmentInProgress.DipMapper
{
    /// <summary>
    /// DipMapper is a simple lightweight data access and object relational mapper.
    /// </summary>
    public static class DipMapper
    {
        internal enum ConnType
        {
            MSSQL,
            OleDb
        }

        /// <summary>
        /// Select a single record and return a populated instance of the specified type.
        /// An exception is thrown if more than one record is returned.
        /// </summary>
        /// <typeparam name="T">The type of object to populate and return.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="parameters">A dictionary of parameters used in the WHERE clause.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>A populated instance of the specified type, else returns null if no record is found.</returns>
        public static T Single<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : new()
        {
            return Select<T>(conn, parameters, transaction, closeAndDisposeConnection).SingleOrDefault();
        }

        /// <summary>
        /// Selects a set of records and returns them as a list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to populate and return.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="parameters">A dictionary of parameters used in the WHERE clause.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>A list of the specified type.</returns>
        public static IEnumerable<T> Select<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : new()
        {
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlSelect<T>(propertyInfos, parameters);
            var extendedParameters = GetExtendedParameters<T>(parameters);
            var results = ExecuteReader<T>(conn, sql, propertyInfos, extendedParameters, CommandType.Text, transaction, closeAndDisposeConnection);
            return results;
        }

        /// <summary>
        /// Inserts the object and returns a fully populated instance of the object. 
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="target">The target object.</param>
        /// <param name="identityField">The name of the identity field of target object.</param>
        /// <param name="skipOnCreateFields">Fields to skip when inserting the record. This is used when relying on the database to apply default values when creating a new record.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>A fully populated instance of the newly inserted object including its new identity field.</returns>
        public static T Insert<T>(this IDbConnection conn, T target, string identityField, IEnumerable<string> skipOnCreateFields = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
        {
            if (skipOnCreateFields == null)
            {
                skipOnCreateFields = new List<string>();
            }

            if (!string.IsNullOrWhiteSpace(identityField)
                && !skipOnCreateFields.Contains(identityField))
            {
                ((IList) skipOnCreateFields).Add(identityField);
            }

            var connType = GetConnType(conn);
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlInsert<T>(connType, propertyInfos, identityField, skipOnCreateFields);
            var extendedParameters = GetExtendedParameters(target, propertyInfos, skipOnCreateFields);
            var result = ExecuteReader<T>(conn, sql, propertyInfos, extendedParameters, CommandType.Text, transaction, closeAndDisposeConnection).Single();
            return result;
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="target">The target object.</param>
        /// <param name="parameters">A dictionary of parameters used in the WHERE clause.</param>
        /// <param name="skipOnUpdateFields">Fields to skip when updating e.g. read-only fields.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The number of records affected.</returns>
        public static int Update<T>(this IDbConnection conn, T target, Dictionary<string, object> parameters = null, IEnumerable<string> skipOnUpdateFields = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
        {
            if (skipOnUpdateFields == null)
            {
                skipOnUpdateFields = new List<string>();
            }

            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlUpdate<T>(propertyInfos, parameters, skipOnUpdateFields);
            var extendedParameters = GetExtendedParameters(target, propertyInfos, skipOnUpdateFields, parameters);
            return ExecuteNonQuery(conn, sql, extendedParameters, CommandType.Text, transaction, closeAndDisposeConnection);
        }

        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="parameters">A dictionary of parameters used in the WHERE clause.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The number of records affected.</returns>
        public static int Delete<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
        {
            var sql = GetSqlDelete<T>(parameters);
            var extendedParameters = GetExtendedParameters<T>(parameters);
            return ExecuteNonQuery(conn, sql, extendedParameters, CommandType.Text, transaction, closeAndDisposeConnection);
        }

        /// <summary>
        /// Execute a SQL statement or stored procedure and only return the number of rows impacted.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="sql">The SQL or the name of the stored procedure to be executed.</param>
        /// <param name="parameters">A dictionary of parameters.</param>
        /// <param name="commandType">Indicates whether executing SQL (Text) or stored proc (StoredProcedure). Note, TableDirect is not supported.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The number of records affected.</returns>
        public static int ExecuteNonQuery(this IDbConnection conn, string sql, Dictionary<string, object> parameters = null, CommandType commandType = CommandType.Text, IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
        {
            try
            {
                var command = GetCommand(conn, sql, parameters, commandType, transaction);
                OpenConnection(conn);
                return command.ExecuteNonQuery();
            }
            finally
            {
                if (closeAndDisposeConnection)
                {
                    CloseAndDispose(conn);
                }
            }
        }

        /// <summary>
        /// Execute a SQL statement or stored procedure and only return a scalar value. No assumpptions are made about the type of the scalar value, which is returned as n object.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="sql">The SQL or the name of the stored procedure to be executed.</param>
        /// <param name="parameters">A dictionary of parameters.</param>
        /// <param name="commandType">Indicates whether executing SQL (Text) or stored proc (StoredProcedure). Note, TableDirect is not supported.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>A scalar value resulting from executing the SQL statement or stored procedure.</returns>
        public static object ExecuteScalar(this IDbConnection conn, string sql, Dictionary<string, object> parameters = null, CommandType commandType = CommandType.Text, IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
        {
            try
            {
                var command = GetCommand(conn, sql, parameters, commandType, transaction);
                OpenConnection(conn);
                return command.ExecuteScalar();
            }
            finally
            {
                if (closeAndDisposeConnection)
                {
                    CloseAndDispose(conn);
                }
            }
        }

        /// <summary>
        /// Executes a SQL statement and returns the results as a list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="sql">The SQL statement to execute.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The results of executing the SQL statement as a list of the specified type.</returns>
        public static IEnumerable<T> ExecuteSql<T>(this IDbConnection conn, string sql, IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
        {
            var propertyInfos = GetPropertyInfos<T>();
            var results = ExecuteReader<T>(conn, sql, propertyInfos, null, CommandType.Text, transaction, closeAndDisposeConnection);
            return results;
        }

        /// <summary>
        /// Executes a stored procedure and returns the results as a list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="procedureName">The stored procedure to execute.</param>
        /// <param name="parameters">A dictionary of parameters passed to the stored procedure.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The results of executing the stored procedure as a list of the specified type.</returns>
        public static IEnumerable<T> ExecuteProcedure<T>(this IDbConnection conn, string procedureName, Dictionary<string, object> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
        {
            var propertyInfos = GetPropertyInfos<T>();
            var results = ExecuteReader<T>(conn, procedureName, propertyInfos, parameters, CommandType.StoredProcedure, transaction, closeAndDisposeConnection);
            return results;
        }

        internal static IEnumerable<PropertyInfo> GetPropertyInfos<T>()
        {
            var propertyInfoResults = new List<PropertyInfo>();
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                if (UnsupportedProperty(propertyInfo))
                {
                    continue;
                }

                propertyInfoResults.Add(propertyInfo);
            }

            return propertyInfoResults;
        }

        internal static bool UnsupportedProperty(PropertyInfo propertyInfo)
        {
            // Skip non-public properties and properties that are either 
            // classes (but not strings), interfaces, lists, generic 
            // lists or arrays.
            var propertyType = propertyInfo.PropertyType;
            if (propertyType.IsNotPublic)
            {
                return true;
            }

            if (propertyType != typeof(string)
                && (propertyType.IsClass
                    || propertyType.IsInterface
                    || propertyType.IsArray
                    || propertyType.GetInterfaces()
                        .Any(
                            i =>
                                (i.GetTypeInfo().Name.Equals(typeof(IEnumerable).Name)
                                 || (i.IsGenericType &&
                                     i.GetGenericTypeDefinition().Name.Equals(typeof(IEnumerable<>).Name))))))
            {
                return true;
            }

            return false;
        }

        internal static string GetSqlTableName<T>()
        {
            if (typeof(T).IsGenericType
                && typeof(T).GenericTypeArguments.Any())
            {
                return typeof(T).GenericTypeArguments[0].Name;
            }

            return typeof(T).Name;
        }

        internal static string GetSqlSelect<T>(IEnumerable<PropertyInfo> propertyInfos, Dictionary<string, object> parameters = null)
        {
            return "SELECT " + GetSqlSelectFields(propertyInfos) + " FROM " + GetSqlTableName<T>() + GetSqlWhereClause(parameters) + ";";
        }

        internal static string GetSqlInsert<T>(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, string identityField, IEnumerable<string> skipOnCreateFields = null)
        {
            return "INSERT INTO " + GetSqlTableName<T>() + GetSqlInsertFields(propertyInfos, skipOnCreateFields) + ";" +
                   "SELECT " + GetSqlSelectFields(propertyInfos) + " FROM " + GetSqlTableName<T>() + " WHERE " +
                   identityField + " = " + GetIdentitySql(connType) + ";";
        }

        internal static string GetSqlUpdate<T>(IEnumerable<PropertyInfo> propertyInfos, Dictionary<string, object> parameters, IEnumerable<string> skipOnUpdateFields = null)
        {
            return "UPDATE " + GetSqlTableName<T>() + " SET " + GetSqlUpdateFields(propertyInfos, skipOnUpdateFields) + GetSqlWhereClause(parameters) + ";";
        }

        internal static string GetSqlDelete<T>(Dictionary<string, object> parameters)
        {
            return "DELETE FROM " + GetSqlTableName<T>() + GetSqlWhereClause(parameters) + ";";
        }

        internal static string GetSqlWhereClause(Dictionary<string, object> parameters)
        {
            if (parameters == null
                || !parameters.Any())
            {
                return string.Empty;
            }

            string where = " WHERE ";

            foreach (var parameter in parameters)
            {
                where += parameter.Key + GetSqlWhereAssignment(parameter.Value) + "@_" + parameter.Key + " AND ";
            }

            if (where.EndsWith(" AND "))
            {
                where = where.Remove(where.Length - 5, 5);
            }

            return where;
        }

        internal static string GetSqlSelectFields(IEnumerable<PropertyInfo> propertyInfos)
        {
            string fields = string.Empty;

            foreach (var propertyInfo in propertyInfos)
            {
                fields += propertyInfo.Name + ", ";
            }

            fields = fields.Remove(fields.Length - 2, 2);
            return fields;
        }

        internal static string GetSqlUpdateFields(IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipOnUpdateFields)
        {
            string fields = string.Empty;

            foreach (var propertyInfo in propertyInfos)
            {
                if (skipOnUpdateFields.Contains(propertyInfo.Name))
                {
                    continue;
                }

                fields += propertyInfo.Name + "=@" + propertyInfo.Name + ", ";
            }

            fields = fields.Remove(fields.Length - 2, 2);
            return fields;
        }

        internal static string GetSqlInsertFields(IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipOnCreateFields)
        {
            string fields = string.Empty;
            string parameters = string.Empty;

            foreach (var propertyInfo in propertyInfos)
            {
                if (skipOnCreateFields.Contains(propertyInfo.Name))
                {
                    continue;
                }

                fields += propertyInfo.Name + ", ";
                parameters += "@" + propertyInfo.Name + ", ";
            }

            fields = fields.Remove(fields.Length - 2, 2);
            parameters = parameters.Remove(parameters.Length - 2, 2);

            return " (" + fields + ") VALUES (" + parameters + ")";
        }

        internal static string GetSqlWhereAssignment(object value)
        {
            if (value == null)
            {
                return " is ";
            }

            if (value.GetType().Name == "String"
                && string.IsNullOrEmpty(value.ToString()))
            {
                return " is ";
            }

            return "=";
        }

        internal static string GetIdentitySql(ConnType connType)
        {
            switch (connType)
            {
                case ConnType.MSSQL:
                    return "SCOPE_IDENTITY()";
            }

            throw new NotImplementedException("Connection " + connType.GetType().Name + " not supported.");
        }

        internal static Dictionary<string, object> GetExtendedParameters<T>(Dictionary<string, object> parameters = null)
        {
            return GetExtendedParameters<T>(default(T), null, null, parameters);
        }

        internal static Dictionary<string, object> GetExtendedParameters<T>(T target, IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipFields)
        {
            return GetExtendedParameters<T>(target, propertyInfos, skipFields, null);
        }

        internal static Dictionary<string, object> GetExtendedParameters<T>(T target, IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipFields, Dictionary<string, object> parameters)
        {
            var extendedParameters = new Dictionary<string, object>();

            if (propertyInfos != null)
            {
                foreach (var propertyInfo in propertyInfos)
                {
                    if (skipFields.Contains(propertyInfo.Name))
                    {
                        continue;
                    }

                    var val = propertyInfo.GetValue(target);
                    extendedParameters.Add("@" + propertyInfo.Name, val ?? DBNull.Value);
                }
            }

            if (parameters == null)
            {
                return extendedParameters;
            }

            foreach (var parameter in parameters)
            {
                extendedParameters.Add("@_" + parameter.Key, parameter.Value ?? DBNull.Value);
            }

            return extendedParameters;
        }

        private static T CreateNew<T>()
        {
            return Activator.CreateInstance<T>();
        }

        private static ConnType GetConnType(IDbConnection conn)
        {
            if (conn is SqlConnection)
            {
                return ConnType.MSSQL;
            }

            throw new NotImplementedException("Connection " + conn.GetType().Name + " not supported.");
        }

        private static void OpenConnection(IDbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        private static IDbCommand GetCommand(IDbConnection conn, string queryString, Dictionary<string, object> parameters, CommandType commandType, IDbTransaction transaction)
        {
            if (conn is SqlConnection)
            {
                return GetSqlCommand((SqlConnection)conn, queryString, parameters, commandType, (SqlTransaction)transaction);
            }

            throw new NotImplementedException("Connection " + conn.GetType().Name + " not supported.");
        }

        private static SqlCommand GetSqlCommand(SqlConnection conn, string queryString, Dictionary<string, object> parameters, CommandType commandType, SqlTransaction transaction)
        {
            var sqlCommand = new SqlCommand(queryString, conn) {CommandType = commandType};

            if (transaction != null)
            {
                sqlCommand.Transaction = transaction;
            }

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    sqlCommand.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }
            }

            return sqlCommand;
        }

        private static IEnumerable<T> ExecuteReader<T>(IDbConnection conn, string sql, IEnumerable<PropertyInfo> propertyInfos, Dictionary<string, object> parameters, CommandType commandType, IDbTransaction transaction, bool closeAndDisposeConnection)
        {
            var result = new List<T>();

            IDataReader reader = null;

            try
            {
                var command = GetCommand(conn, sql, parameters, commandType, transaction);
                OpenConnection(conn);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var t = ReadData<T>(reader, propertyInfos);
                    result.Add(t);
                }
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        reader.Close();
                    }

                    reader.Dispose();
                }

                if (closeAndDisposeConnection)
                {
                    CloseAndDispose(conn);
                }
            }

            return result;
        }

        private static T ReadData<T>(IDataReader reader, IEnumerable<PropertyInfo> propertyInfos)
        {
            var t = CreateNew<T>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var propertyInfo = propertyInfos.FirstOrDefault(p => p.Name == reader.GetName(i));
                if (propertyInfo == null)
                {
                    throw new Exception("DipMapper exception : Unable to map field " + reader.GetName(i) +
                                        " to object " + t.GetType().Name);
                }

                propertyInfo.SetValue(t, reader[i] == DBNull.Value ? null : reader[i]);
            }

            return t;
        }

        private static void CloseAndDispose(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
            }

            conn.Dispose();
        }
    }
}
