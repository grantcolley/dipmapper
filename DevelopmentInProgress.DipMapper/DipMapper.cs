//-----------------------------------------------------------------------
// <copyright file="DipMapper.cs" company="Development In Progress Ltd">
//     Copyright © 2016. All rights reserved.
// </copyright>
// <author>Grant Colley</author>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DevelopmentInProgress.DipMapper.Test")]

namespace DevelopmentInProgress.DipMapper
{
    /// <summary>
    /// DipMapper is a simple lightweight data access and object relational mapper.
    /// </summary>
    public static class DipMapper
    {
        internal static class DynamicMethodCache
        {
            private static readonly IDictionary<Type, object> cache = new ConcurrentDictionary<Type, object>();

            internal static void Add<T>(T value)
            {
                cache.Add(typeof(T), value);
            }

            internal static bool Contains(Type t)
            {
                return cache.ContainsKey(t);
            }

            internal static T Get<T>()
            {
                return (T)cache[typeof(T)];
            }
        }

        internal enum ConnType
        {
            MSSQL,
            MySql,
            Odbc,
            OleDb,
            Oracle
        }

        private static readonly Dictionary<ConnType, IAddDataParameter> AddDataParameters;

        private static readonly Dictionary<ConnType, ISqlIdentitySelect> SqlIdentitySelects;

