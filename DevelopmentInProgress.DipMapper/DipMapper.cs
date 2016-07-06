using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

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
            string select = "SELECT ";
            var propertyInfos = typeof(T).GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                // Skip non-public properties and properties that are either 
                // classes (but not strings), interfaces, lists, generic 
                // lists or arrays.
                var propertyType = propertyInfo.PropertyType;
                if (propertyType != typeof (string)
                    && (propertyType.IsClass
                        || propertyType.IsInterface
                        || propertyType.IsArray
                        || propertyType.GetInterfaces()
                            .Any(
                                i =>
                                    (i.GetTypeInfo().Name.Equals(typeof (IEnumerable).Name)
                                     || (i.IsGenericType &&
                                         i.GetGenericTypeDefinition().Name.Equals(typeof (IEnumerable<>).Name))))))
                {
                    continue;
                }

                select += propertyInfo.Name + ", ";
            }

            if (select.EndsWith(", "))
            {
                select = select.Remove(select.Length - 2, 2);
            }

            if (typeof(T).IsGenericType
                && typeof(T).GenericTypeArguments.Any())
            {
                select += " FROM " + typeof(T).GenericTypeArguments[0].Name;
            }
            else
            {
                select += " FROM " + typeof (T).Name;
            }

            return select;
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
                where += parameter.Key + "=" + SqlConvert(parameter.Value) + " AND ";
            }

            if (where.EndsWith(" AND "))
            {
                where = where.Remove(where.Length - 5, 5);
            }

            return where;
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
