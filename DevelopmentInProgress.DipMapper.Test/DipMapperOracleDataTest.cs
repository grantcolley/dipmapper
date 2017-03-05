//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Oracle.ManagedDataAccess.Client;

//namespace DevelopmentInProgress.DipMapper.Test
//{
//    [TestClass]
//    public class DipMapperOracleDataTest
//    {
//        private static string connectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=XE)));User Id=dipmapper;Password=dipmapper;";

//        [ClassInitialize]
//        public static void ClassInitialise(TestContext testContext)
//        {
//            using (var conn = new OracleConnection(connectionString))
//            {
//                var createTable = "  CREATE TABLE \"DIPMAPPER\".\"ACTIVITYORA\""
//                                  + "(	\"ID\" NUMBER(*,0) NOT NULL ENABLE, "
//                                  + "	\"NAME\" VARCHAR2(50 BYTE), "
//                                  + "	\"STATUS\" FLOAT(126), "
//                                  + "	\"ISACTIVE\" NUMBER(1,0), "
//                                  + "	\"CREATED\" TIMESTAMP (6) NOT NULL, "
//                                  + "	\"UPDATED\" TIMESTAMP (6), "
//                                  + "	\"ACTIVITYTYPE\" NUMBER(1,0))";

//                conn.ExecuteNonQuery(createTable);

//                var createSequence = "CREATE SEQUENCE \"DIPMAPPER\".\"ActivityOra_seq\""
//                                     + "  MINVALUE 1 "
//                                     + "  START WITH 1 "
//                                     + "  INCREMENT BY 1 "
//                                     + "  NOCACHE";

//                conn.ExecuteNonQuery(createSequence);

//                var createProc = "CREATE OR REPLACE PROCEDURE \"DIPMAPPER\".\"GETACTIVITIES\""
//                                 + " ( "
//                                 + "   pIsActive IN NUMBER, "
//                                 + "   pCursor OUT SYS_REFCURSOR "
//                                 + " ) AS "
//                                 + " BEGIN "
//                                 + " open pCursor for "
//                                 + "   select ID , "
//                                 + " NAME , "
//                                 + " STATUS , "
//                                 + " ISACTIVE , "
//                                 + " CREATED , "
//                                 + " UPDATED , "
//                                 + " ACTIVITYTYPE  from ACTIVITYORA where IsActive = pIsActive;  "
//                                 + " END GETACTIVITIES;";

//                conn.ExecuteNonQuery(createProc.ToString());
//            }
//        }

//        [ClassCleanup]
//        public static void ClassCleanup()
//        {
//            using (var conn = new OracleConnection(connectionString))
//            {
//                conn.ExecuteNonQuery("DROP TABLE \"DIPMAPPER\".\"ACTIVITYORA\"");
//                conn.ExecuteNonQuery("DROP SEQUENCE \"DIPMAPPER\".\"ActivityOra_seq\"");
//                conn.ExecuteNonQuery("DROP PROCEDURE \"DIPMAPPER\".\"GETACTIVITIES\"");
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
//                Created = DateTime.Today.AddDays(2),
//                Updated = null,
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
//                read.Id = Convert.ToInt32(conn.ExecuteScalar("SELECT \"ActivityOra_seq\".NEXTVAL FROM DUAL"));
//                read = conn.Insert<ActivityOra>(read);

//                write.Id = Convert.ToInt32(conn.ExecuteScalar("SELECT \"ActivityOra_seq\".NEXTVAL FROM DUAL"));
//                write = conn.Insert<ActivityOra>(write,
//                    new List<OracleParameter>()
//                    {
//                        new OracleParameter() {ParameterName = "Id", Value = write.Id},
//                        new OracleParameter() {ParameterName = "Name", Value = write.Name},
//                        new OracleParameter() {ParameterName = "Status", Value = write.Status},
//                        new OracleParameter() {ParameterName = "IsActive", Value = Convert.ToInt32(write.IsActive)},
//                        new OracleParameter() {ParameterName = "Created", Value = write.Created},
//                        new OracleParameter() {ParameterName = "ActivityType", Value = Convert.ToInt32(write.ActivityType)
//                        }
//                    });

//                email.Id = Convert.ToInt32(conn.ExecuteScalar("SELECT \"ActivityOra_seq\".NEXTVAL FROM DUAL"));
//                email = conn.Insert<ActivityOra>(email);

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
//                var activity1 = conn.Single<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = 2 } });
//                var activity2 = conn.Single<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = 3 } });
//                var activity3 = conn.Single<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = 1 } });

