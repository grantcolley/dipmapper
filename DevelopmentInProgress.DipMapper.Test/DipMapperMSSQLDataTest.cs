﻿//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace DevelopmentInProgress.DipMapper.Test
//{
//    [TestClass]
//    public class DipMapperMSSQLDataTest
//    {
//        private static string connectionString = "Data Source=(local);Initial Catalog=DipMapper;Integrated Security=true";

//        [ClassInitialize]
//        public static void ClassInitialise(TestContext testContext)
//        {
//            using (var conn = new SqlConnection(connectionString))
//            {
//                var createTable = new StringBuilder("CREATE TABLE [dbo].[Activity](");
//                createTable.Append("[Id] [int] IDENTITY(1,1) NOT NULL,");
//                createTable.Append("[Name] [varchar](50) NULL,");
//                createTable.Append("[Level] [float] NULL,");
//                createTable.Append("[IsActive] [bit] NULL,");
//                createTable.Append("[Created] [datetime] NULL,");
//                createTable.Append("[Updated] [datetime] NULL,");
//                createTable.Append("[ActivityType] [int] NULL)");

//                conn.ExecuteNonQuery(createTable.ToString());

//                var createProc = new StringBuilder("CREATE PROCEDURE GetActivities");
//                createProc.Append(" @IsActive bit");
//                createProc.Append(" AS");
//                createProc.Append(" BEGIN");
//                createProc.Append(" SELECT * from Activity WHERE IsActive = @IsActive;");
//                createProc.Append(" END");

//                conn.ExecuteNonQuery(createProc.ToString());
//            }
//        }

//        [ClassCleanup]
//        public static void ClassCleanup()
//        {
//            using (var conn = new SqlConnection(connectionString))
//            {
//                conn.ExecuteNonQuery("DROP TABLE Activity;");
//                conn.ExecuteNonQuery("DROP PROCEDURE GetActivities;");
//            }
//        }

//        [TestMethod]
//        public void DipMapper_Database_Test()
//        {
//            // Arrange
//            var read = new Activity()
//            {
//                Name = "Read",
//                Level = 1,
//                IsActive = true,
//                Created = DateTime.Today,
//                Updated = DateTime.Today,
//                ActivityType = ActivityTypeEnum.Shared,
//            };

//            var write = new Activity()
//            {
//                Name = "Write",
//                Level = 2,
//                IsActive = true,
//                Created = DateTime.Today.AddDays(1),
//                Updated = DateTime.Today.AddDays(1),
//                ActivityType = ActivityTypeEnum.Private,
//            };

//            var email = new Activity()
//            {
//                Name = "Email",
//                Level = 3,
//                IsActive = false,
//                Created = DateTime.Today.AddDays(2),
//                Updated = null,
//                ActivityType = ActivityTypeEnum.Public,
//            };

//            using (var conn = new SqlConnection(connectionString))
//            {
//                // Test Insert /////////////////////////////////////
//                // Act 
//                read = conn.Insert<Activity>(read, "Id");
//                write = conn.Insert<Activity>(write, "Id");
//                email = conn.Insert<Activity>(email, "Id");

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
//                var activity = conn.Single<Activity>(new Dictionary<string, object>() { { "Id", 2 } });

//                // Assert
//                Assert.AreEqual(activity.Name, "Write");
//                ////////////////////////////////////////////////////

//                // Single return none //////////////////////////////
//                // Act
//                var admin = conn.Single<Activity>(new Dictionary<string, object>() { { "Id", 1000 } });

//                // Assert
//                Assert.IsNull(admin);
//                ////////////////////////////////////////////////////

//                // Test Select Many ////////////////////////////////
//                // Act
//                var activities = conn.Select<Activity>(new Dictionary<string, object>() { { "IsActive", true } });

//                // Assert
//                Assert.AreEqual(activities.Count(), 2);
//                Assert.AreEqual(activities.ElementAt(0).Id, 1);
//                Assert.AreEqual(activities.ElementAt(0).Name, "Read");
//                Assert.AreEqual(activities.ElementAt(1).Id, 2);
//                Assert.AreEqual(activities.ElementAt(1).Name, "Write");
//                ////////////////////////////////////////////////////

//                // Select return none //////////////////////////////
//                // Act
//                var internals = conn.Select<Activity>(new Dictionary<string, object>() { { "ActivityType", 100 } });

//                // Assert
//                Assert.AreEqual(internals.Count(), 0);
//                ////////////////////////////////////////////////////

//                // ExecuteSql //////////////////////////////////////
//                // Arrange
//                activities = null;
//                var sql = "SELECT * FROM Activity WHERE IsActive = 1;";

//                // Act 
//                activities = conn.ExecuteSql<Activity>(sql);

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
//                activities = conn.ExecuteProcedure<Activity>("GetActivities", new Dictionary<string, object>() { { "@IsActive", true } });

//                // Assert
//                Assert.AreEqual(activities.Count(), 2);
//                Assert.AreEqual(activities.ElementAt(0).Id, 1);
//                Assert.AreEqual(activities.ElementAt(0).Name, "Read");
//                Assert.AreEqual(activities.ElementAt(1).Id, 2);
//                Assert.AreEqual(activities.ElementAt(1).Name, "Write");
//                ////////////////////////////////////////////////////

//                // ExecuteScalar ///////////////////////////////////
//                // Act
//                var result = conn.ExecuteScalar("SELECT Name FROM Activity WHERE Id = 2");

//                // Assert
//                Assert.AreEqual(result, "Write");
//                ////////////////////////////////////////////////////

//                // Update single ///////////////////////////////////
//                // Arrange
//                read.Name = "Read Only";

//                // Act
//                conn.Update(read, new Dictionary<string, object>() { { "Id", 1 } }, new[] { "Id" });

//                // Assert
//                var readOnly = conn.Single<Activity>(new Dictionary<string, object>() { { "Id", 1 } });
//                Assert.AreEqual(readOnly.Name, "Read Only");
//                Assert.AreEqual(readOnly.Id, 1);
//                ////////////////////////////////////////////////////

//                // Update many /////////////////////////////////////
//                // Arrange 
//                readOnly.IsActive = false;

//                // Act
//                conn.Update<Activity>(readOnly, null, new[] { "Id" });

//                // Assert
//                var updated = conn.Select<Activity>(new Dictionary<string, object>() { { "IsActive", false } });
//                Assert.AreEqual(updated.Count(), 3);
//                ////////////////////////////////////////////////////

//                // Delete single ///////////////////////////////////
//                // Act
//                conn.Delete<Activity>(new Dictionary<string, object>() { { "Id", 1 } });

//                // Assert
//                readOnly = conn.Single<Activity>(new Dictionary<string, object>() { { "Id", 1 } });
//                Assert.IsNull(readOnly);
//                ////////////////////////////////////////////////////

//                // Delete many /////////////////////////////////////
//                // Act
//                conn.Delete<Activity>(new Dictionary<string, object>() { { "IsActive", false } });

//                // Assert
//                activities = conn.Select<Activity>();
//                Assert.AreEqual(activities.Count(), 0);
//                ////////////////////////////////////////////////////
//            }
//        }
//    }
//}