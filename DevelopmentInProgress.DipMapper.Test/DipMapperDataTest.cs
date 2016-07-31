using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        public void DipMapper_GetPropertyInfos_Test()
        {
            // Arrange
            var conn = new SqlConnection(connectionString);
            var parameters = new Dictionary<string, object>() {{"Id", 1}};

            // Act
            var activity = conn.Single<Activity>(parameters);

            // Assert
            Assert.AreEqual(activity.Id, 6);
            Assert.AreEqual(activity.Name, "Email");
            Assert.AreEqual(activity.Level, 3);
            Assert.AreEqual(activity.IsActive, true);
            Assert.AreEqual(activity.Created, new DateTime(2016, 08, 01));
            Assert.AreEqual(activity.Updated, new DateTime(2016, 08, 01));
            Assert.AreEqual(activity.ActivityType, 1);
        }
    }
}
