using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;

namespace DevelopmentInProgress.DipMapper.Test
{
    [TestClass]
    public class DipMapperOracleTest
    {
        [TestMethod]
        public void GetParameterPrefix_NotWhereClause()
        {
            // Arrange

            // Act
            var parameterPrefix = DipMapper.GetParameterPrefix(DipMapper.ConnType.Oracle);

            // Assert
            Assert.AreEqual(parameterPrefix, ":");
        }

        [TestMethod]
        public void GetParameterPrefix_IsWhereClause()
        {
            // Arrange

            // Act
            var parameterPrefix = DipMapper.GetParameterPrefix(DipMapper.ConnType.Oracle, true);

            // Assert
            Assert.AreEqual(parameterPrefix, ":p");
        }

        [TestMethod]
        public void GetSqlWhereClause_WithParameters_WhereClause()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var genericActivity = new GenericActivityOra<ActivityOra>() { Name = "ActivityOra" };

            var parameters = new Dictionary<string, object>();
            parameters.Add("Id", genericActivity.Id);
            parameters.Add("Name", genericActivity.Name);
            parameters.Add("Status", genericActivity.Status);
            parameters.Add("IsActive", genericActivity.IsActive);
            parameters.Add("Created", genericActivity.Created);
            parameters.Add("Updated", genericActivity.Updated);
            parameters.Add("ActivityType", genericActivity.ActivityType);

            // Act
            var sqlWhereClause = DipMapper.GetSqlWhereClause(connType, parameters);

            // Assert
            Assert.AreEqual(sqlWhereClause, " WHERE Id=:pId AND Name=:pName AND Status=:pStatus AND IsActive=:pIsActive AND Created=:pCreated AND Updated is :pUpdated AND ActivityType=:pActivityType");
        }

        [TestMethod]
        public void GetSqlInsertFields_GetSql()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var skipFields = new List<string>();