//                // Assert
//                Assert.AreEqual(activity1.Id, 2);
//                Assert.AreEqual(activity1.Name, "Write");
//                Assert.AreEqual(activity1.Status, 2);
//                Assert.AreEqual(activity1.IsActive, true);
//                Assert.AreEqual(activity1.Created, DateTime.Today.AddDays(2));
//                Assert.AreEqual(activity1.Updated, null);
//                Assert.AreEqual(activity1.ActivityType, ActivityTypeEnum.Private);

//                Assert.AreEqual(activity2.Id, 3);
//                Assert.AreEqual(activity2.Name, "Email");
//                Assert.AreEqual(activity2.Status, 3);
//                Assert.AreEqual(activity2.IsActive, false);
//                Assert.AreEqual(activity2.Created, DateTime.Today.AddDays(2));
//                Assert.AreEqual(activity2.Updated, null);
//                Assert.AreEqual(activity2.ActivityType, ActivityTypeEnum.Public);

//                Assert.AreEqual(activity3.Id, 1);
//                Assert.AreEqual(activity3.Name, "Read");
//                Assert.AreEqual(activity3.Status, 1);
//                Assert.AreEqual(activity3.IsActive, true);
//                Assert.AreEqual(activity3.Created, DateTime.Today);
//                Assert.AreEqual(activity3.Updated, DateTime.Today);
//                Assert.AreEqual(activity3.ActivityType, ActivityTypeEnum.Shared);
//                ////////////////////////////////////////////////////

//                // Single return none //////////////////////////////
//                // Act
//                var admin = conn.Single<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = 1000 } });

//                // Assert
//                Assert.IsNull(admin);
//                ////////////////////////////////////////////////////

//                // Test Select Many ////////////////////////////////
//                // Act
//                var activities = conn.Select<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "IsActive", Value = 1 } });

//                // Assert
//                Assert.AreEqual(activities.Count(), 2);
//                Assert.AreEqual(activities.ElementAt(0).Id, 1);
//                Assert.AreEqual(activities.ElementAt(0).Name, "Read");
//                Assert.AreEqual(activities.ElementAt(1).Id, 2);
//                Assert.AreEqual(activities.ElementAt(1).Name, "Write");
//                ////////////////////////////////////////////////////

//                // Select return none //////////////////////////////
//                // Act
//                var internals = conn.Select<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "ActivityType", Value = 1000 } });

//                // Assert
//                Assert.AreEqual(internals.Count(), 0);
//                ////////////////////////////////////////////////////

//                // ExecuteSql //////////////////////////////////////
//                // Arrange
//                activities = null;
//                var sql = "SELECT * FROM ActivityOra WHERE IsActive = 1";

//                // Act 
//                activities = conn.ExecuteSql<ActivityOra>(sql);

//                // Assert
//                Assert.AreEqual(activities.Count(), 2);
//                Assert.AreEqual(activities.ElementAt(0).Id, 1);
//                Assert.AreEqual(activities.ElementAt(0).Name, "Read");
//                Assert.AreEqual(activities.ElementAt(1).Id, 2);
//                Assert.AreEqual(activities.ElementAt(1).Name, "Write");
//                ////////////////////////////////////////////////////

//                //// ExecuteProcedure ////////////////////////////////
//                //// Arrange
//                activities = null;
//                var parameters = new List<OracleParameter>();
//                parameters.Add(new OracleParameter() { ParameterName = "IsActive", Value = 1 });
//                parameters.Add(new OracleParameter() { ParameterName = "cursor", OracleDbType = OracleDbType.RefCursor, Direction = ParameterDirection.Output });

//                // Act 
//                activities = conn.ExecuteProcedure<ActivityOra>("GetActivities", parameters);

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
//                conn.Update(read, new OracleParameter() { ParameterName = "Id", Value = 1 });

//                // Assert
//                var readOnly = conn.Single<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = 1 } });
//                Assert.AreEqual(readOnly.Name, "Read Only");
//                Assert.AreEqual(readOnly.Id, 1);
//                ////////////////////////////////////////////////////

//                // Update single /////////////////////////////////////
//                // Arrange 
//                readOnly.IsActive = false;
//                write.IsActive = false;

//                // Act
//                conn.Update<ActivityOra>(readOnly, new List<OracleParameter>() { new OracleParameter() { ParameterName = "IsActive", Value = 0 } }, new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = readOnly.Id } });
//                conn.Update<ActivityOra>(write, new List<OracleParameter>() { new OracleParameter() { ParameterName = "IsActive", Value = 0 } }, new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = write.Id } });

//                // Assert
//                var updated = conn.Select<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "IsActive", Value = 0 } });
//                Assert.AreEqual(updated.Count(), 3);
//                ////////////////////////////////////////////////////

