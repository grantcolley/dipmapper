﻿//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace DevelopmentInProgress.DipMapper.Test
//{
//    [TestClass]
//    public class DipMapperMsSqlDataTest
//    {
//        private static string connectionString = "Data Source=(local);Initial Catalog=DipMapper;Integrated Security=true";

//        [ClassInitialize]
//        public static void ClassInitialise(TestContext testContext)
//        {
//            //ClassCleanup();
//            using (var conn = new SqlConnection(connectionString))
//            {
//                var createTable = new StringBuilder("CREATE TABLE [dbo].[Activity](");
//                createTable.Append("[Id] [int] IDENTITY(1,1) NOT NULL,");
//                createTable.Append("[Name] [varchar](50) NULL,");
//                createTable.Append("[Level] [float] NULL,");
//                createTable.Append("[IsActive] [bit] NULL,");
//                createTable.Append("[Created] [datetime] NOT NULL,");
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
//                ActivityType = ActivityTypeEnum.Public,
//            };

//            using (var conn = new SqlConnection(connectionString))
//            {
//                // Test Insert /////////////////////////////////////
//                // Act 
//                read = conn.Insert<Activity>(read, "Id");

//                write = conn.Insert<Activity>(write, "Id",
//                    new List<SqlParameter>()
//                    {
//                        new SqlParameter() {ParameterName = "Name", Value = write.Name},
//                        new SqlParameter() {ParameterName = "Level", Value = write.Level},
//                        new SqlParameter() {ParameterName = "IsActive", Value = write.IsActive},
//                        new SqlParameter() {ParameterName = "ActivityType", Value = write.ActivityType},
//                        new SqlParameter() {ParameterName = "Created", Value = write.Created}
//                    });

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
//                var activity1 = conn.Single<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = 2 } });
//                var activity2 = conn.Single<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = 3 } });
//                var activity3 = conn.Single<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = 1 } });

//                // Assert
//                Assert.AreEqual(activity1.Id, 2);
//                Assert.AreEqual(activity1.Name, "Write");
//                Assert.AreEqual(activity1.Level, 2);
//                Assert.AreEqual(activity1.IsActive, true);
//                Assert.AreEqual(activity1.Created, DateTime.Today.AddDays(1));
//                Assert.AreEqual(activity1.Updated, null);
//                Assert.AreEqual(activity1.ActivityType, ActivityTypeEnum.Private);

//                Assert.AreEqual(activity2.Id, 3);
//                Assert.AreEqual(activity2.Name, "Email");
//                Assert.AreEqual(activity2.Level, 3);
//                Assert.AreEqual(activity2.IsActive, false);
//                Assert.AreEqual(activity2.Created, DateTime.Today.AddDays(2));
//                Assert.AreEqual(activity2.Updated, null);
//                Assert.AreEqual(activity2.ActivityType, ActivityTypeEnum.Public);

//                Assert.AreEqual(activity3.Id, 1);
//                Assert.AreEqual(activity3.Name, "Read");
//                Assert.AreEqual(activity3.Level, 1);
//                Assert.AreEqual(activity3.IsActive, true);
//                Assert.AreEqual(activity3.Created, DateTime.Today);
//                Assert.AreEqual(activity3.Updated, DateTime.Today);
//                Assert.AreEqual(activity3.ActivityType, ActivityTypeEnum.Shared);
//                ////////////////////////////////////////////////////

//                // Single return none //////////////////////////////
//                // Act
//                var admin = conn.Single<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = 1000 } });

//                // Assert
//                Assert.IsNull(admin);
//                ////////////////////////////////////////////////////

//                // Test Select Many ////////////////////////////////
//                // Act
//                var activities = conn.Select<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "IsActive", Value = true } });

//                // Assert
//                Assert.AreEqual(activities.Count(), 2);
//                Assert.AreEqual(activities.ElementAt(0).Id, 1);
//                Assert.AreEqual(activities.ElementAt(0).Name, "Read");
//                Assert.AreEqual(activities.ElementAt(1).Id, 2);
//                Assert.AreEqual(activities.ElementAt(1).Name, "Write");
//                ////////////////////////////////////////////////////

//                // Select return none //////////////////////////////
//                // Act
//                var internals = conn.Select<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "ActivityType", Value = 1000 } });

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
//                activities = conn.ExecuteProcedure<Activity>("GetActivities", new List<SqlParameter>() { new SqlParameter() { ParameterName = "@IsActive", Value = true } });

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
//                conn.Update(read, new SqlParameter() { ParameterName = "Id", Value = 1 });

//                // Assert
//                var readOnly = conn.Single<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = 1 } });
//                Assert.AreEqual(readOnly.Name, "Read Only");
//                Assert.AreEqual(readOnly.Id, 1);
//                ////////////////////////////////////////////////////

//                // Update single /////////////////////////////////////
//                // Arrange 
//                readOnly.IsActive = false;
//                write.IsActive = false;