            // Act
            var sqlInsertFields = DipMapper.GetSqlInsertFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlInsertFields, " (Id, Name, Status, IsActive, Created, Updated, ActivityType) VALUES (:Id, :Name, :Status, :IsActive, :Created, :Updated, :ActivityType)");
        }

        [TestMethod]
        public void GetSqlInsertFields_SkipMultipleFields_GetSql()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var skipFields = new List<string>() { "Updated", "Id" };            

            // Act
            var sqlInsertFields = DipMapper.GetSqlInsertFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlInsertFields, " (Name, Status, IsActive, Created, ActivityType) VALUES (:Name, :Status, :IsActive, :Created, :ActivityType)");
        }

        [TestMethod]
        public void GetConnType_GetForOracleConn()
        {
            // Arrange
            var conn = new OracleConnection();

            // Act
            var connType = DipMapper.GetConnType(conn);

            // Assert
            Assert.AreEqual(connType, DipMapper.ConnType.Oracle);
        }

        [TestMethod]
        public void GetSqlUpdateFields_SkipIdentityOnUpdateOnly_GetSql()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var skipFields = new List<string>() { "Id" };

            // Act
            var sqlUpdateFields = DipMapper.GetSqlUpdateFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdateFields, "Name=:Name, Status=:Status, IsActive=:IsActive, Created=:Created, Updated=:Updated, ActivityType=:ActivityType");
        }

        [TestMethod]
        public void GetSqlUpdateFields_SkipMultipleFields_GetSql()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var skipFields = new List<string>() { "Id", "Created", "Updated"};

            // Act
            var sqlUpdateFields = DipMapper.GetSqlUpdateFields(connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdateFields, "Name=:Name, Status=:Status, IsActive=:IsActive, ActivityType=:ActivityType");
        }

        [TestMethod]
        public void GetSqlSelect_WithParameters()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var parameters = new Dictionary<string, object>() {{"Id", 3}};

            // Act
            var sqlSelect = DipMapper.GetSqlSelect<ActivityOra>(connType, propertyInfos, parameters);

            // Assert
            Assert.AreEqual(sqlSelect, "SELECT Id, Name, Status, IsActive, Created, Updated, ActivityType FROM ActivityOra WHERE Id=:pId;");
        }

        [TestMethod]
        public void GetSqlInsert_NoIdentity()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var skipFields = new List<string>();

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<ActivityOra>(DipMapper.ConnType.Oracle, propertyInfos, "", "", skipFields);

            // Assert
            Assert.AreEqual(sqlInsert, "INSERT INTO ActivityOra (Id, Name, Status, IsActive, Created, Updated, ActivityType) VALUES (:Id, :Name, :Status, :IsActive, :Created, :Updated, :ActivityType);");
        }

        [TestMethod]
        public void GetSqlInsert_WithIdentity()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var skipIdentity = new List<string>() { "Id" };

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<ActivityOra>(DipMapper.ConnType.Oracle, propertyInfos, "Id", "", skipIdentity);

            // Assert
            Assert.AreEqual(sqlInsert, "DECLARE nextId NUMBER; BEGIN nextId := .nextval; INSERT INTO ActivityOra (Name, Status, IsActive, Created, Updated, ActivityType) VALUES (:Name, :Status, :IsActive, :Created, :Updated, :ActivityType); SELECT Id, Name, Status, IsActive, Created, Updated, ActivityType FROM ActivityOra WHERE Id = nextId; END;");
        }

        [TestMethod]
        public void GetSqlInsert_SkipFields()
        {
            // Arrange
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var skipFields = new List<string>() { "Id", "Created", "Updated" };

            // Act
            var sqlInsert = DipMapper.GetSqlInsert<ActivityOra>(DipMapper.ConnType.Oracle, propertyInfos, "Id", "", skipFields);

            // Assert
            Assert.AreEqual(sqlInsert, "DECLARE nextId NUMBER; BEGIN nextId := .nextval; INSERT INTO ActivityOra (Name, Status, IsActive, ActivityType) VALUES (:Name, :Status, :IsActive, :ActivityType); SELECT Id, Name, Status, IsActive, Created, Updated, ActivityType FROM ActivityOra WHERE Id = nextId; END;");
        }

        [TestMethod]
        public void GetSqlUpdate_WithoutParameters()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var parameters = new Dictionary<string, object>();
            var skipFields = new List<string>();

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<ActivityOra>(connType, propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE ActivityOra SET Id=:Id, Name=:Name, Status=:Status, IsActive=:IsActive, Created=:Created, Updated=:Updated, ActivityType=:ActivityType;");
        }

        [TestMethod]
        public void GetSqlUpdate_WithParameters()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var parameters = new Dictionary<string, object>() {{"Id", 5}};
            var skipFields = new List<string>() {"Id"};

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<ActivityOra>(connType, propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE ActivityOra SET Name=:Name, Status=:Status, IsActive=:IsActive, Created=:Created, Updated=:Updated, ActivityType=:ActivityType WHERE Id=:pId;");
        }

        [TestMethod]
        public void GetSqlUpdate_SkipFields()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };
            var skipFields = new List<string>() {"Id", "Created", "Updated"};

            // Act
            var sqlUpdate = DipMapper.GetSqlUpdate<ActivityOra>(connType, propertyInfos, parameters, skipFields);

            // Assert
            Assert.AreEqual(sqlUpdate, "UPDATE ActivityOra SET Name=:Name, Status=:Status, IsActive=:IsActive, ActivityType=:ActivityType WHERE Id=:pId;");
        }

        [TestMethod]
        public void GetSqlDelete_WithParameters()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };

            // Act
            var deleteSql = DipMapper.GetSqlDelete<ActivityOra>(connType, parameters);

            // Assert
            Assert.AreEqual(deleteSql, "DELETE FROM ActivityOra WHERE Id=:pId;");
        }

        [TestMethod]
        public void GetExtendedParameters_ParametersOnly()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };

            // Act
            var extendedParameters = DipMapper.GetExtendedParameters<ActivityOra>(connType, parameters);

            // Assert
            Assert.AreEqual(extendedParameters.Count(), 1);
            Assert.AreEqual(extendedParameters.ElementAt(0).Key, ":pId");
        }

        [TestMethod]
        public void GetExtendedParameters_PropertyInfosParametersSkipFields()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var parameters = new Dictionary<string, object>() { { "Id", 5 } };
            var skipFields = new List<string>() {"Id"};

            var activity = new ActivityOra()
            {
                Id = 3,
                Name = "Activity1",
                IsActive = true,
                Status = 4,
                Created = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared
            };

            // Act
            var extendedParameters = DipMapper.GetExtendedParameters(activity, connType, propertyInfos, skipFields, parameters);

            // Assert
            Assert.AreEqual(extendedParameters.Count(), 7);
            Assert.AreEqual(extendedParameters.ElementAt(0).Key, ":Name");
            Assert.AreEqual(extendedParameters.ElementAt(1).Key, ":Status");
            Assert.AreEqual(extendedParameters.ElementAt(2).Key, ":IsActive");
            Assert.AreEqual(extendedParameters.ElementAt(3).Key, ":Created");
            Assert.AreEqual(extendedParameters.ElementAt(4).Key, ":Updated");
            Assert.AreEqual(extendedParameters.ElementAt(5).Key, ":ActivityType");
            Assert.AreEqual(extendedParameters.ElementAt(6).Key, ":pId");
        }

        [TestMethod]
        public void GetExtendedParameters_PropertyInfosSkipFields()
        {
            // Arrange
            var conn = new OracleConnection();
            var connType = DipMapper.GetConnType(conn);
            var propertyInfos = DipMapper.GetPropertyInfos<ActivityOra>();
            var skipFields = new List<string>() { "Id" };

            var activity = new ActivityOra()
            {
                Id = 3,
                Name = "Activity1",
                IsActive = true,
                Status = 4,
                Created = DateTime.Today,
                ActivityType = ActivityTypeEnum.Shared
            };

            // Act
            var extendedParameters = DipMapper.GetExtendedParameters(activity, connType, propertyInfos, skipFields);

            // Assert
            Assert.AreEqual(extendedParameters.Count(), 6);
            Assert.AreEqual(extendedParameters.ElementAt(0).Key, ":Name");
            Assert.AreEqual(extendedParameters.ElementAt(1).Key, ":Status");
            Assert.AreEqual(extendedParameters.ElementAt(2).Key, ":IsActive");
            Assert.AreEqual(extendedParameters.ElementAt(3).Key, ":Created");
            Assert.AreEqual(extendedParameters.ElementAt(4).Key, ":Updated");
            Assert.AreEqual(extendedParameters.ElementAt(5).Key, ":ActivityType");
        }

        [TestMethod]
        public void New_ActivatorCreateInstance()
        {
            // Arrange
            var newT = DipMapper.New<ActivityOra>(false);

            // Act
            var activity = newT();
            activity.Name = "Test";

            // Assert
            Assert.AreEqual(activity.Name, "Test");
        }

        [TestMethod]
        public void New_DynamicMethod()
        {
            // Arrange
            var newT = DipMapper.New<ActivityOra>(true);

            // Act
            var activity = newT();
            activity.Name = "Test";

            // Assert
            Assert.AreEqual(activity.Name, "Test");
        }

        [TestMethod]
        public void New_DynamicMethod_Cached()
        {
            // Arrange
            var newActivity1 = DipMapper.New<ActivityOra>(true);
            var newGenericActivity1 = DipMapper.New<GenericActivityOra<int>>(true);
            var newActivity2 = DipMapper.New<ActivityOra>(true);
            var newGenericActivity2 = DipMapper.New<GenericActivityOra<int>>(true);
            var newGenericActivity3 = DipMapper.New<GenericActivityOra<ActivityOra>>(true);

            // Act
            var activity1 = newActivity1();
            activity1.Name = "Activity1";

            var genericActivity1 = newGenericActivity1();
            genericActivity1.Name = "GenericActivity1";

            var activity2 = newActivity2();
            activity2.Name = "Activity2";

            var genericActivity2 = newGenericActivity2();
            genericActivity2.Name = "GenericActivity2";

            var genericActivity3 = newGenericActivity3();
            genericActivity3.Name = "GenericActivity3";

            // Assert
            Assert.AreEqual(activity1.Name, "Activity1");
            Assert.AreEqual(activity2.Name, "Activity2");
            Assert.AreEqual(genericActivity1.Name, "GenericActivity1");
            Assert.AreEqual(genericActivity2.Name, "GenericActivity2");
            Assert.AreEqual(genericActivity3.Name, "GenericActivity3");
        }
    }
}
