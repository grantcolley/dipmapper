using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DevelopmentInProgress.DipMapper.Test")]

namespace DevelopmentInProgress.DipMapper
{
    public static class DipMapper
    {
        public static T Single<T>(this IDbConnection conn, Dictionary<string, object> parameters = null) where T : new()
        {
            return Select<T>(conn, parameters).Single();
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

                    reader.Close();
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

        public static T Insert<T>(this IDbConnection conn, T target, IEnumerable<string> identityFields)
        {
            var propertyInfos = GetPropertyInfos<T>(identityFields);
            var sql = GetSqlInsert<T>(propertyInfos);
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        public static T Update<T>(this IDbConnection conn, T target, Dictionary<string, object> parameters = null)
        {
            IEnumerable<string> ignore = null;
            if (parameters != null)
            {
                ignore = parameters.Keys;
            }

            var propertyInfos = GetPropertyInfos<T>(ignore);
            var sql = GetSqlUpdate<T>(propertyInfos, parameters);
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        public static void Delete<T>(this IDbConnection conn, Dictionary<string, object> parameters = null)
        {
            var sql = GetSqlDelete<T>(parameters);
            OpenConnection(conn);
            throw new NotImplementedException();
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

        internal static string GetSqlInsert<T>(IEnumerable<PropertyInfo> propertyInfos)
        {
            return "INSERT INTO " + GetSqlTableName<T>() + GetSqlInsertFields(propertyInfos) + ";";
        }

        internal static string GetSqlUpdate<T>(IEnumerable<PropertyInfo> propertyInfos, Dictionary<string, object> parameters)
        {
            return "UPDATE " + GetSqlTableName<T>() + " SET " + GetSqlUpdateFields(propertyInfos) + GetSqlWhereClause(parameters) + ";";
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

        internal static IEnumerable<PropertyInfo> GetPropertyInfos<T>(IEnumerable<string> ignore = null)
        {
            if (ignore == null)
            {
                ignore = new List<string>();
            }

            var propertyInfoResults = new List<PropertyInfo>();
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                if (ignore.Contains(propertyInfo.Name))
                {
                    continue;
                }

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

        internal static string GetSqlUpdateFields(IEnumerable<PropertyInfo> propertyInfos)
        {
            string fields = string.Empty;

            foreach (var propertyInfo in propertyInfos)
            {
                fields += propertyInfo.Name + "=@" + propertyInfo.Name + ", ";
            }

            fields = fields.Remove(fields.Length - 2, 2);
            return fields;
        }

        internal static string GetSqlInsertFields(IEnumerable<PropertyInfo> propertyInfos)
        {
            string fields = string.Empty;
            string parameters = string.Empty;
            
            foreach (var propertyInfo in propertyInfos)
            {
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
            foreach (var kvp in parameters)
            {
                sqlCommand.Parameters.AddWithValue(kvp.Key, kvp.Value);
            }

            return sqlCommand;
        }
    }
}
