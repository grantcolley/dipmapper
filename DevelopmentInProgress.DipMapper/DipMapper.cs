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
            MsSql,
            MySql,
            Odbc,
            OleDb,
            Oracle
        }

        private static readonly Dictionary<ConnType, IDbHelper> DbHelpers;

        /// <summary>
        /// Static constructor for DipMapper.
        /// </summary>
        static DipMapper()
        {
            DbHelpers = new Dictionary<ConnType, IDbHelper>
            {
                {ConnType.MsSql, new MsSqlHelper()},
                {ConnType.Oracle, new OracleHelper()},
                {ConnType.MySql, new MySqlHelper()},
                {ConnType.Odbc, new DefaultDbHelper()},
                {ConnType.OleDb, new DefaultDbHelper()}
            };
        }

        /// <summary>
        /// Select a single record and return a populated instance of the specified type.
        /// An exception is thrown if more than one record is returned.
        /// </summary>
        /// <typeparam name="T">The type of object to populate and return.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="parameters">A list of parameters used in the WHERE clause.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>A populated instance of the specified type, else returns null if no record is found.</returns>
        public static T Single<T>(this IDbConnection conn, IEnumerable<IDbDataParameter> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            return Select<T>(conn, parameters, transaction, closeAndDisposeConnection).SingleOrDefault();
        }

        /// <summary>
        /// Selects a set of records and returns them as a list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to populate and return.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="parameters">A list of parameters used in the WHERE clause.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <param name="optimiseObjectCreation">Optimises object creation by compiling a <see cref="DynamicMethod"/> for creating instances of objects of a specified type. The method is cached for reuse.</param>
        /// <returns>A list of the specified type.</returns>
        public static IEnumerable<T> Select<T>(this IDbConnection conn, IEnumerable<IDbDataParameter> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false, bool optimiseObjectCreation = false) where T : class, new()
        {
            var connType = GetConnType(conn);
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlSelect<T>(connType, propertyInfos, parameters);
            var command = GetCommand(conn, sql, null, parameters, null, CommandType.Text, transaction);
            var results = ExecuteReader<T>(conn, command, propertyInfos, closeAndDisposeConnection, optimiseObjectCreation);
            return results;
        }

        /// <summary>
        /// Inserts the object then returns it. Each supported public property will map to a corresponding field in a 
        /// table of the same name as the object.
        /// This is suitable for tables with no identity fields or where the identity value must be known prior to the 
        /// insert e.g. such as Oracle tables before Oracle 12c where the identity value is obtained from a sequence.
        /// This is not suitable for tables that have an auto incrementing identity field e.g. MS Sql Server and MySql.
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="target">The target object.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The newly inserted object.</returns>
        public static T Insert<T>(this IDbConnection conn, T target, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            var connType = GetConnType(conn);
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlInsert<T>(connType, propertyInfos, null, null);
            var parameters = GetGenericParameters(target, propertyInfos, null);
            var command = GetCommand(conn, sql, null, null, parameters, CommandType.Text, transaction);
            int recordsAffected = ExecuteNonQuery(conn, command, closeAndDisposeConnection);
            if (recordsAffected.Equals(1))
            {
                return target;
            }

            throw new Exception("DipMapper exception : " + recordsAffected + " records affected.");
        }

        /// <summary>
        /// Inserts the object then returns it. Each supported public property will map to a corresponding field in a 
        /// table of the same name as the object.
        /// This is suitable for tables with no identity fields or where the identity value must be known prior to the 
        /// insert e.g. such as Oracle tables before Oracle 12c where the identity value is obtained from a sequence.
        /// This is not suitable for tables that have an auto incrementing identity field e.g. MS Sql Server and MySql.
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="target">The target object.</param>
        /// <param name="insertParameters">Paremeters to be inserted. Note, if parameters are provided all other properties of the object will not be inserted.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The newly inserted object.</returns>
        public static T Insert<T>(this IDbConnection conn, T target, IEnumerable<IDbDataParameter> insertParameters, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            var connType = GetConnType(conn);
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlInsert<T>(connType, propertyInfos, null, insertParameters);
            var command = GetCommand(conn, sql, insertParameters, null, null, CommandType.Text, transaction);
            int recordsAffected = ExecuteNonQuery(conn, command, closeAndDisposeConnection);
            if (recordsAffected.Equals(1))
            {
                return target;
            }

            throw new Exception("DipMapper exception : " + recordsAffected + " records affected.");
        }

        /// <summary>
        /// Inserts the object then returns it with its new identity value populated. Each supported public property  
        /// will map to a corresponding field in a table of the same name as the object.
        /// This is suitable for tables that have an auto incrementing identity field e.g. MS Sql Server and MySql.
        /// This is not suitable for tables with no identity fields or where the identity value must be known prior 
        /// to the insert e.g. such as Oracle tables before Oracle 12c where the identity value is obtained from a sequence. 
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="target">The target object.</param>
        /// <param name="identityField">The name of the identity field of target object.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>A fully populated instance of the newly inserted object including its new identity field.</returns>
        public static T Insert<T>(this IDbConnection conn, T target, string identityField, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            var connType = GetConnType(conn);
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlInsert<T>(connType, propertyInfos, identityField, null);
            var parameters = GetGenericParameters(target, propertyInfos, identityField);
            var command = GetCommand(conn, sql, null, null, parameters, CommandType.Text, transaction);
            var result = ExecuteReader<T>(conn, command, propertyInfos, closeAndDisposeConnection, false).SingleOrDefault();
            return result;
        }

        /// <summary>
        /// Inserts the object then returns it with its new identity value populated. Each supported public property  
        /// will map to a corresponding field in a table of the same name as the object.
        /// This is suitable for tables that have an auto incrementing identity field e.g. MS Sql Server and MySql.
        /// This is not suitable for tables with no identity fields or where the identity value must be known prior 
        /// to the insert e.g. such as Oracle tables before Oracle 12c where the identity value is obtained from a sequence. 
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="target">The target object.</param>
        /// <param name="identityField">The name of the identity field of target object.</param>
        /// <param name="insertParameters">Paremeters to be inserted. Parameter names must map to the name of the field being inserted. 
        /// Note, if parameters are provided, only those fields with a corresponding parameter will be included in the insert.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>A fully populated instance of the newly inserted object including its new identity field.</returns>
        public static T Insert<T>(this IDbConnection conn, T target, string identityField, IEnumerable<IDbDataParameter> insertParameters, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            var connType = GetConnType(conn);
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlInsert<T>(connType, propertyInfos, identityField, insertParameters);
            var command = GetCommand(conn, sql, insertParameters, null, null, CommandType.Text, transaction);
            var result = ExecuteReader<T>(conn, command, propertyInfos, closeAndDisposeConnection, false).SingleOrDefault();
            return result;
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="target">The target object.</param>
        /// <param name="identity">The parameter for the identity field used in the WHERE clause.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The number of records affected.</returns>
        public static int Update<T>(this IDbConnection conn, T target, IDbDataParameter identity, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            var connType = GetConnType(conn);
            var propertyInfos = GetPropertyInfos<T>();
            var whereClauseParameters = new List<IDbDataParameter>() {identity};
            var sql = GetSqlUpdate<T>(connType, propertyInfos, null, whereClauseParameters, identity);
            var parameters = GetGenericParameters(target, propertyInfos, null, identity);
            var command = GetCommand(conn, sql, null, whereClauseParameters, parameters, CommandType.Text, transaction);
            return ExecuteNonQuery(conn, command, closeAndDisposeConnection);
        }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="target">The target object.</param>
        /// <param name="updateParameters">A list of parameters used in the WHERE clause.</param>
        /// <param name="whereClauseParameters">A list of parameters used in the WHERE clause.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The number of records affected.</returns>
        public static int Update<T>(this IDbConnection conn, T target, IEnumerable<IDbDataParameter> updateParameters, IEnumerable<IDbDataParameter> whereClauseParameters, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            var connType = GetConnType(conn);
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlUpdate<T>(connType, propertyInfos, updateParameters, whereClauseParameters, null);
            var command = GetCommand(conn, sql, updateParameters, whereClauseParameters, null, CommandType.Text, transaction);
            return ExecuteNonQuery(conn, command, closeAndDisposeConnection);
        }

        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <typeparam name="T">The type of target object.</typeparam>
        /// <param name="conn">The database connection.</param>
        /// <param name="parameters">A list of parameters used in the WHERE clause.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The number of records affected.</returns>
        public static int Delete<T>(this IDbConnection conn, IEnumerable<IDbDataParameter> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false) where T : class, new()
        {
            var connType = GetConnType(conn);
            var sql = GetSqlDelete<T>(connType, parameters);
            var command = GetCommand(conn, sql, null, parameters, null, CommandType.Text, transaction);
            return ExecuteNonQuery(conn, command, closeAndDisposeConnection);
        }

        /// <summary>
        /// Execute a SQL statement or stored procedure and only return the number of rows impacted.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="sql">The SQL or the name of the stored procedure to be executed.</param>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="commandType">Indicates whether executing SQL (Text) or stored proc (StoredProcedure). Note, TableDirect is not supported.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>The number of records affected.</returns>
        public static int ExecuteNonQuery(this IDbConnection conn, string sql, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text, IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
        {
            var command = GetCommand(conn, sql, parameters, null, null, commandType, transaction);
            return ExecuteNonQuery(conn, command, closeAndDisposeConnection);
        }

        /// <summary>
        /// Execute a SQL statement or stored procedure and only return a scalar value. No assumpptions are made about the type of the scalar value, which is returned as n object.
        /// </summary>
        /// <param name="conn">The database connection.</param>
        /// <param name="sql">The SQL or the name of the stored procedure to be executed.</param>
        /// <param name="parameters">A list of parameters.</param>
        /// <param name="commandType">Indicates whether executing SQL (Text) or stored proc (StoredProcedure). Note, TableDirect is not supported.</param>
        /// <param name="transaction">A transaction to attach to the database command.</param>
        /// <param name="closeAndDisposeConnection">A flag indicating whether to close and dispose the connection once the query has been completed.</param>
        /// <returns>A scalar value resulting from executing the SQL statement or stored procedure.</returns>
        public static object ExecuteScalar(this IDbConnection conn, string sql, IEnumerable<IDbDataParameter> parameters = null, CommandType commandType = CommandType.Text, IDbTransaction transaction = null, bool closeAndDisposeConnection = false)
        {
            IDbCommand command = null;

            try
            {
                command = GetCommand(conn, sql, parameters, null, null, commandType, transaction);
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
            var command = GetCommand(conn, sql, null, null, null, CommandType.Text, transaction);
            var results = ExecuteReader<T>(conn, command, propertyInfos, closeAndDisposeConnection, optimiseObjectCreation);
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
        public static IEnumerable<T> ExecuteProcedure<T>(this IDbConnection conn, string procedureName, IEnumerable<IDbDataParameter> parameters = null, IDbTransaction transaction = null, bool closeAndDisposeConnection = false, bool optimiseObjectCreation = false) where T : class, new()
        {
            var propertyInfos = GetPropertyInfos<T>();
            var command = GetCommand(conn, procedureName, parameters, null, null, CommandType.StoredProcedure, transaction);
            var results = ExecuteReader<T>(conn, command, propertyInfos, closeAndDisposeConnection, optimiseObjectCreation);
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

        private static bool UnsupportedProperty(PropertyInfo propertyInfo)
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

        internal static string GetSqlSelect<T>(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, IEnumerable<IDbDataParameter> parameters)
        {
            return "SELECT " + GetSqlSelectFields(propertyInfos) + " FROM " + GetSqlTableName<T>() + GetSqlWhereClause(connType, parameters);
        }

        internal static string GetSqlInsert<T>(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, string identityField, IEnumerable<IDbDataParameter> insertParameters)
        {
            string insertSql = "INSERT INTO " + GetSqlTableName<T>() + GetSqlInsertFields<T>(connType, propertyInfos, identityField, insertParameters);

            if (string.IsNullOrWhiteSpace(identityField))
            {
                return insertSql;
            }

            return DbHelpers[connType].GetSqlSelectWithIdentity<T>(insertSql, propertyInfos, identityField);
        }

        internal static string GetSqlUpdate<T>(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, IEnumerable<IDbDataParameter> updateParameters, IEnumerable<IDbDataParameter> whereClauseParameters, IDbDataParameter identity)
        {
            return "UPDATE " + GetSqlTableName<T>() + " SET " + GetSqlUpdateFields(connType, propertyInfos, updateParameters, identity) + GetSqlWhereClause(connType, whereClauseParameters);
        }

        internal static string GetSqlDelete<T>(ConnType connType, IEnumerable<IDbDataParameter> parameters)
        {
            return "DELETE FROM " + GetSqlTableName<T>() + GetSqlWhereClause(connType, parameters);
        }

        internal static string GetSqlWhereClause(ConnType connType, IEnumerable<IDbDataParameter> parameters)
        {
            if (parameters == null
                || !parameters.Any())
            {
                return string.Empty;
            }

            string where = " WHERE ";

            foreach (var parameter in parameters)
            {
                where += parameter.ParameterName + GetSqlWhereAssignment(parameter.Value) + DbHelpers[connType].GetParameterName(parameter.ParameterName, true) + " AND ";
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

        internal static string GetSqlInsertFields<T>(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, string identityField, IEnumerable<IDbDataParameter> insertParameters)
        {
            string fields = string.Empty;
            string parameters = string.Empty;

            foreach (var propertyInfo in propertyInfos)
            {
                if (!string.IsNullOrWhiteSpace(identityField)
                    && propertyInfo.Name.Equals(identityField))
                {
                    continue;
                }

                if (insertParameters != null
                    && !insertParameters.Any(p => p.ParameterName.Equals(propertyInfo.Name)))
                {
                    continue;
                }

                fields += propertyInfo.Name + ", ";
                parameters += DbHelpers[connType].GetParameterName(propertyInfo.Name) + ", ";
            }

            fields = fields.Remove(fields.Length - 2, 2);
            parameters = parameters.Remove(parameters.Length - 2, 2);

            return " (" + fields + ") VALUES (" + parameters + ")";
        }

        internal static string GetSqlUpdateFields(ConnType connType, IEnumerable<PropertyInfo> propertyInfos, IEnumerable<IDbDataParameter> updateParameters, IDbDataParameter identity)
        {
            string fields = string.Empty;

            foreach (var propertyInfo in propertyInfos)
            {
                if (updateParameters != null
                    && !updateParameters.Any(p => p.ParameterName == propertyInfo.Name))
                {
                    continue;
                }

                if (identity != null
                    && propertyInfo.Name == identity.ParameterName)
                {
                    continue;
                }

                fields += propertyInfo.Name + "=" + DbHelpers[connType].GetParameterName(propertyInfo.Name) + ", ";
            }

            fields = fields.Remove(fields.Length - 2, 2);    
            return fields;
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

        internal static Dictionary<string, object> GetGenericParameters<T>(T target, IEnumerable<PropertyInfo> propertyInfos, string identityField, IDbDataParameter identityParameter = null)
        {
            var parameters = new Dictionary<string, object>();

            if (propertyInfos != null)
            {
                foreach (var propertyInfo in propertyInfos)
                {
                    if (!string.IsNullOrWhiteSpace(identityField)
                        && identityField.Equals(propertyInfo.Name))
                    {
                        continue;
                    }

                    if (identityParameter != null
                        && identityParameter.ParameterName == propertyInfo.Name)
                    {
                        continue;
                    }

                    parameters.Add(propertyInfo.Name, propertyInfo.GetValue(target));
                }
            }

            return parameters;
        }

        internal static ConnType GetConnType(IDbConnection conn)
        {
            if (conn is SqlConnection)
            {
                return ConnType.MsSql;
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

            throw new NotSupportedException("DipMapper exception : Connection " + conn.GetType().Name + " not supported.");
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

        private static IDbCommand GetCommand(IDbConnection conn, string queryString, IEnumerable<IDbDataParameter> dbDataParameters, IEnumerable<IDbDataParameter> dbDataParametersWhereClause, Dictionary<string, object> genericParameters, CommandType commandType, IDbTransaction transaction)
        {
            var connType = GetConnType(conn);
            var command = conn.CreateCommand();
            command.CommandText = queryString;
            command.CommandType = commandType;

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            if (dbDataParameters != null)
            {
                foreach (var dbDataParameter in dbDataParameters)
                {
                    dbDataParameter.ParameterName = DbHelpers[connType].GetParameterName(dbDataParameter.ParameterName);
                    command.Parameters.Add(dbDataParameter);
                }
            }

            if (genericParameters != null)
            {
                foreach (var kvp in genericParameters)
                {
                    DbHelpers[connType].AddDataParameter(command, kvp.Key, kvp.Value);
                }
            }

            if (dbDataParametersWhereClause != null)
            {
                foreach (var dbDataParameter in dbDataParametersWhereClause)
                {
                    dbDataParameter.ParameterName = DbHelpers[connType].GetParameterName(dbDataParameter.ParameterName, true);
                    command.Parameters.Add(dbDataParameter);
                }
            }

            return command;
        }

        private static int ExecuteNonQuery(IDbConnection conn, IDbCommand command, bool closeAndDisposeConnection = false)
        {
            try
            {
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

        private static IEnumerable<T> ExecuteReader<T>(IDbConnection conn, IDbCommand command, IEnumerable<PropertyInfo> propertyInfos, bool closeAndDisposeConnection, bool optimiseObjectCreation) where T : class, new()
        {
            IDataReader reader = null;
            var result = new List<T>();
            var connType = GetConnType(conn);

            try
            {
                OpenConnection(conn);
                reader = command.ExecuteReader();

                var newT = New<T>(optimiseObjectCreation);

                while (reader.Read())
                {
                    var t = DbHelpers[connType].ReadData<T>(reader, newT(), propertyInfos);
                    result.Add(t);
                }
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

        internal interface IDbHelper
        {
            string GetSqlSelectWithIdentity<T>(string sqlInsert, IEnumerable<PropertyInfo> propertyInfos, string identityField);
            string GetParameterName(string name, bool isWhereClause = false);
            void AddDataParameter(IDbCommand comnmand, string parameterName, object data);
            T ReadData<T>(IDataReader reader, T t, IEnumerable<PropertyInfo> propertyInfos);
        }

        internal class DefaultDbHelper : IDbHelper
        {
            public virtual void AddDataParameter(IDbCommand command, string parameterName, object data)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = GetParameterName(parameterName);
                parameter.Value = data ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            public virtual string GetParameterName(string name, bool isWhereClause = false)
            {
                return isWhereClause ? "p" + name : name;
            }

            public virtual string GetSqlSelectWithIdentity<T>(string sqlInsert, IEnumerable<PropertyInfo> propertyInfos, string identityField)
            {
                return sqlInsert;
            }

            public virtual T ReadData<T>(IDataReader reader, T t, IEnumerable<PropertyInfo> propertyInfos)
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
        }

        internal class MsSqlHelper : DefaultDbHelper
        {
            public override void AddDataParameter(IDbCommand command, string parameterName, object data)
            {
                ((SqlCommand)command).Parameters.AddWithValue(GetParameterName(parameterName), data ?? DBNull.Value);
            }

            public override string GetParameterName(string name, bool isWhereClause = false)
            {
                if (name.StartsWith("@"))
                {
                    name = name.Remove(0, 1);
                }

                return isWhereClause ? "@p" + name : "@" + name;
            }

            public override string GetSqlSelectWithIdentity<T>(string sqlInsert, IEnumerable<PropertyInfo> propertyInfos, string identityField)
            {
                return sqlInsert + ";SELECT " + GetSqlSelectFields(propertyInfos) + " FROM " + GetSqlTableName<T>() + " WHERE " + identityField + " = SCOPE_IDENTITY();";
            }
        }

        internal class OracleHelper : DefaultDbHelper
        {
            public override void AddDataParameter(IDbCommand command, string parameterName, object data)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = GetParameterName(parameterName);

                if (data is bool)
                {
                    parameter.Value = (bool)data ? 1 : 0;
                }
                else if (data is Enum)
                {
                    parameter.Value = Convert.ChangeType(data, Enum.GetUnderlyingType(data.GetType()));
                }
                else
                {
                    parameter.Value = data ?? DBNull.Value;
                }

                command.Parameters.Add(parameter);
            }

            public override string GetParameterName(string name, bool isWhereClause = false)
            {
                if (name.StartsWith(":"))
                {
                    name = name.Remove(0, 1);
                }

                return isWhereClause ? ":p" + name : ":" + name;
            }

            public override T ReadData<T>(IDataReader reader, T t, IEnumerable<PropertyInfo> propertyInfos)
            {

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var propertyInfo = propertyInfos.FirstOrDefault(p => p.Name.ToUpper() == reader.GetName(i));
                    if (propertyInfo == null)
                    {
                        throw new Exception("DipMapper exception : Unable to map field " + reader.GetName(i) +
                                            " to object " + t.GetType().Name);
                    }

                    Type propertyType;
                    if (propertyInfo.PropertyType.IsGenericType
                        && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>))
                    {
                        propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
                    }
                    else if (propertyInfo.PropertyType == typeof(Enum))
                    {
                        propertyType = Enum.GetUnderlyingType(propertyInfo.PropertyType);
                    }
                    else
                    {
                        propertyType = propertyInfo.PropertyType;
                    }

                    if (reader[i] == DBNull.Value)
                    {
                        propertyInfo.SetValue(t, null);                        
                    }
                    else if (propertyType == typeof(int))
                    {
                        propertyInfo.SetValue(t, Convert.ToInt32(reader[i]));
                    }
                    else if (propertyType == typeof(double))
                    {
                        propertyInfo.SetValue(t, Convert.ToDouble(reader[i]));
                    }
                    else if (propertyType == typeof(DateTime))
                    {
                        propertyInfo.SetValue(t, Convert.ToDateTime(reader[i]));
                    }
                    else if (propertyType == typeof(bool))
                    {
                        propertyInfo.SetValue(t, Convert.ToBoolean(reader[i]));
                    }
                    else
                    {
                        propertyInfo.SetValue(t, reader[i] == DBNull.Value ? null : reader[i]);
                    }
                }

                return t;
            }
        }

        internal class MySqlHelper : DefaultDbHelper
        {
            public override void AddDataParameter(IDbCommand command, string parameterName, object data)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = GetParameterName(parameterName);
                parameter.Value = data ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }

            public override string GetParameterName(string name, bool isWhereClause = false)
            {
                if (name.StartsWith("?"))
                {
                    name = name.Remove(0, 1);
                }

                return isWhereClause ? "?p" + name : "?" + name;
            }

            public override string GetSqlSelectWithIdentity<T>(string sqlInsert, IEnumerable<PropertyInfo> propertyInfos, string identityField)
            {
                return sqlInsert + ";SELECT " + GetSqlSelectFields(propertyInfos) + " FROM " + GetSqlTableName<T>() + " WHERE " + identityField + " = LAST_INSERT_ID();";
            }
        }
    }
}
