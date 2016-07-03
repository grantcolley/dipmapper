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

            return "SELECT " + GetSelectFields<T>() + " FROM " + typeof (T).Name + GetSqlWhereClause(paramaters);
        }

        private static string GetStoredProcedureParameters(Dictionary<string, object> paramaters)
        {
            throw new NotImplementedException();
        }

        public static string GetSelectFields<T>()
        {
            string fields = string.Empty;
            var propertyInfos = typeof(T).GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.PropertyType != typeof (string) &&
                    propertyInfo.PropertyType.GetInterfaces()
                        .Any(
                            i =>
                                (i.IsGenericType &&
                                 i.GetGenericTypeDefinition().Name.Equals(typeof (IEnumerable<>).Name)) ||
                                i.GetTypeInfo().Name.Equals(typeof (IEnumerable).Name)))
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

        public static string GetSqlWhereClause(Dictionary<string, object> parameters)
        {
            return string.Empty;
        }
    }
}
