using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevelopmentInProgress.DipMapper.Test
{
    [TestClass]
    public class DipMapperMSSQLTest
    {
        [TestMethod]
        public void GetParameterPrefix_NotWhereClause()
        {
            // Arrange

            // Act
            var parameterPrefix = DipMapper.GetParameterPrefix(DipMapper.ConnType.MSSQL);

            // Assert
            Assert.AreEqual(parameterPrefix, "@");
        }

        [TestMethod]
        public void GetParameterPrefix_IsWhereClause()
        {
            // Arrange

            // Act
            var parameterPrefix = DipMapper.GetParameterPrefix(DipMapper.ConnType.MSSQL, true);

            // Assert
            Assert.AreEqual(parameterPrefix, "@p");
        }

        [TestMethod]
        public void GetSqlWhereClause_WithParameters_WhereClause()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var genericActivity = new GenericActivity<Activity>() {Name = "Activity"};

            var parameters = new Dictionary<string, object>();
            parameters.Add("Id", genericActivity.Id);
            parameters.Add("Name", genericActivity.Name);
            parameters.Add("Level", genericActivity.Level);
            parameters.Add("IsActive", genericActivity.IsActive);
            parameters.Add("Created", genericActivity.Created);
            parameters.Add("Updated", genericActivity.Updated);
            parameters.Add("ActivityType", genericActivity.ActivityType);

            // Act
            var sqlWhereClause = DipMapper.GetSqlWhereClause(connType, parameters);

            // Assert
            Assert.AreEqual(sqlWhereClause, " WHERE Id=@pId AND Name=@pName AND Level=@pLevel AND IsActive=@pIsActive AND Created=@pCreated AND Updated is @pUpdated AND ActivityType=@pActivityType");
        }

        [TestMethod]
        public void GetSqlInsertFields_SkipIdentityFieldOnly_GetSql()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() {"Id"};

            // Act
            var sqlInsertFields = DipMapper.GetSqlInsertFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlInsertFields, " (Name, Level, IsActive, Created, Updated, ActivityType) VALUES (@Name, @Level, @IsActive, @Created, @Updated, @ActivityType)");
        }

        [TestMethod]
        public void GetSqlInsertFields_SkipMultipleFields_GetSql()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Updated", "Id" };            

            // Act
            var sqlInsertFields = DipMapper.GetSqlInsertFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlInsertFields, " (Name, Level, IsActive, Created, ActivityType) VALUES (@Name, @Level, @IsActive, @Created, @ActivityType)");
        }

        [TestMethod]
        public void GetConnType_GetForSqlConn()
        {
            // Arrange
            var conn = new SqlConnection();

            // Act
            var connType = DipMapper.GetConnType(conn);

            // Assert
            Assert.AreEqual(connType, DipMapper.ConnType.MSSQL);
        }

        [TestMethod]
        public void GetSqlUpdateFields_SkipIdentityOnUpdateOnly_GetSql()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Id" };

            // Act
            var sqlUpdateFields = DipMapper.GetSqlUpdateFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdateFields, "Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType");
        }

        [TestMethod]
        public void GetSqlUpdateFields_SkipMultipleFields_GetSql()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Id", "Created", "Updated"};

            // Act
            var sqlUpdateFields = DipMapper.GetSqlUpdateFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdateFields, "Name=@Name, Level=@Level, IsActive=@IsActive, ActivityType=@ActivityType");
        }

        [TestMethod]
        public void GetSqlSelect_WithParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() {{"Id", 3}};

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<Activity>(connType, propertyInfos, parameters);

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id=@pId;");
        }

        [TestMethod]
        public void GetSqlInsert_NoIdentity()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>();

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(DipMapper.ConnType.MSSQL, propertyInfos, "", "", skipFields);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Id, Name, Level, IsActive, Created, Updated, ActivityType) VALUES (@Id, @Name, @Level, @IsActive, @Created, @Updated, @ActivityType);");
        }

        [TestMethod]
        public void GetSqlInsert_WithIdentity()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipIdentity = new List<string>() { "Id" };

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(DipMapper.ConnType.MSSQL, propertyInfos, "Id", "", skipIdentity);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Name, Level, IsActive, Created, Updated, ActivityType) VALUES (@Name, @Level, @IsActive, @Created, @Updated, @ActivityType);SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id = SCOPE_IDENTITY();");
        }

        [TestMethod]
        public void GetSqlInsert_SkipFields()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Id", "Created", "Updated" };

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<Activity>(DipMapper.ConnType.MSSQL, propertyInfos, "Id", "", skipFields);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO Activity (Name, Level, IsActive, ActivityType) VALUES (@Name, @Level, @IsActive, @ActivityType);SELECT Id, Name, Level, IsActive, Created, Updated, ActivityType FROM Activity WHERE Id = SCOPE_IDENTITY();");
        }

        [TestMethod]
        public void GetSqlUpdate_WithoutParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>();
            var skipFields = new List<string>();

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Id=@Id, Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType;");
        }

        [TestMethod]
        public void GetSqlUpdate_WithParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() {{"Id", 5}};
            var skipFields = new List<string>() {"Id"};

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Name=@Name, Level=@Level, IsActive=@IsActive, Created=@Created, Updated=@Updated, ActivityType=@ActivityType WHERE Id=@pId;");
        }

        [TestMethod]
        public void GetSqlUpdate_SkipFields()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };
            var skipFields = new List<string>() {"Id", "Created", "Updated"};

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<Activity>(connType, propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE Activity SET Name=@Name, Level=@Level, IsActive=@IsActive, ActivityType=@ActivityType WHERE Id=@pId;");
        }

        [TestMethod]
        public void GetSqlDelete_WithParameters()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };

            // Act
            var deleteSql = DipMapper.GetSqlDelete<Activity>(connType, parameters);

            // Assert
            Assert.AreEqual(deleteSql, "DELETE FROM Activity WHERE Id=@pId;");
        }

        [TestMethod]
        public void GetExtendedParameters_ParametersOnly()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };

            // Act
            var extendedParameters = DipMapper.GetExtendedParameters<Activity>(connType, parameters);

            // Assert
            Assert.AreEqual(extendedParameters.Count(), 1);
            Assert.AreEqual(extendedParameters.ElementAt(0).Key, "@pId");
        }

        [TestMethod]
        public void GetExtendedParameters_PropertyInfosParametersSkipFields()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };
            var skipFields = new List<string>() {"Id"};

            var activity = new Activity()
            {
                Id = 3,
                Name = "Activity1",
                IsActive = true,
                Level = 4,
                Created = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared
            };

            // Act
            var extendedParameters = DipMapper.GetExtendedParameters(activity, connType, propertyInfos, skipFields, parameters);

            // Assert
            Assert.AreEqual(extendedParameters.Count(), 7);
            Assert.AreEqual(extendedParameters.ElementAt(0).Key, "@Name");
            Assert.AreEqual(extendedParameters.ElementAt(1).Key, "@Level");
            Assert.AreEqual(extendedParameters.ElementAt(2).Key, "@IsActive");
            Assert.AreEqual(extendedParameters.ElementAt(3).Key, "@Created");
            Assert.AreEqual(extendedParameters.ElementAt(4).Key, "@Updated");
            Assert.AreEqual(extendedParameters.ElementAt(5).Key, "@ActivityType");
            Assert.AreEqual(extendedParameters.ElementAt(6).Key, "@pId");
        }

        [TestMethod]
        public void GetExtendedParameters_PropertyInfosSkipFields()
        {
            // Arrange
            var conn = new SqlConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<Activity>();
            var skipFields = new List<string>() { "Id" };

            var activity = new Activity()
            {
                Id = 3,
                Name = "Activity1",
                IsActive = true,
                Level = 4,
                Created = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared
            };

            // Act
            var extendedParameters = DipMapper.GetExtendedParameters(activity, connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(extendedParameters.Count(), 6);
            Assert.AreEqual(extendedParameters.ElementAt(0).Key, "@Name");
            Assert.AreEqual(extendedParameters.ElementAt(1).Key, "@Level");
            Assert.AreEqual(extendedParameters.ElementAt(2).Key, "@IsActive");
            Assert.AreEqual(extendedParameters.ElementAt(3).Key, "@Created");
            Assert.AreEqual(extendedParameters.ElementAt(4).Key, "@Updated");
            Assert.AreEqual(extendedParameters.ElementAt(5).Key, "@ActivityType");
        }
    }
}
