using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DevelopmentInProgress.DipMapper.Test")]

namespace DevelopmentInProgress.DipMapper
{
    public static class DipMapper
    {
        public static T Single<T>(this IDbConnection conn, Dictionary<string, object> paramaters = null)
        {
            return Select<T>(conn, paramaters).FirstOrDefault();
        }

        public static IEnumerable<T> Select<T>(this IDbConnection conn, Dictionary<string, object> paramaters = null)
        {
            var sql = GetSqlSelect<T>() + GetSqlWhereClause(paramaters);
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        public static T Insert<T>(this IDbConnection conn, T target, string IdentityField)
        {
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        public static T Update<T>(this IDbConnection conn, T target, Dictionary<string, object> paramaters = null)
        {
            var sql = GetSqlUpdate<T>(target, paramaters);
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        public static void Delete<T>(this IDbConnection conn, Dictionary<string, object> paramaters = null)
        {
            var sql = GetSqlDelete<T>(paramaters);
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        public static T ExecuteScalar<T>(this IDbConnection conn, Dictionary<string, object> paramaters = null, CommandType commandType = CommandType.TableDirect, string sql = "")
        {
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        public static IEnumerable<T> Execute<T>(this IDbConnection conn, Dictionary<string, object> paramaters = null, CommandType commandType = CommandType.TableDirect, string sql = "")
        {
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        internal static string GetSqlSelect<T>()
        {
            return "SELECT " + GetFields<T>() + " FROM " + GetTableName<T>();
        }

        internal static string GetSqlUpdate<T>(T target, Dictionary<string, object> parameters)
        {
            IEnumerable<string> ignore = null;

            if (parameters != null)
            {
                ignore = parameters.Keys;
            }

            return "UPDATE " + GetTableName<T>() + " SET " 
                + GetFields<T>(target, ignore) + GetSqlWhereClause(parameters);
        }

        internal static string GetSqlDelete<T>(Dictionary<string, object> parameters)
        {
            return "DELETE FROM " + GetTableName<T>() + GetSqlWhereClause(parameters);
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
                where += parameter.Key + GetSqlWhereAssignment(parameter.Value) + " AND ";
            }

            if (where.EndsWith(" AND "))
            {
                where = where.Remove(where.Length - 5, 5);
            }

            return where;
        }

        internal static string GetTableName<T>()
        {
            if (typeof (T).IsGenericType
                && typeof (T).GenericTypeArguments.Any())
            {
                return typeof (T).GenericTypeArguments[0].Name;
            }

            return typeof (T).Name;
        }

        internal static string GetFields<T>(T target = default(T), IEnumerable<string> ignore = null)
        {
            bool isUpdate = target != null;
            string fields = string.Empty;
            PropertyInfo[] propertyInfos;

            if (isUpdate)
            {
                propertyInfos = target.GetType().GetProperties(BindingFlags.Public);
            }
            else
            {
                propertyInfos = typeof(T).GetProperties(BindingFlags.Public);
            }

            foreach (var propertyInfo in propertyInfos)
            {
                if (isUpdate
                    && ignore != null
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

                if (isUpdate)
                {
                    fields += propertyInfo.Name + GetSqlUpdateAssignment(propertyInfo.GetValue(target)) + ", ";
                }
                else
                {
                    fields += propertyInfo.Name + ", ";
                }
            }

            if (fields.EndsWith(", "))
            {
                fields = fields.Remove(fields.Length - 2, 2);
            }

            return fields;
        }

        internal static string GetSqlWhereAssignment(object value)
        {
            var result = SqlConvert(value);
            if (result == "null")
            {
                return " is " + result;
            }

            return "=" + result;
        }

        internal static string GetSqlUpdateAssignment(object value)
        {
            return "=" + SqlConvert(value);
        }

        internal static string SqlConvert(object value)
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

        //private static string GetCommandText<T>(CommandType commandType, Dictionary<string, object> paramaters)
        //{
        //    if (commandType == CommandType.StoredProcedure)
        //    {
        //        return GetStoredProcedureParameters(paramaters);
        //    }

        //    return GetSelectSql<T>() + GetWhereSql(paramaters);
        //}

        //private static string GetStoredProcedureParameters(Dictionary<string, object> paramaters)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