//                // Act
//                conn.Update<Activity>(readOnly, new List<SqlParameter>() { new SqlParameter() { ParameterName = "IsActive", Value = false } }, new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = readOnly.Id } });
//                conn.Update<Activity>(write, new List<SqlParameter>() { new SqlParameter() { ParameterName = "IsActive", Value = false } }, new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = write.Id } });

//                // Assert
//                var updated = conn.Select<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "IsActive", Value = false } });
//                Assert.AreEqual(updated.Count(), 3);
//                ////////////////////////////////////////////////////

//                // Delete single ///////////////////////////////////
//                // Act
//                conn.Delete<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = 1 } });

//                // Assert
//                readOnly = conn.Single<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = 1 } });
//                Assert.IsNull(readOnly);

//                activities = conn.Select<Activity>();
//                Assert.AreEqual(activities.Count(), 2);
//                ////////////////////////////////////////////////////

//                // Delete many /////////////////////////////////////
//                // Act
//                conn.Delete<Activity>(new List<SqlParameter>() { new SqlParameter() { ParameterName = "IsActive", Value = false } });

//                // Assert
//                activities = conn.Select<Activity>();
//                Assert.AreEqual(activities.Count(), 0);
//                ////////////////////////////////////////////////////
//            }
//        }

//        [TestMethod]
//        public void Transaction_Commit()
//        {
//            // Assert
//            var read = new Activity()
//            {
//                Name = "Read",
//                Level = 1,
//                IsActive = true,
//                Created = DateTime.Today,
//                Updated = DateTime.Today,
//                ActivityType = ActivityTypeEnum.Shared,
//            };

//            using (var conn = new SqlConnection(connectionString))
//            {
//                read = conn.Insert(read, "Id");
//            }

//            // Act
//            using (var conn = new SqlConnection(connectionString))
//            {
//                conn.Open();
//                var transaction = conn.BeginTransaction();

//                read.Name = "Read Only";

//                conn.Update(read,
//                    new List<SqlParameter>() { new SqlParameter() { ParameterName = "Name", Value = read.Name } },
//                    new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = read.Id } },
//                    transaction);

//                read.Level = 3;

//                conn.Update(read,
//                    new List<SqlParameter>() { new SqlParameter() { ParameterName = "Level", Value = read.Level } },
//                    new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = read.Id } },
//                    transaction);

//                transaction.Commit();
//            }

//            // Assert
//            Activity result;
//            using (var conn = new SqlConnection(connectionString))
//            {
//                result =
//                    conn.Single<Activity>(new List<SqlParameter>()
//                    {
//                        new SqlParameter() {ParameterName = "Id", Value = read.Id}
//                    });
//            }

//            Assert.AreEqual(result.Id, read.Id);
//            Assert.AreEqual(result.Name, "Read Only");
//            Assert.AreEqual(result.Level, 3);
//            Assert.AreEqual(result.Created, read.Created);
//            Assert.AreEqual(result.Updated, read.Updated);
//            Assert.AreEqual(result.ActivityType, read.ActivityType);
//        }

//        [TestMethod]
//        public void Transaction_Rollback()
//        {
//            // Assert
//            var read = new Activity()
//            {
//                Name = "Read",
//                Level = 1,
//                IsActive = true,
//                Created = DateTime.Today,
//                Updated = DateTime.Today,
//                ActivityType = ActivityTypeEnum.Shared,
//            };

//            using (var conn = new SqlConnection(connectionString))
//            {
//                read = conn.Insert(read, "Id");
//            }

//            // Act
//            using (var conn = new SqlConnection(connectionString))
//            {
//                conn.Open();
//                var transaction = conn.BeginTransaction();

//                try
//                {
//                    read.Name = "Read Only";

//                    conn.Update(read,
//                        new List<SqlParameter>() { new SqlParameter() { ParameterName = "Name", Value = read.Name } },
//                        new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = read.Id } },
//                        transaction);

//                    read.Level = 3;

//                    conn.Update(read,
//                        new List<SqlParameter>() { new SqlParameter() { ParameterName = "Level", Value = read.Level } },
//                        new List<SqlParameter>() { new SqlParameter() { ParameterName = "Id", Value = read.Id } },
//                        transaction);

//                    int i = 0;
//                    var e = 1 / i;

//                    transaction.Commit();
//                }
//                catch (Exception)
//                {
//                    transaction.Rollback();
//                }
//            }

//            // Assert
//            Activity result;
//            using (var conn = new SqlConnection(connectionString))
//            {
//                result =
//                    conn.Single<Activity>(new List<SqlParameter>()
//                    {
//                        new SqlParameter() {ParameterName = "Id", Value = read.Id}
//                    });
//            }

//            Assert.AreEqual(result.Id, read.Id);
//            Assert.AreEqual(result.Name, "Read");
//            Assert.AreEqual(result.Level, 1);
//            Assert.AreEqual(result.Created, read.Created);
//            Assert.AreEqual(result.Updated, read.Updated);
//            Assert.AreEqual(result.ActivityType, read.ActivityType);
//        }
//    }
//}
