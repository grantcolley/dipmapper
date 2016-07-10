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
            var sql = GetSqlSelect<T>(paramaters);
            OpenConnection(conn);
            throw new NotImplementedException();
        }

        public static T Insert<T>(this IDbConnection conn, T target, IEnumerable<string> identityFields)
        {
            var sql = GetSqlInsert<T>(target, identityFields);
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

        internal static string GetSqlSelect<T>(Dictionary<string, object> paramaters = null)
        {
            return "SELECT " + GetSqlSelectFields<T>() + " FROM " + GetSqlTableName<T>() + GetSqlWhereClause(paramaters) + ";";
        }

        internal static string GetSqlInsert<T>(T target, IEnumerable<string> identityFields)
        {
            return "INSERT INTO " + GetSqlTableName<T>() + GetSqlInsertFields(target, identityFields) + ";";
        }

        internal static string GetSqlUpdate<T>(T target, Dictionary<string, object> parameters)
        {
            IEnumerable<string> ignore = null;

            if (parameters != null)
            {
                ignore = parameters.Keys;
            }

            return "UPDATE " + GetSqlTableName<T>() + " SET " + GetSqlUpdateFields<T>(target, ignore) + GetSqlWhereClause(parameters) + ";";
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
                where += parameter.Key + GetSqlWhereAssignment(parameter.Value) + " AND ";
            }

            if (where.EndsWith(" AND "))
            {
                where = where.Remove(where.Length - 5, 5);
            }

            return where;
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

        internal static string GetSqlSelectFields<T>()
        {
            string fields = string.Empty;
            PropertyInfo[] propertyInfos = typeof (T).GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                if (SkipProperty(propertyInfo))
                {
                    continue;
                }

                fields += propertyInfo.Name + ", ";
            }

            fields = fields.Remove(fields.Length - 2, 2);
            return fields;
        }

        internal static string GetSqlUpdateFields<T>(T target, IEnumerable<string> ignore)
        {
            string fields = string.Empty;
            PropertyInfo[] propertyInfos = target.GetType().GetProperties();

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

                fields += propertyInfo.Name + GetSqlUpdateAssignment(propertyInfo.GetValue(target)) + ", ";
            }

            fields = fields.Remove(fields.Length - 2, 2);
            return fields;
        }

        internal static string GetSqlInsertFields<T>(T target, IEnumerable<string> identityFields)
        {
            string fields = string.Empty;
            string values = string.Empty;
            PropertyInfo[] propertyInfos = target.GetType().GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                if (identityFields.Contains(propertyInfo.Name))
                {
                    continue;
                }

                if (SkipProperty(propertyInfo))
                {
                    continue;
                }

                fields += propertyInfo.Name + ", ";
                values += SqlConvert(propertyInfo.GetValue(target)) + ", ";
            }

            fields = fields.Remove(fields.Length - 2, 2);
            values = values.Remove(values.Length - 2, 2);

            return " (" + fields + ") VALUES (" + values + ")";
        }

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

        private static string GetSqlWhereAssignment(object value)
        {
            var result = SqlConvert(value);
            if (result == "null")
            {
                return " is " + result;
            }

            return "=" + result;
        }

        private static string GetSqlUpdateAssignment(object value)
        {
            return "=" + SqlConvert(value);
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