        /// <summary>
        /// Static constructor for DipMapper.
        /// </summary>
        static DipMapper()
        {
            AddDataParameters = new Dictionary<ConnType, IAddDataParameter>
            {
                {ConnType.MSSQL, new MSSQLAddDataParameter()},
                {ConnType.Oracle, new OracleAddDataParameter()},
                {ConnType.MySql, new DefaultAddDataParameter()},
                {ConnType.Odbc, new DefaultAddDataParameter()},
                {ConnType.OleDb, new DefaultAddDataParameter()}
            };

            SqlIdentitySelects = new Dictionary<ConnType, ISqlIdentitySelect>
            {
                {ConnType.MSSQL, new MSSQLSqlSelectWithIdentity()},
                {ConnType.Oracle, new OracleSqlSelectWithIdentity()},
                {ConnType.MySql, new MySqlSqlSelectWithIdentity()},
                {ConnType.Odbc, new DefaultSqlSelectWithIdentity()},
                {ConnType.OleDb, new DefaultSqlSelectWithIdentity()}
            };
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
        public static T Single<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
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
        /// <param name="optimiseObjectCreation">Optimises object creation by compiling a <see cref="DynamicMethod"/> for creating instances of objects of a specified type. The method is cached for reuse.</param>
        /// <returns>A list of the specified type.</returns>
        public static IEnumerable<T> Select<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false, bool optimiseObjectCreation = false) where T : class, new()
        {
            var connType = GetConnType(conn);
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlSelect<T>(connType, propertyInfos, parameters);
            var extendedParameters = GetExtendedParameters<T>(connType, parameters);
            var results = ExecuteReader<T>(conn, sql, propertyInfos, extendedParameters, CommandType.Text, transaction, closeAndDisposeConnection, optimiseObjectCreation);
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
        public static T Insert<T>(this IDbConnection conn, T target, string identityField = "", IEnumerable<string> skipOnCreateFields = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            return Insert(conn, target, identityField, "", skipOnCreateFields, transaction, closeAndDisposeConnection);
        }

        /// <summary>
        /// Inserts the object and returns a fully populated instance of the object. 
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="target">The target object.</param>
        /// <param name="identityField">The name of the identity field of target object.</param>
        /// <param name="sequenceName">The name of the Oracle sequence field of target object.</param>
        /// <param name="skipOnCreateFields">Fields to skip when inserting the record. This is used when relying on the database to apply default values when creating a new record.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>A fully populated instance of the newly inserted object including its new identity field.</returns>
        public static T Insert<T>(this IDbConnection conn, T target, string identityField = "", string sequenceName = "", IEnumerable<string> skipOnCreateFields = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            if (skipOnCreateFields == null)
            {
                skipOnCreateFields = new List<string>();
            }

            var connType = GetConnType(conn);

            if (connType != ConnType.Oracle
                && !string.IsNullOrWhiteSpace(identityField)
                && !skipOnCreateFields.Contains(identityField))
            {
                ((IList) skipOnCreateFields).Add(identityField);
            }

            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlInsert<T>(connType, propertyInfos, identityField, sequenceName, skipOnCreateFields);
            var extendedParameters = GetExtendedParameters(target, connType, propertyInfos, skipOnCreateFields);
            var result = ExecuteReader<T>(conn, sql, propertyInfos, extendedParameters, CommandType.Text, transaction, closeAndDisposeConnection, false).SingleOrDefault();
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
        public static int Update<T>(this IDbConnection conn, T target, Dictionary<string, object> parameters = null, IEnumerable<string> skipOnUpdateFields = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            if (skipOnUpdateFields == null)
            {
                skipOnUpdateFields = new List<string>();
            }

            var connType = GetConnType(conn);
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlUpdate<T>(connType, propertyInfos, parameters, skipOnUpdateFields);
            var extendedParameters = GetExtendedParameters(target, connType, propertyInfos, skipOnUpdateFields, parameters);
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
        public static int Delete<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            var connType = GetConnType(conn);
            var sql = GetSqlDelete<T>(connType, parameters);
            var extendedParameters = GetExtendedParameters<T>(connType, parameters);
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
            IDbCommand command = null;

            try
            {
                command = GetCommand(conn, sql, parameters, commandType, transaction);
                OpenConnection(conn);
                var recordsAffected = command.ExecuteNonQuery();
                return recordsAffected;
            }
            finally
            {
                CloseAndDispose(command);

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
            IDbCommand command = null;

            try
            {
                command = GetCommand(conn, sql, parameters, commandType, transaction);
                OpenConnection(conn);
                var result = command.ExecuteScalar();
                return result;
            }
            finally
            {
                CloseAndDispose(command);

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
        /// <param name="optimiseObjectCreation">Optimises object creation by compiling a <see cref="DynamicMethod"/> for creating instances of objects of a specified type. The method is cached for reuse.</param>
        /// <returns>The results of executing the SQL statement as a list of the specified type.</returns>
        public static IEnumerable<T> ExecuteSql<T>(this IDbConnection conn, string sql, IDbTransaction transaction = null, bool closeAndDisposeConnection = false, bool optimiseObjectCreation = false) where T : class, new()
        {
            var propertyInfos = GetPropertyInfos<T>();
            var results = ExecuteReader<T>(conn, sql, propertyInfos, null, CommandType.Text, transaction, closeAndDisposeConnection, optimiseObjectCreation);
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
        /// <param name="optimiseObjectCreation">Optimises object creation by compiling a <see cref="DynamicMethod"/> for creating instances of objects of a specified type. The method is cached for reuse.</param>
        /// <returns>The results of executing the stored procedure as a list of the specified type.</returns>
        public static IEnumerable<T> ExecuteProcedure<T>(this IDbConnection conn, string procedureName, Dictionary<string, object> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false, bool optimiseObjectCreation = false) where T : class, new()
        {
            var propertyInfos = GetPropertyInfos<T>();
            var results = ExecuteReader<T>(conn, procedureName, propertyInfos, parameters, CommandType.StoredProcedure, transaction, closeAndDisposeConnection, optimiseObjectCreation);
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

        internal static string GetSqlSelect<T>(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, Dictionary<string, object> parameters = null)
        {
            return "SELECT " + GetSqlSelectFields(propertyInfos) + " FROM " + GetSqlTableName<T>() + GetSqlWhereClause(connType, parameters) + ";";
        }

        internal static string GetSqlInsert<T>(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, string identityField, string sequenceName, IEnumerable<string> skipOnCreateFields = null)
        {
            string insertSql = "INSERT INTO " + GetSqlTableName<T>() + GetSqlInsertFields(connType, propertyInfos, skipOnCreateFields) + ";";

            if (string.IsNullOrEmpty(identityField))
            {
                return insertSql;
            }

            return SqlIdentitySelects[connType].GetSqlSelectWithIdentity<T>(insertSql, propertyInfos, identityField, sequenceName);
        }

        internal static string GetSqlUpdate<T>(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, Dictionary<string, object> parameters, IEnumerable<string> skipOnUpdateFields = null)
        {
            return "UPDATE " + GetSqlTableName<T>() + " SET " + GetSqlUpdateFields(connType, propertyInfos, skipOnUpdateFields) + GetSqlWhereClause(connType, parameters) + ";";
        }

        internal static string GetSqlDelete<T>(ConnType connType, Dictionary<string, object> parameters)
        {
            return "DELETE FROM " + GetSqlTableName<T>() + GetSqlWhereClause(connType, parameters) + ";";
        }

        internal static string GetSqlWhereClause(ConnType connType, Dictionary<string, object> parameters)
        {
            if (parameters == null
                || !parameters.Any())
            {
                return string.Empty;
            }

            string where = " WHERE ";

            foreach (var parameter in parameters)
            {
                where += parameter.Key + GetSqlWhereAssignment(parameter.Value) + GetParameterPrefix(connType, true) + parameter.Key + " AND ";
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

        internal static string GetSqlUpdateFields(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipOnUpdateFields)
        {
            string fields = string.Empty;

            foreach (var propertyInfo in propertyInfos)
            {
                if (skipOnUpdateFields.Contains(propertyInfo.Name))
                {
                    continue;
                }

                fields += propertyInfo.Name + "=" + GetParameterPrefix(connType) + propertyInfo.Name + ", ";
            }

            fields = fields.Remove(fields.Length - 2, 2);
            return fields;
        }

        internal static string GetSqlInsertFields(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipOnCreateFields)
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
                parameters += GetParameterPrefix(connType) + propertyInfo.Name + ", ";
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

        internal static Dictionary<string, object> GetExtendedParameters<T>(ConnType connType, Dictionary<string, object> parameters = null)
        {
            return GetExtendedParameters<T>(default(T), connType, null, null, parameters);
        }

        internal static Dictionary<string, object> GetExtendedParameters<T>(T target, ConnType connType, IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipFields)
        {
            return GetExtendedParameters<T>(target, connType, propertyInfos, skipFields, null);
        }

        internal static Dictionary<string, object> GetExtendedParameters<T>(T target, ConnType connType, IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipFields, Dictionary<string, object> parameters)
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
                    extendedParameters.Add(GetParameterPrefix(connType) + propertyInfo.Name, val ?? DBNull.Value);
                }
            }

            if (parameters == null)
            {
                return extendedParameters;
            }

            foreach (var parameter in parameters)
            {
                extendedParameters.Add(GetParameterPrefix(connType, true) + parameter.Key, parameter.Value ?? DBNull.Value);
            }

            return extendedParameters;
        }

        internal static ConnType GetConnType(IDbConnection conn)
        {
            if (conn is SqlConnection)
            {
                return ConnType.MSSQL;
            }
            
            if (conn.GetType().Name.Equals("OracleConnection"))
            {
                return ConnType.Oracle;
            }
            
            if (conn.GetType().Name.Equals("MySqlConnection"))
            {
                return ConnType.MySql;
            }
            
            if (conn.GetType().Name.Equals("OdbcConnection"))
            {
                return ConnType.Odbc;
            }
            
            if (conn.GetType().Name.Equals("OleDbConnection"))
            {
                return ConnType.OleDb;
            }

            throw new NotSupportedException("Connection " + conn.GetType().Name + " not supported.");
        }

        internal static Func<T> New<T>(bool optimiseObjectCreation) where T : class, new()
        {
            if (optimiseObjectCreation)
            {
                if (DynamicMethodCache.Contains(typeof(Func<T>)))
                {
                    return DynamicMethodCache.Get<Func<T>>();
                }

                return DynamicMethod<T>();
            }

            return ActivatorCreateInstance<T>;
        }

        private static T ActivatorCreateInstance<T>() where T : class, new()
        {
            return Activator.CreateInstance<T>();
        }

        private static Func<T> DynamicMethod<T>() where T : class, new()
        {
            var t = typeof (T);
            var dynMethod = new DynamicMethod("DIPMAPPER_" + typeof(T).Name, t, null, t);
            ILGenerator ilGen = dynMethod.GetILGenerator();
            ilGen.Emit(OpCodes.Newobj, t.GetConstructor(Type.EmptyTypes));
            ilGen.Emit(OpCodes.Ret);
            var result = (Func<T>)dynMethod.CreateDelegate(typeof(Func<T>));
            DynamicMethodCache.Add(result);
            return result;
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
            var command = conn.CreateCommand();
            command.CommandText = queryString;
            command.CommandType = commandType;

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    AddDataParameters[GetConnType(conn)].AddDataParameter(command, kvp.Key, kvp.Value);
                }
            }

            return command;
        }

        private static IEnumerable<T> ExecuteReader<T>(IDbConnection conn, string sql, IEnumerable<PropertyInfo> propertyInfos, Dictionary<string, object> parameters, CommandType commandType, IDbTransaction transaction, bool closeAndDisposeConnection, bool optimiseObjectCreation) where T : class, new()
        {
            var result = new List<T>();

            IDataReader reader = null;
            IDbCommand command = null;

            try
            {
                command = GetCommand(conn, sql, parameters, commandType, transaction);
                OpenConnection(conn);
                reader = command.ExecuteReader();

                var newT = New<T>(optimiseObjectCreation);

                while (reader.Read())
                {
                    var t = ReadData<T>(reader, newT(), propertyInfos);
                    result.Add(t);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                throw;
            }
            finally
            {
                CloseAndDispose(reader);

                CloseAndDispose(command);

                if (closeAndDisposeConnection)
                {
                    CloseAndDispose(conn);
                }
            }

            return result;
        }

        private static T ReadData<T>(IDataReader reader, T t, IEnumerable<PropertyInfo> propertyInfos)
        {

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

        private static string GetParameterPrefix(ConnType connType, bool isWhereClause = false)
        {
            switch (connType)
            {
                case ConnType.MSSQL:
                    return isWhereClause ? "@p" : "@";
                case ConnType.MySql:
                    return isWhereClause ? "?p" : "?";
                case ConnType.Oracle:
                    return isWhereClause ? ":p" : ":";
                default:
                    throw new NotSupportedException("Connection " + connType.GetType().Name + " not supported.");
            }
        }

        private static void CloseAndDispose(IDataReader reader)
        {
            if (reader != null)
            {
                if (!reader.IsClosed)
                {
                    reader.Close();
                }

                reader.Dispose();
            }
        }

        private static void CloseAndDispose(IDbCommand command)
        {
            if (command != null)
            {
                command.Dispose();
            }
        }

        private static void CloseAndDispose(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
            }

            conn.Dispose();
        }

        internal interface IAddDataParameter
        {
            void AddDataParameter(IDbCommand comnmand, string parameterName, object data);
        }

        internal class DefaultAddDataParameter : IAddDataParameter
        {
            public void AddDataParameter(IDbCommand command, string parameterName, object data)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.Value = data;
                command.Parameters.Add(parameter);
            }
        }

        internal class MSSQLAddDataParameter : IAddDataParameter
        {
            public void AddDataParameter(IDbCommand command, string parameterName, object data)
            {
                ((SqlCommand) command).Parameters.AddWithValue(parameterName, data);
            }
        }

        internal class OracleAddDataParameter : IAddDataParameter
        {
            public void AddDataParameter(IDbCommand command, string parameterName, object data)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;

                if (data is bool)
                {
                    parameter.Value = (bool)data ? 1 : 0;
                }
                if (data is Enum)
                {
                    parameter.Value = Convert.ChangeType(data, Enum.GetUnderlyingType(data.GetType()));
                }

                command.Parameters.Add(parameter);
            }
        }

        internal interface ISqlIdentitySelect
        {
            string GetSqlSelectWithIdentity<T>(string sqlInsert, IEnumerable<PropertyInfo> propertyInfos, string identityField, string sequenceName);
        }

        internal class DefaultSqlSelectWithIdentity : ISqlIdentitySelect
        {
            public string GetSqlSelectWithIdentity<T>(string sqlInsert, IEnumerable<PropertyInfo> propertyInfos, string identityField, string sequenceName)
            {
                return sqlInsert;
            }
        }

        internal class MSSQLSqlSelectWithIdentity : ISqlIdentitySelect
        {
            public string GetSqlSelectWithIdentity<T>(string sqlInsert, IEnumerable<PropertyInfo> propertyInfos, string identityField, string sequenceName)
            {
                return sqlInsert 
                    + "SELECT " + GetSqlSelectFields(propertyInfos) + " FROM " + GetSqlTableName<T>() + " WHERE " + identityField + " = SCOPE_IDENTITY();";
            }
        }

        internal class MySqlSqlSelectWithIdentity : ISqlIdentitySelect
        {
            public string GetSqlSelectWithIdentity<T>(string sqlInsert, IEnumerable<PropertyInfo> propertyInfos, string identityField, string sequenceName)
            {
                return sqlInsert 
                    + "SELECT " + GetSqlSelectFields(propertyInfos) + " FROM " + GetSqlTableName<T>() + " WHERE " + identityField + " = LAST_INSERT_ID();";
            }
        }

        internal class OracleSqlSelectWithIdentity : ISqlIdentitySelect
        {
            public string GetSqlSelectWithIdentity<T>(string sqlInsert, IEnumerable<PropertyInfo> propertyInfos, string identityField, string sequenceName)
            {
                return "DECLARE "
                       + "   next" + identityField + " NUMBER;"
                       + "BEGIN "
                       + " next" + identityField + " := " + sequenceName + ".nextval;"
                       + sqlInsert + ";"
                       + " SELECT " + GetSqlSelectFields(propertyInfos) + " FROM " + GetSqlTableName<T>() + " WHERE " + identityField + " = next" + identityField + ";"
                       + " END;";
            }
        }
    }
}
