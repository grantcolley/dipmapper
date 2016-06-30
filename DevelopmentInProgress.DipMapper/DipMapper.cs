using System;
using System.Collections.Generic;
using System.Data;

namespace DevelopmentInProgress.DipMapper
{
    public static class DipMapper
    {
        public static T Single<T>(this IDbConnection conn, object paramaters = null)
        {
            OpenConnection(conn);

            var result = Activator.CreateInstance<T>();

            return result;
        }

        public static IEnumerable<T> Select<T>(this IDbConnection conn)
        {
            OpenConnection(conn);

            var results = new List<T>();

            return results;
        }

        private static void OpenConnection(IDbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            else if (conn.State != ConnectionState.Open)
            {
                throw new Exception("Unable to open connection : ConnectionState=" + conn.State);
            }
        }
    }
}
