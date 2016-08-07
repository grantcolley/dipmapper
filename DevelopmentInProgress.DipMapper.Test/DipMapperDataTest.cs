using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipMapper.Test
{
    [TestClass]
    public class DipMapperDataTest
    {
        private string connectionString = "Data Source=(local);Initial Catalog=DipMapper;Integrated Security=true";

        [TestInitialize]
        public void Setup()
        {
        }

        [TestMethod]
        public void DipMapper_SelectSingleActivityById_TestPasses()
        {
            // Arrange
            var conn = new SqlConnection(connectionString);
            var parameters = new Dictionary<string, object>() {{"Id", 1}};

            // Act
            var activity = conn.Single<Activity>(parameters);

            // Assert
            Assert.AreEqual(activity.Id, 1);
            Assert.AreEqual(activity.Name, "Email");
            Assert.AreEqual(activity.Level, 3);
            Assert.AreEqual(activity.IsActive, true);
            Assert.AreEqual(activity.Created, new DateTime(2016, 08, 01));
            Assert.AreEqual(activity.Updated, new DateTime(2016, 08, 01));
            Assert.AreEqual(activity.ActivityType, ActivityTypeEnum.Private);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DipMapper_SelectSingleActivityByIsActive_InvalidOperationException()
        {
            // Arrange
            var conn = new SqlConnection(connectionString);
            var parameters = new Dictionary<string, object>() {{"IsActive", true}};

            // Act
            var activity = conn.Single<Activity>(parameters);

            // Assert
        }

        [TestMethod]
        public void DipMapper_SelectActivitiesByIsActive_TestPasses()
        {
            // Arrange
            var conn = new SqlConnection(connectionString);
            var parameters = new Dictionary<string, object>() {{"IsActive", true}};

            // Act
            var activities = conn.Select<Activity>(parameters);

            // Assert
            var activity1 = activities.FirstOrDefault(a1 => a1.Id == 1);
            var activity2 = activities.FirstOrDefault(a1 => a1.Id == 2);

            Assert.AreEqual(activities.Count(), 2);

            Assert.AreEqual(activity1.Id, 1);
            Assert.AreEqual(activity1.Name, "Email");
            Assert.AreEqual(activity1.Level, 3);
            Assert.AreEqual(activity1.IsActive, true);
            Assert.AreEqual(activity1.Created, new DateTime(2016, 08, 01));
            Assert.AreEqual(activity1.Updated, new DateTime(2016, 08, 01));
            Assert.AreEqual(activity1.ActivityType, ActivityTypeEnum.Private);

            Assert.AreEqual(activity2.Id, 2);
            Assert.AreEqual(activity2.Name, "Write");
            Assert.AreEqual(activity2.Level, 2);
            Assert.AreEqual(activity2.IsActive, true);
            Assert.AreEqual(activity2.Created, new DateTime(2016, 08, 01));
            Assert.AreEqual(activity2.Updated, new DateTime(2016, 08, 01));
            Assert.AreEqual(activity2.ActivityType, ActivityTypeEnum.Shared);
        }

        [TestMethod]
        public void DipMapper_SelectAllActivities_TestPasses()
        {
            // Arrange
            var conn = new SqlConnection(connectionString);

            // Act
            var activities = conn.Select<Activity>();

            // Assert
            Assert.AreEqual(activities.Count(), 3);
        }

        [TestMethod]
        public void DipMapper_SelectSingleActivityByInvalidActivityType_NoneReturned()
        {
            // Arrange
            var conn = new SqlConnection(connectionString);
            var parameters = new Dictionary<string, object>() {{"ActivityType", 100}};

            // Act
            var activity = conn.Single<Activity>(parameters);

            // Assert
            Assert.IsNull(activity);
        }

        [TestMethod]
        public void DipMapper_SelectActivitiesByInvalidActivityType_NoneReturned()
        {
            // Arrange
            var conn = new SqlConnection(connectionString);
            var parameters = new Dictionary<string, object>() {{"ActivityType", 100}};

            // Act
            var activities = conn.Select<Activity>(parameters);

            // Assert
            Assert.AreEqual(activities.Count(), 0);
        }

        [TestMethod]
        public void DipMapper_Database_Test()
        {
            // Arrange
            var read = new Activity()
            {
                Name = "Read",
                Level = 1,
                IsActive = true,
                Created = DateTime.Today,
                Updated = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared,
            };

            var write = new Activity()
            {
                Name = "Write",
                Level = 2,
                IsActive = true,
                Created = DateTime.Today.AddDays(1),
                Updated = DateTime.Today.AddDays(1),
                ActivityType = ActivityTypeEnum.Private,
            };

            var email = new Activity()
            {
                Name = "Email",
                Level = 3,
                IsActive = true,
                Created = DateTime.Today.AddDays(2),
                Updated = null,
                ActivityType = ActivityTypeEnum.Public,
            };

            using (var conn = new SqlConnection(connectionString))
            {
                // Test Insert
                // Act 
                read = conn.Insert<Activity>(read, "Id");
                write = conn.Insert<Activity>(write, "Id");
                email = conn.Insert<Activity>(email, "Id");

                // Assert
                Assert.AreEqual(read.Id, 1);
                Assert.AreEqual(write.Id, 2);
                Assert.AreEqual(email.Id, 3);

                // Select single

                // Select many

                // Select none

                // Update single

                // Update many

                // Delete single

                // Delete many
            }
        }
    }
}
