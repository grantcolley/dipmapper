﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Oracle.ManagedDataAccess.Client;

//namespace DevelopmentInProgress.DipMapper.Test
//{
//    [TestClass]
//    public class DipMapperOracleDataTest
//    {
//        private static string connectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=XE)));User Id=authmanager;Password=authman;";

//        [ClassInitialize]
//        public static void ClassInitialise(TestContext testContext)
//        {
//            using (var conn = new OracleConnection(connectionString))
//            {
//                var createTable = "  CREATE TABLE \"AUTHMANAGER\".\"ACTIVITYORA\""
//                                  + "(	\"ID\" NUMBER(*,0) NOT NULL ENABLE, "
//                                  + "	\"NAME\" VARCHAR2(50 BYTE), "
//                                  + "	\"STATUS\" FLOAT(126), "
//                                  + "	\"ISACTIVE\" NUMBER(1,0), "
//                                  + "	\"CREATED\" TIMESTAMP (6), "
//                                  + "	\"UPDATED\" TIMESTAMP (6), "
//                                  + "	\"ACTIVITYTYPE\" NUMBER(1,0))";

//                conn.ExecuteNonQuery(createTable);

//                var createProc = "CREATE OR REPLACE PROCEDURE GETACTIVITIES "
//                                 + " ( "
//                                 + "   ISACTIVE IN NUMBER "
//                                 + " ) AS "
//                                 + " BEGIN "
//                                 + "   select ID , "
//                                 + " NAME , "
//                                 + " STATUS , "
//                                 + " ISACTIVE , "
//                                 + " CREATED , "
//                                 + " UPDATED , "
//                                 + " ACTIVITYTYPE  from ACTIVITYORA where IsActive = ISACTIVE;  "
//                                 + " END GETACTIVITIES;";

//                conn.ExecuteNonQuery(createProc.ToString());
//            }
//        }

//        [ClassCleanup]
//        public static void ClassCleanup()
//        {
//            using (var conn = new OracleConnection(connectionString))
//            {
//                conn.ExecuteNonQuery("DROP TABLE \"AUTHMANAGER\".\"ACTIVITYORA\";");
//                conn.ExecuteNonQuery("DROP PROCEDURE GetActivities;");
//            }
//        }

//        [TestMethod]
//        public void DipMapper_Database_Test()
//        {
//            // Arrange
//            var read = new ActivityOra()
//            {
//                Name = "Read",
//                Status = 1,
//                IsActive = true,
//                Created = DateTime.Today,
//                Updated = DateTime.Today,
//                ActivityType = ActivityTypeEnum.Shared,
//            };

//            var write = new ActivityOra()
//            {
//                Name = "Write",
//                Status = 2,
//                IsActive = true,
//                Created = DateTime.Today.AddDays(1),
//                Updated = DateTime.Today.AddDays(1),
//                ActivityType = ActivityTypeEnum.Private,
//            };

//            var email = new ActivityOra()
//            {
//                Name = "Email",
//                Status = 3,
//                IsActive = false,
//                Created = DateTime.Today.AddDays(2),
//                Updated = null,
//                ActivityType = ActivityTypeEnum.Public,
//            };

//            using (var conn = new OracleConnection(connectionString))
//            {
//                // Test Insert /////////////////////////////////////
//                // Act 
//                read = conn.Insert<ActivityOra>(read, "Id", "ActivityOra_seq");
//                write = conn.Insert<ActivityOra>(write, "Id", "ActivityOra_seq");
//                email = conn.Insert<ActivityOra>(email, "Id", "ActivityOra_seq");

//                // Assert
//                Assert.AreEqual(read.Id, 1);
//                Assert.AreEqual(read.Name, "Read");
//                Assert.AreEqual(write.Id, 2);
//                Assert.AreEqual(write.Name, "Write");
//                Assert.AreEqual(email.Id, 3);
//                Assert.AreEqual(email.Name, "Email");
//                ////////////////////////////////////////////////////

//                // Test Select Single //////////////////////////////
//                // Act
//                var activity = conn.Single<ActivityOra>(new Dictionary<string, object>() { { "Id", 2 } });

//                // Assert
//                Assert.AreEqual(activity.Name, "Write");
//                ////////////////////////////////////////////////////

//                // Single return none //////////////////////////////
//                // Act
//                var admin = conn.Single<ActivityOra>(new Dictionary<string, object>() { { "Id", 1000 } });

