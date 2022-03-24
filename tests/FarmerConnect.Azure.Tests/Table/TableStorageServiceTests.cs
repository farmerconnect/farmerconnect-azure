using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FarmerConnect.Azure.Storage.Table;
using FluentAssertions;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace FarmerConnect.Azure.Tests.Table
{
    public class TableStorageServiceTests : IClassFixture<TableStorageFixture>
    {
        private readonly TableStorageFixture _fixture;

        public TableStorageServiceTests(TableStorageFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "TableStorage")]
        public async Task CreatingATableReturnsTheTableAddress()
        {
            // Arrange
            var name = _fixture.GetTableName();

            // Act
            var tableAddress = await _fixture.TableStorageService.CreateTable(name);

            // Assert
            tableAddress.Should().NotBeNullOrEmpty();
            tableAddress.Should().Contain(name);
        }

        [Fact]
        [Trait("Category", "TableStorage")]
        public async Task DeleteTableReturns()
        {
            // Arrange
            var name = _fixture.GetTableName();
            _ = await _fixture.TableStorageService.CreateTable(name);

            // Act
            var result = await _fixture.TableStorageService.DeleteTable(name);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "TableStorage")]
        public async Task AddReturnsSuccess()
        {
            // Arrange
            var name = _fixture.GetTableName();
            var tableAddress = await _fixture.TableStorageService.CreateTable(name);

            var testObject = new TableStorageTestObject("Object Name", "1", "Test");

            await _fixture.TableStorageService.Add<TableStorageTestObject>(new Uri(tableAddress), testObject);

            // Act
            TableStorageTestObject returnObject =
                await _fixture.TableStorageService.Get<TableStorageTestObject>(new Uri(tableAddress), testObject.PartitionKey, testObject.RowKey);

            // Assert
            returnObject.Should().NotBeNull();
            returnObject.Name.Should().Be(testObject.Name);
        }

        [Fact]
        [Trait("Category", "TableStorage")]
        public async Task GetReturnsSuccess()
        {
            // Arrange
            var name = _fixture.GetTableName();
            var tableAddress = await _fixture.TableStorageService.CreateTable(name);

            var testObject = new TableStorageTestObject("Object Name", "1", "Test");

            await _fixture.TableStorageService.Add<TableStorageTestObject>(new Uri(tableAddress), testObject);

            // Act
            TableStorageTestObject returnObject =
                await _fixture.TableStorageService.Get<TableStorageTestObject>(new Uri(tableAddress), testObject.PartitionKey, testObject.RowKey);

            // Assert
            returnObject.Should().NotBeNull();
            returnObject.Name.Should().Be(testObject.Name);
        }

        [Fact(Skip = "Batch returns 403. Reason is probably missing support for SAS tokens: https://github.com/Azure/Azurite/issues/959")]
        [Trait("Category", "TableStorage")]
        public async Task AddBatchReturnsSuccess()
        {
            // Arrange
            var name = _fixture.GetTableName();
            var tableAddress = await _fixture.TableStorageService.CreateTable(name);
            var uri = new Uri(tableAddress);

            // Act
            var testObjects = new List<TableStorageTestObject>();
            for (int i = 0; i < 150; i++)
            {
                TableStorageTestObject testObject = new TableStorageTestObject("test object", "" + i, "test");
                testObjects.Add(testObject);
            }

            await _fixture.TableStorageService.AddBatch<TableStorageTestObject>(uri, testObjects);

            // Assert
            var resultObjects = _fixture.TableStorageService.GetByPartitionKey<TableStorageTestObject>(uri, "test");
            resultObjects.Should().NotBeNullOrEmpty();
            resultObjects.Count().Should().Be(150);
        }

        [Fact(Skip = "Batch returns 403. Reason is probably missing support for SAS tokens: https://github.com/Azure/Azurite/issues/959")]
        [Trait("Category", "TableStorage")]
        public async Task DeleteExistingBatch()
        {
            // Arrange
            var name = _fixture.GetTableName();
            var tableAddress = await _fixture.TableStorageService.CreateTable(name);
            var uri = new Uri(tableAddress);

            var testObject = new TableStorageTestObject("Object Name", "1", "test");
            var testObject2 = new TableStorageTestObject("Object Name", "2", "test");
            var testObject3 = new TableStorageTestObject("Object Name", "2", "anotherkey");

            await _fixture.TableStorageService.Add<TableStorageTestObject>(uri, testObject);
            await _fixture.TableStorageService.Add<TableStorageTestObject>(uri, testObject2);
            await _fixture.TableStorageService.Add<TableStorageTestObject>(uri, testObject3);

            // Act
            await _fixture.TableStorageService.DeleteByPartitionKey(uri, "test");

            // Assert
            var emptyResponse = _fixture.TableStorageService.GetByPartitionKey<TableStorageTestObject>(uri, "test");

            emptyResponse.Should().BeEmpty();

            var remainingEntry = _fixture.TableStorageService.Get<TableStorageTestObject>(uri, "anotherkey", "2");

            remainingEntry.Should().NotBeNull();
        }

        [Fact(Skip = "Batch returns 403. Reason is probably missing support for SAS tokens: https://github.com/Azure/Azurite/issues/959")]
        [Trait("Category", "TableStorage")]
        public async Task DeleteExistingBatchWithContinuationToken()
        {
            // Arrange
            var name = _fixture.GetTableName();
            var tableAddress = await _fixture.TableStorageService.CreateTable(name);
            var uri = new Uri(tableAddress);

            IList<TableStorageTestObject> testObjects = new List<TableStorageTestObject>();
            for (int i = 0; i < 2500; i++)
            {
                TableStorageTestObject testObject = new TableStorageTestObject("Test" + i, "" + i, "test");
                testObjects.Add(testObject);
            }

            await _fixture.TableStorageService.AddBatch<TableStorageTestObject>(uri, testObjects);

            // Act
            await _fixture.TableStorageService.DeleteByPartitionKey(uri, "test");

            // Assert

            var results = _fixture.TableStorageService.GetByPartitionKey<TableStorageTestObject>(uri, "test");

            results.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "TableStorage")]
        public async Task AddToAIncorrectTableAddressThrowsAnException()
        {
            // Arrange
            var tableAddress = $"http://127.0.0.1:10002/devstoreaccount1/test1235?sv=2018-03-28&sr=c&si=default-access&sig=J1NAQzGLkAFrP5gIHyEeKCsmz6MoBvEm1Vq%2F6ZyGoBQ%3D";

            var testObject = new TableStorageTestObject("Object Name", "1", "Test");

            // Act / Assert
            await Assert.ThrowsAnyAsync<StorageException>(() => _fixture.TableStorageService.Add<TableStorageTestObject>(new Uri(tableAddress), testObject));
        }

        [Fact]
        [Trait("Category", "TableStorage")]
        public async Task GetFromDifferentTableReturnsEmpty()
        {
            // Arrange
            var tableAddress1 = await _fixture.TableStorageService.CreateTable(_fixture.GetTableName());
            var tableAddress2 = await _fixture.TableStorageService.CreateTable(_fixture.GetTableName());

            var testObject = new TableStorageTestObject("Object Name", "1", "Test");
            await _fixture.TableStorageService.Add<TableStorageTestObject>(new Uri(tableAddress1), testObject);

            // Act
            var result = await _fixture.TableStorageService.Get<TableStorageTestObject>(new Uri(tableAddress2), "Test", "1");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "TableStorage")]
        public async Task QueryReturnsSuccess()
        {
            // Arrange
            var tableName = _fixture.GetTableName();
            var tableAddress = await _fixture.TableStorageService.CreateTable(tableName);

            var list = new List<TableStorageTestObject>()
            {
                new("first", "001", "00A"),
                new("second", "002", "00A"),
                new("third", "003", "00A"),
                new("fourth", "004", "00B"),
            };

            var inserts = list.Select(item => _fixture.TableStorageService.Add(new Uri(tableAddress), item));
            await Task.WhenAll(inserts);

            // Act
            var onlyPartitionA = _fixture.TableStorageService.Query<TableStorageTestObject>(new Uri(tableAddress), "PartitionKey eq '00A'");
            var onlyPartitionB = _fixture.TableStorageService.Query<TableStorageTestObject>(new Uri(tableAddress), "PartitionKey eq '00B'");
            var partitionAWithFilteredName = _fixture.TableStorageService.Query<TableStorageTestObject>(new Uri(tableAddress),
                "PartitionKey eq '00A' and (Name eq 'first' or Name eq 'invalid')");

            // Assert
            onlyPartitionA.Should().HaveCount(3);
            onlyPartitionB.Should().HaveCount(1);
            partitionAWithFilteredName.Should().HaveCount(1);
            partitionAWithFilteredName.Should().OnlyContain(a => a.RowKey.Equals("001"));
        }

        [Fact]
        [Trait("Category", "TableStorage")]
        public async Task QueryShouldThrowException()
        {
            // Arrange
            var tableName = _fixture.GetTableName();
            var tableAddress = await _fixture.TableStorageService.CreateTable(tableName);

            // Act
            Action act = () => _fixture.TableStorageService.Query<TableStorageTestObject>(new Uri(tableAddress), "<invalid_command>; insert into table").ToList();

            // Assert
            act.Should().Throw<StorageException>();
        }

        public class TableStorageTestObject : TableStorageEntity
        {
            public string Name { get; set; }

            public TableStorageTestObject()
            {
            }

            public TableStorageTestObject(string name, string rowKey, string partitionKey)
            {
                Name = name;
                PartitionKey = partitionKey;
                RowKey = rowKey;
            }
        }
    }
}
