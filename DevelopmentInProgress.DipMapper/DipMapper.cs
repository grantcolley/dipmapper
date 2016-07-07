using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace DevelopmentInProgress.DipMapper
{
    public static class DipMapper
    {
        public static T Single<T>(this IDbConnection conn, Dictionary<string, object> paramaters = null, CommandType commandType = CommandType.TableDirect, string storedProcedure = "")
        {
            var target = Activator.CreateInstance<T>();

            var sql = GetCommandText<T>(commandType, paramaters);

            OpenConnection(conn);

            return target;
        }

        public static IEnumerable<T> Select<T>(this IDbConnection conn)
        {
            OpenConnection(conn);

            var results = new List<T>();

            return results;
        }

        public static string Insert<T>(this IDbConnection conn)
        {
            OpenConnection(conn);


        }

        public static void OpenConnection(IDbConnection conn)
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

        public static string GetCommandText<T>(CommandType commandType, Dictionary<string, object> paramaters)
        {
            if (commandType == CommandType.StoredProcedure)
            {
                return GetStoredProcedureParameters(paramaters);
            }

            return GetSelectSql<T>() + GetWhereSql(paramaters);
        }

        private static string GetStoredProcedureParameters(Dictionary<string, object> paramaters)
        {
            throw new NotImplementedException();
        }

        public static string GetSelectSql<T>()
        {
            string fields = GetFields<T>();

            if (typeof (T).IsGenericType
                && typeof (T).GenericTypeArguments.Any())
            {
                return "SELECT " + fields + " FROM " + typeof (T).GenericTypeArguments[0].Name;
            }

            return "SELECT " + fields + " FROM " + typeof (T).Name;
        }

        private static string GetFields<T>(IEnumerable<string> ignore = null)
        {
            string fields = string.Empty;

            var propertyInfos = typeof(T).GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                if (ignore != null
                    && ignore.Contains(propertyInfo.Name))
                {
                    continue;
                }

                // Skip non-public properties and properties that are either 
                // classes (but not strings), interfaces, lists, generic 
                // lists or arrays.
                var propertyType = propertyInfo.PropertyType;
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
                    continue;
                }

                fields += propertyInfo.Name + ", ";
            }

            if (fields.EndsWith(", "))
            {
                fields = fields.Remove(fields.Length - 2, 2);
            }

            return fields;
        }

        public static string GetWhereSql(Dictionary<string, object> parameters)
        {
            if (parameters == null
                || !parameters.Any())
            {
                return string.Empty;
            }

            string where = "WHERE ";

            foreach (var parameter in parameters)
            {
                where += parameter.Key + WhereValue(parameter.Value) + " AND ";
            }

            if (where.EndsWith(" AND "))
            {
                where = where.Remove(where.Length - 5, 5);
            }

            return where;
        }

        private static string WhereValue(object value)
        {
            var result = SqlConvert(value);
            if (result == "null")
            {
                return " is " + result;
            }

            return "=" + result;
        }

        private static string SqlConvert(object value)
        {
            if (value == null)
            {
                return "null";
            }

            switch (value.GetType().Name)
            {
                case "String":
                    if (string.IsNullOrEmpty(value.ToString()))
                    {
                        return "null";
                    }

                    return "'" + value + "'";

                case "Boolean":
                    return (bool) value ? "1" : "0";

                case "DateTime":
                    return "'" + ((DateTime) value).Date + "'";

                default:
                    if (value.GetType().IsEnum)
                    {
                        return Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())).ToString();
                    }

                    return value.ToString();
            }
        }
    }
}