//                // Assert
//                Assert.IsNull(admin);
//                ////////////////////////////////////////////////////

//                // Test Select Many ////////////////////////////////
//                // Act
//                var activities = conn.Select<ActivityOra>(new Dictionary<string, object>() { { "IsActive", true } });

//                // Assert
//                Assert.AreEqual(activities.Count(), 2);
//                Assert.AreEqual(activities.ElementAt(0).Id, 1);
//                Assert.AreEqual(activities.ElementAt(0).Name, "Read");
//                Assert.AreEqual(activities.ElementAt(1).Id, 2);
//                Assert.AreEqual(activities.ElementAt(1).Name, "Write");
//                ////////////////////////////////////////////////////

//                // Select return none //////////////////////////////
//                // Act
//                var internals = conn.Select<ActivityOra>(new Dictionary<string, object>() { { "ActivityType", 100 } });

//                // Assert
//                Assert.AreEqual(internals.Count(), 0);
//                ////////////////////////////////////////////////////

//                // ExecuteSql //////////////////////////////////////
//                // Arrange
//                activities = null;
//                var sql = "SELECT * FROM ActivityOra WHERE IsActive = 1;";

//                // Act 
//                activities = conn.ExecuteSql<ActivityOra>(sql);

//                // Assert
//                Assert.AreEqual(activities.Count(), 2);
//                Assert.AreEqual(activities.ElementAt(0).Id, 1);
//                Assert.AreEqual(activities.ElementAt(0).Name, "Read");
//                Assert.AreEqual(activities.ElementAt(1).Id, 2);
//                Assert.AreEqual(activities.ElementAt(1).Name, "Write");
//                ////////////////////////////////////////////////////

//                // ExecuteProcedure ////////////////////////////////
//                // Arrange
//                activities = null;

//                // Act 
//                activities = conn.ExecuteProcedure<ActivityOra>("GetActivities", new Dictionary<string, object>() { { "@IsActive", true } });

//                // Assert
//                Assert.AreEqual(activities.Count(), 2);
//                Assert.AreEqual(activities.ElementAt(0).Id, 1);
//                Assert.AreEqual(activities.ElementAt(0).Name, "Read");
//                Assert.AreEqual(activities.ElementAt(1).Id, 2);
//                Assert.AreEqual(activities.ElementAt(1).Name, "Write");
//                ////////////////////////////////////////////////////

//                // ExecuteScalar ///////////////////////////////////
//                // Act
//                var result = conn.ExecuteScalar("SELECT Name FROM ActivityOra WHERE Id = 2");

//                // Assert
//                Assert.AreEqual(result, "Write");
//                ////////////////////////////////////////////////////

//                // Update single ///////////////////////////////////
//                // Arrange
//                read.Name = "Read Only";

//                // Act
//                conn.Update(read, new Dictionary<string, object>() { { "Id", 1 } }, new[] { "Id" });

//                // Assert
//                var readOnly = conn.Single<ActivityOra>(new Dictionary<string, object>() { { "Id", 1 } });
//                Assert.AreEqual(readOnly.Name, "Read Only");
//                Assert.AreEqual(readOnly.Id, 1);
//                ////////////////////////////////////////////////////

//                // Update many /////////////////////////////////////
//                // Arrange 
//                readOnly.IsActive = false;

//                // Act
//                conn.Update<ActivityOra>(readOnly, null, new[] { "Id" });

//                // Assert
//                var updated = conn.Select<ActivityOra>(new Dictionary<string, object>() { { "IsActive", false } });
//                Assert.AreEqual(updated.Count(), 3);
//                ////////////////////////////////////////////////////

//                // Delete single ///////////////////////////////////
//                // Act
//                conn.Delete<ActivityOra>(new Dictionary<string, object>() { { "Id", 1 } });

//                // Assert
//                readOnly = conn.Single<ActivityOra>(new Dictionary<string, object>() { { "Id", 1 } });
//                Assert.IsNull(readOnly);
//                ////////////////////////////////////////////////////

//                // Delete many /////////////////////////////////////
//                // Act
//                conn.Delete<ActivityOra>(new Dictionary<string, object>() { { "IsActive", false } });

//                // Assert
//                activities = conn.Select<ActivityOra>();
//                Assert.AreEqual(activities.Count(), 0);
//                ////////////////////////////////////////////////////
//            }
//        }
//    }
//}