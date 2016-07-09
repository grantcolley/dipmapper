using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipMapper.Test
{
    [TestClass]
    public class DipMapperTest
    {
        private Activity email;
        private Activity write;
        private GenericActivity<Int32> genericEmail;
        private GenericActivity<Activity> genericWrite;
        
        [TestInitialize]
        public void Setup()
        {
            email = new Activity()
            {
                Id = 6,
                Name = "Email",
                Level = 3,
                IsActive = true,
                Created = DateTime.Today,
                ActivityType = ActivityTypeEnum.Private,
            };
            
            write = new Activity()
            {
                Id=5,
                Name = "Write",
                Level = 2,
                IsActive = true,
                Created = DateTime.Today,
                Updated = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared,
            };

            email.ParentActivity = write;
            write.Activities_2.Add(email);

            genericWrite = new GenericActivity<Activity>()
            {
                Id = 8,
                Name = "Generic Write",
                Level = 5,
                IsActive = true,
                Created = DateTime.Today,
                Updated = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared,
            };

            genericEmail = new GenericActivity<Int32>()
            {
                Id = 7,
                Name = "Generic Email",
                Level = 4,
                IsActive = false,
                Created = DateTime.Today,
                ActivityType = ActivityTypeEnum.Public,
                GenericProperty = 8
            };

            genericEmail.ParentActivity = genericEmail;
            genericEmail.Activities_2.Add(genericEmail);

            genericWrite.GenericProperty = write;
            genericWrite.ParentActivity = genericWrite;
            genericWrite.Activities_2.Add(genericWrite);
        }

        [TestMethod]
        public void DipMapper_GetTableName_Test()
        {
            // Arrange

            // Act
            var tableName = DipMapper.GetSqlTableName<Activity>();
            var genericTableName = DipMapper.GetSqlTableName<GenericActivity<Activity>>();
            var genericTableNameInt32 = DipMapper.GetSqlTableName<GenericActivity<Int32>>();

            // Assert
            Assert.AreEqual(tableName, "Activity");
            Assert.AreEqual(genericTableName, "Activity");
            Assert.AreEqual(genericTableNameInt32, "Int32");
        }

        [TestMethod]
        public void DipMapper_GetFields_ForSelect_Test()
        {
            // Arrange

            // Act
            var sqlActivity = DipMapper.GetSqlFields<Activity>();
            var sqlGenericActivity = DipMapper.GetSqlFields<GenericActivity<Activity>>();
            var sqlGenericActivityInt32 = DipMapper.GetSqlFields<GenericActivity<Int32>>();
            
            // Assert
            Assert.AreEqual(sqlActivity, "Id, Name, Level, IsActive, Created, Updated, ActivityType");
            Assert.AreEqual(sqlGenericActivity, "Id, Name, Level, IsActive, Created, Updated, ActivityType");
            Assert.AreEqual(sqlGenericActivityInt32, "Id, Name, Level, IsActive, Created, Updated, ActivityType, GenericProperty");
        }

        [TestMethod]
        public void DipMapper_GetFields_ForUpdate_Test()
        {
            // Arrange
            var ignoreId = new List<string>() { "Id" };

            // Act
            var sqlEmail = DipMapper.GetSqlFields<Activity>(email, ignoreId);
            var sqlGenericWrite = DipMapper.GetSqlFields<GenericActivity<Activity>>(genericWrite, ignoreId);
            var sqlGenericEmail = DipMapper.GetSqlFields<GenericActivity<Int32>>(genericEmail, ignoreId);

            // Assert
            Assert.AreEqual(sqlEmail, string.Format("Name='Email', Level=3, IsActive=1, Created='{0}', Updated=null, ActivityType=1", DateTime.Today.Date));
            Assert.AreEqual(sqlGenericWrite, string.Format("Name='Generic Write', Level=5, IsActive=1, Created='{0}', Updated='{0}', ActivityType=2", DateTime.Today.Date));
            Assert.AreEqual(sqlGenericEmail, string.Format("Name='Generic Email', Level=4, IsActive=0, Created='{0}', Updated=null, ActivityType=0, GenericProperty=8", DateTime.Today.Date));
        }

        [TestMethod]
        public void DipMapper_GetSqlWhereClause_Test()
        {
            // Arrange
            var parameters = new Dictionary<string, object>();
            parameters.Add("Id", genericEmail.Id);
            parameters.Add("Name", genericEmail.Name);
            parameters.Add("Level", genericEmail.Level);
            parameters.Add("IsActive", genericEmail.IsActive);
            parameters.Add("Created", genericEmail.Created);
            parameters.Add("Updated", genericEmail.Updated);
            parameters.Add("ActivityType", genericEmail.ActivityType);
            parameters.Add("GenericProperty", genericEmail.GenericProperty);

            // Act
            var sqlWhereClause = DipMapper.GetSqlWhereClause(parameters);

            // Assert
            Assert.AreEqual(sqlWhereClause, string.Format(" WHERE Id=7 AND Name='Generic Email' AND Level=4 AND IsActive=0 AND Created='{0}' AND Updated is null AND ActivityType=0 AND GenericProperty=8", DateTime.Today.Date));
        }

        [TestMethod]
        public void DipMapper_GetSqlSelect_Test()
        {
            // Arrange

            // Act

            // Assert
        }

        [TestMethod]
        public void DipMapper_GetSqlUpdate_Test()
        {
            // Arrange

            // Act

            // Assert
        }

        [TestMethod]
        public void DipMapper_GetSqlDelete_Test()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