//                // Delete single ///////////////////////////////////
//                // Act
//                conn.Delete<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = 1 } });

//                // Assert
//                readOnly = conn.Single<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = 1 } });
//                Assert.IsNull(readOnly);

//                activities = conn.Select<ActivityOra>();
//                Assert.AreEqual(activities.Count(), 2);
//                ////////////////////////////////////////////////////

//                // Delete many /////////////////////////////////////
//                // Act
//                conn.Delete<ActivityOra>(new List<OracleParameter>() { new OracleParameter() { ParameterName = "IsActive", Value = 0 } });

//                // Assert
//                activities = conn.Select<ActivityOra>();
//                Assert.AreEqual(activities.Count(), 0);
//                ////////////////////////////////////////////////////
//            }
//        }

//        [TestMethod]
//        public void Transaction_Commit()
//        {
//            // Assert
//            var read = new ActivityOra()
//            {
//                Name = "Read",
//                Status = 1,
//                IsActive = true,
//                Created = DateTime.Today,
//                Updated = DateTime.Today,
//                ActivityType = ActivityTypeEnum.Shared,
//            };

//            using (var conn = new OracleConnection(connectionString))
//            {
//                read.Id = Convert.ToInt32(conn.ExecuteScalar("SELECT \"ActivityOra_seq\".NEXTVAL FROM DUAL"));
//                read = conn.Insert(read);
//            }

//            // Act
//            using (var conn = new OracleConnection(connectionString))
//            {
//                conn.Open();
//                var transaction = conn.BeginTransaction();

//                read.Name = "Read Only";

//                conn.Update(read,
//                    new List<OracleParameter>() { new OracleParameter() { ParameterName = "Name", Value = read.Name } },
//                    new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = read.Id } },
//                    transaction);

//                read.Status = 3;

//                conn.Update(read,
//                    new List<OracleParameter>() { new OracleParameter() { ParameterName = "Status", Value = read.Status } },
//                    new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = read.Id } },
//                    transaction);

//                transaction.Commit();
//            }

//            // Assert
//            ActivityOra result;
//            using (var conn = new OracleConnection(connectionString))
//            {
//                result =
//                    conn.Single<ActivityOra>(new List<OracleParameter>()
//                    {
//                        new OracleParameter() {ParameterName = "Id", Value = read.Id}
//                    });
//            }

//            Assert.AreEqual(result.Id, read.Id);
//            Assert.AreEqual(result.Name, "Read Only");
//            Assert.AreEqual(result.Status, 3);
//            Assert.AreEqual(result.Created, read.Created);
//            Assert.AreEqual(result.Updated, read.Updated);
//            Assert.AreEqual(result.ActivityType, read.ActivityType);
//        }

//        [TestMethod]
//        public void Transaction_Rollback()
//        {
//            // Assert
//            var read = new ActivityOra()
//            {
//                Name = "Read",
//                Status = 1,
//                IsActive = true,
//                Created = DateTime.Today,
//                Updated = DateTime.Today,
//                ActivityType = ActivityTypeEnum.Shared,
//            };

//            using (var conn = new OracleConnection(connectionString))
//            {
//                read.Id = Convert.ToInt32(conn.ExecuteScalar("SELECT \"ActivityOra_seq\".NEXTVAL FROM DUAL"));
//                read = conn.Insert(read);
//            }

//            // Act
//            using (var conn = new OracleConnection(connectionString))
//            {
//                conn.Open();
//                var transaction = conn.BeginTransaction();

//                try
//                {
//                    read.Name = "Read Only";

//                    conn.Update(read,
//                        new List<OracleParameter>() { new OracleParameter() { ParameterName = "Name", Value = read.Name } },
//                        new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = read.Id } },
//                        transaction);

//                    read.Status = 3;

//                    conn.Update(read,
//                        new List<OracleParameter>() { new OracleParameter() { ParameterName = "Level", Value = read.Status } },
//                        new List<OracleParameter>() { new OracleParameter() { ParameterName = "Id", Value = read.Id } },
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
//            ActivityOra result;
//            using (var conn = new OracleConnection(connectionString))
//            {
//                result =
//                    conn.Single<ActivityOra>(new List<OracleParameter>()
//                    {
//                        new OracleParameter() {ParameterName = "Id", Value = read.Id}
//                    });
//            }

//            Assert.AreEqual(result.Id, read.Id);
//            Assert.AreEqual(result.Name, "Read");
//            Assert.AreEqual(result.Status, 1);
//            Assert.AreEqual(result.Created, read.Created);
//            Assert.AreEqual(result.Updated, read.Updated);
//            Assert.AreEqual(result.ActivityType, read.ActivityType);
//        }
//    }
//}
