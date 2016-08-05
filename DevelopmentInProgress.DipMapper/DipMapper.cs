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
    public static class DipMapper
    {
        public static T Single<T>(this IDbConnection conn, Dictionary<string, object> parameters = null) where T : new()
        {
            return Select<T>(conn, parameters).SingleOrDefault();
        }

        public static IEnumerable<T> Select<T>(this IDbConnection conn, Dictionary<string, object> parameters = null) where T : new()
        {
            var result = new List<T>();
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlSelect<T>(propertyInfos, parameters);

            using (conn)
            {
                IDataReader reader = null;

                try
                {
                    var command = GetCommand(conn, sql, parameters);
                    OpenConnection(conn);

                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var t = CreateNew<T>();
                        for (int i = 0; i < reader.FieldCount; i ++)
                        {
                            var propertyInfo = propertyInfos.FirstOrDefault(p => p.Name == reader.GetName(i));
                            if (propertyInfo == null)
                            {
                                throw new Exception("DipMapper exception : Unable to map field " + reader.GetName(i) +
                                                    " to object " + t.GetType().Name);
                            }

                            propertyInfo.SetValue(t, reader[i]);
                        }

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

                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }

            return result;
        }

        public static int Insert<T>(this IDbConnection conn, T target, IEnumerable<string> skipOnCreateFields = null)
        {
            int id;
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlInsert<T>(propertyInfos, skipOnCreateFields);
            var extendedParameters = GetExtendedParameters(target, propertyInfos);

            using (conn)
            {
                try
                {
                    var command = GetCommand(conn, sql, extendedParameters);
                    OpenConnection(conn);
                    id = (int)command.ExecuteScalar();
                }
                finally
                {
                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }

            return id;
        }

        public static void Update<T>(this IDbConnection conn, T target, Dictionary<string, object> parameters = null, IEnumerable<string> skipOnUpdateFields = null)
        {
            var propertyInfos = GetPropertyInfos<T>();
            var sql = GetSqlUpdate<T>(propertyInfos, parameters, skipOnUpdateFields);
            var extendedParameters = GetExtendedParameters(target, propertyInfos, parameters);

            using (conn)
            {
                try
                {
                    var command = GetCommand(conn, sql, extendedParameters);
                    OpenConnection(conn);
                    command.ExecuteNonQuery();
                }
                finally 
                {
                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }
        }

        public static void Delete<T>(this IDbConnection conn, Dictionary<string, object> parameters = null)
        {
            var sql = GetSqlDelete<T>(parameters);

            using (conn)
            {
                try
                {
                    var command = GetCommand(conn, sql, parameters);
                    OpenConnection(conn);
                    command.ExecuteNonQuery();
                }
                finally
                {
                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }
                }
            }
        }

        public static T ExecuteScalar<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, CommandType commandType = CommandType.TableDirect, string sql = "")
        {
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        public static IEnumerable<T> Execute<T>(this IDbConnection conn, Dictionary<string, object> parameters = null, CommandType commandType = CommandType.TableDirect, string sql = "")
        {
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        internal static string GetSqlSelect<T>(IEnumerable<PropertyInfo> propertyInfos, Dictionary<string, object> parameters = null)
        {
            return "SELECT " + GetSqlSelectFields(propertyInfos) + " FROM " + GetSqlTableName<T>() + GetSqlWhereClause(parameters) + ";";
        }

        internal static string GetSqlInsert<T>(IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipOnCreateFields = null)
        {
            return "INSERT INTO " + GetSqlTableName<T>() + GetSqlInsertFields(propertyInfos, skipOnCreateFields) + ";";
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
                where += parameter.Key + GetSqlWhereAssignment(parameter.Value) + "@" + parameter.Key + " AND ";
            }

            if (where.EndsWith(" AND "))
            {
                where = where.Remove(where.Length - 5, 5);
            }

            return where;
        }

        internal static IEnumerable<PropertyInfo> GetPropertyInfos<T>()
        {
            var propertyInfoResults = new List<PropertyInfo>();
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                if (SkipProperty(propertyInfo))
                {
                    continue;
                }

                propertyInfoResults.Add(propertyInfo);
            }

            return propertyInfoResults;
        }

        internal static string GetSqlTableName<T>()
        {
            if (typeof (T).IsGenericType
                && typeof (T).GenericTypeArguments.Any())
            {
                return typeof (T).GenericTypeArguments[0].Name;
            }

            return typeof (T).Name;
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

        internal static string GetSqlUpdateFields(IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipOnUpdateFields = null)
        {
            string fields = string.Empty;

            if (skipOnUpdateFields == null)
            {
                skipOnUpdateFields = new List<string>();
            }

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

        internal static string GetSqlInsertFields(IEnumerable<PropertyInfo> propertyInfos, IEnumerable<string> skipOnCreateFields = null)
        {
            string fields = string.Empty;
            string parameters = string.Empty;

            if (skipOnCreateFields == null)
            {
                skipOnCreateFields = new List<string>();
            }

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

        private static string GetSqlWhereAssignment(object value)
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

        //private static string SqlConvert(object value)
        //{
        //    if (value == null)
        //    {
        //        return "null";
        //    }

        //    switch (value.GetType().Name)
        //    {
        //        case "String":
        //            if (string.IsNullOrEmpty(value.ToString()))
        //            {
        //                return "null";
        //            }

        //            return "'" + value + "'";

        //        case "Boolean":
        //            return (bool) value ? "1" : "0";

        //        case "DateTime":
        //            return "'" + ((DateTime) value).Date + "'";

        //        default:
        //            if (value.GetType().IsEnum)
        //            {
        //                return Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())).ToString();
        //            }

        //            return value.ToString();
        //    }
        //}

        internal static bool SkipProperty(PropertyInfo propertyInfo)
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

        internal static Dictionary<string, object> GetExtendedParameters<T>(T target, IEnumerable<PropertyInfo> propertyInfos, Dictionary<string, object> parameters = null)
        {
            var extendedParameters = new Dictionary<string, object>();
            for (int i = 0; i < propertyInfos.Count(); i++)
            {
                var propertyInfo = propertyInfos.ElementAt(i);
                var val = propertyInfo.GetValue(target);
                extendedParameters.Add("@" + propertyInfo.Name, val);
            }

            if (parameters == null)
            {
                return extendedParameters;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                string parameterName;
                var parameter = parameters.ElementAt(i);
                if (extendedParameters.ContainsKey("@" + parameter.Key))
                {
                    parameterName = "@" + parameter.Key;
                }
                else
                {
                    parameterName = "@" + parameter.Key + "1";
                }

                extendedParameters.Add(parameterName, parameter.Value);
            }

            return extendedParameters;
        }

        private static T CreateNew<T>()
        {
            return Activator.CreateInstance<T>();
        }

        private static void OpenConnection(IDbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            else if (conn.State != ConnectionState.Open)
            {
                throw new Exception("Unable to open connection. ConnectionState=" + conn.State);
            }
        }

        private static IDbCommand GetCommand(IDbConnection conn, string queryString, Dictionary<string, object> parameters)
        {
            if (conn is SqlConnection)
            {
                return GetSqlCommand((SqlConnection)conn, queryString, parameters);
            }

            throw new NotImplementedException("IDbConnection not recognised.");
        }

        private static SqlCommand GetSqlCommand(SqlConnection conn, string queryString, Dictionary<string, object> parameters)
        {
            var sqlCommand = new SqlCommand(queryString, conn);

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    sqlCommand.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }
            }

            return sqlCommand;
        }
    }
}
