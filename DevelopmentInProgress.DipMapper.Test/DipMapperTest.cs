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
            var sqlActivity = DipMapper.GetSqlSelectFields<Activity>();
            var sqlGenericActivity = DipMapper.GetSqlSelectFields<GenericActivity<Activity>>();
            var sqlGenericActivityInt32 = DipMapper.GetSqlSelectFields<GenericActivity<Int32>>();
            
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
            var sqlEmail = DipMapper.GetSqlUpdateFields<Activity>(email, ignoreId);
            var sqlGenericWrite = DipMapper.GetSqlUpdateFields<GenericActivity<Activity>>(genericWrite, ignoreId);
            var sqlGenericEmail = DipMapper.GetSqlUpdateFields<GenericActivity<Int32>>(genericEmail, ignoreId);

            // Assert
            Assert.AreEqual(sqlEmail, "Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType");
            Assert.AreEqual(sqlGenericWrite, "Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType");
            Assert.AreEqual(sqlGenericEmail, "Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType, GenericProperty=@GenericProperty");
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
            Assert.AreEqual(sqlWhereClause, " WHERE Id=@Id AND Name=@Name AND Level=@Level AND IsActive=@IsActive AND Created=@Created AND Updated is @Updated AND ActivityType=@ActivityType AND GenericProperty=@GenericProperty");
        }

        [TestMethod]
        public void DipMapper_GetSqlSelect_Test()
        {
            // Arrange

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<Activity>();
            var sqlSelectGeneric = DipMapper.GetSqlSelect<GenericActivity<Activity>>();
            var sqlSelectGenericInt32 = DipMapper.GetSqlSelect<GenericActivity<Int32>>();

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity;");
            Assert.AreEqual(sqlSelectGeneric, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity;");
            Assert.AreEqual(sqlSelectGenericInt32, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType, GenericProperty FROM Int32;");
        }

        [TestMethod]
        public void DipMapper_GetSqlSelect_Where_Test()
        {
            // Arrange
            var parametersActivity = new Dictionary<string, object>();
            parametersActivity.Add("Id", email.Id);

            var parametersGenericActivity = new Dictionary<string, object>();
            parametersGenericActivity.Add("Id", genericWrite.Id);

            var parametersGenericActivityInt32 = new Dictionary<string, object>();
            parametersGenericActivityInt32.Add("Id", genericEmail.Id);

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<Activity>(parametersActivity);
            var sqlSelectGeneric = DipMapper.GetSqlSelect<GenericActivity<Activity>>(parametersGenericActivity);
            var sqlSelectGenericInt32 = DipMapper.GetSqlSelect<GenericActivity<Int32>>(parametersGenericActivityInt32);

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id=@Id;");
            Assert.AreEqual(sqlSelectGeneric, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id=@Id;");
            Assert.AreEqual(sqlSelectGenericInt32, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType, GenericProperty FROM Int32 WHERE Id=@Id;");
        }

        [TestMethod]
        public void DipMapper_GetSqlInsert_Test()
        {
            // Arrange

            // Act
            var sqlInsertEmail = DipMapper.GetSqlInsert<Activity>(email, new [] {"Id"});
            var sqlInsertGenericWrite = DipMapper.GetSqlInsert<GenericActivity<Activity>>(genericWrite, new [] { "Id" });
            var sqlInsertGenericEmail = DipMapper.GetSqlInsert<GenericActivity<Int32>>(genericEmail, new [] { "Id" });

            // Assert
            Assert.AreEqual(sqlInsertEmail, "INSERT INTO Activity (Name, Level, IsActive, Created, Updated, ActivityType) VALUES (@Name, @Level, @IsActive, @Created, @Updated, @ActivityType);");
            Assert.AreEqual(sqlInsertGenericWrite, "INSERT INTO Activity (Name, Level, IsActive, Created, Updated, ActivityType) VALUES (@Name, @Level, @IsActive, @Created, @Updated, @ActivityType);");
            Assert.AreEqual(sqlInsertGenericEmail, "INSERT INTO Int32 (Name, Level, IsActive, Created, Updated, ActivityType, GenericProperty) VALUES (@Name, @Level, @IsActive, @Created, @Updated, @ActivityType, @GenericProperty);");
        }

        [TestMethod]
        public void DipMapper_GetSqlUpdate_Test()
        {
            // Arrange
            var parametersActivity = new Dictionary<string, object>();
            parametersActivity.Add("Id", email.Id);

            var parametersGenericActivity = new Dictionary<string, object>();
            parametersGenericActivity.Add("Id", genericWrite.Id);

            var parametersGenericActivityInt32 = new Dictionary<string, object>();
            parametersGenericActivityInt32.Add("Id", genericEmail.Id);

            // Act
            var sqlUpdateEmail = DipMapper.GetSqlUpdate<Activity>(email, parametersActivity);
            var sqlUpdateGenericWrite = DipMapper.GetSqlUpdate<GenericActivity<Activity>>(genericWrite, parametersGenericActivity);
            var sqlUpdateGenericEmail = DipMapper.GetSqlUpdate<GenericActivity<Int32>>(genericEmail, parametersGenericActivityInt32);

            // Assert
            Assert.AreEqual(sqlUpdateEmail, "UPDATE Activity SET Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType WHERE Id=@Id;");
            Assert.AreEqual(sqlUpdateGenericWrite, "UPDATE Activity SET Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType WHERE Id=@Id;");
            Assert.AreEqual(sqlUpdateGenericEmail, "UPDATE Int32 SET Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType, GenericProperty=@GenericProperty WHERE Id=@Id;");
        }

        [TestMethod]
        public void DipMapper_GetSqlDelete_Test()
        {
            // Arrange
            var parametersActivity = new Dictionary<string, object>();
            parametersActivity.Add("Id", email.Id);

            var parametersGenericActivity = new Dictionary<string, object>();
            parametersGenericActivity.Add("Id", genericWrite.Id);

            var parametersGenericActivityInt32 = new Dictionary<string, object>();
            parametersGenericActivityInt32.Add("Id", genericEmail.Id);

            // Act
            var sqlDeleteEmail = DipMapper.GetSqlDelete<Activity>(parametersActivity);
            var sqlDeleteGenericWrite = DipMapper.GetSqlDelete<GenericActivity<Activity>>(parametersGenericActivity);
            var sqlDeleteGenericEmail = DipMapper.GetSqlDelete<GenericActivity<Int32>>(parametersGenericActivityInt32);

            // Assert
            Assert.AreEqual(sqlDeleteEmail, "DELETE FROM Activity WHERE Id=@Id;");
            Assert.AreEqual(sqlDeleteGenericWrite, "DELETE FROM Activity WHERE Id=@Id;");
            Assert.AreEqual(sqlDeleteGenericEmail, "DELETE FROM Int32 WHERE Id=@Id;");
        }
    }
}
