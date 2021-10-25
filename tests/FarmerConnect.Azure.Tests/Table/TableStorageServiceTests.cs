﻿using System;
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
        [Trait("Category", "Storage")]
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
        [Trait("Category", "Storage")]
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
        [Trait("Category", "Storage")]
        public async Task AddReturnsSuccess()
        {
            // Arrange
            var name = _fixture.GetTableName();
            var tableAddress = await _fixture.TableStorageService.CreateTable(name);

            var testObject = new TableStorageTestObject("Object Name", "1", "Test");

            // Act
            TableStorageTestObject returnObject = await _fixture.TableStorageService.Add<TableStorageTestObject>(new Uri(tableAddress), testObject);

            // Assert
            returnObject.Should().NotBeNull();
            returnObject.Name.Should().Be(testObject.Name);
        }

        [Fact]
        [Trait("Category", "Storage")]
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

        [Fact]
        [Trait("Category", "Storage")]
        public async Task AddBatchReturnsSuccess()
        {
            // Arrange
            var name = _fixture.GetTableName();
            var tableAddress = await _fixture.TableStorageService.CreateTable(name);

            var testObjects = new List<TableStorageTestObject>();
            testObjects.Add(new TableStorageTestObject("Object Name", "1", "Test"));
            testObjects.Add(new TableStorageTestObject("Object Name", "2", "Test"));

            // Act
            var resultObjects = await _fixture.TableStorageService.AddBatch<TableStorageTestObject>(new Uri(tableAddress), testObjects);

            // Assert
            resultObjects.Should().NotBeNullOrEmpty();
            resultObjects.Count().Should().Be(2);
        }

        [Fact]
        [Trait("Category", "Storage")]
        public async Task DeleteExistingBatch()
        {
            // Arrange
            var name = _fixture.GetTableName();
            var containerAddress = await _fixture.TableStorageService.CreateTable(name);

            var testObject = new TableStorageTestObject("Object Name", "1", "Test");

            // Act
            await _fixture.TableStorageService.DeleteByPartitionKey(new Uri(containerAddress), testObject.PartitionKey);

        }

        [Fact]
        [Trait("Category", "Storage")]
        public async Task AddToAIncorrectTableAddressThrowsAnException()
        {
            // Arrange
            var containerAddress = $"http://127.0.0.1:10002/devstoreaccount1/test1235?sv=2018-03-28&sr=c&si=default-access&sig=J1NAQzGLkAFrP5gIHyEeKCsmz6MoBvEm1Vq%2F6ZyGoBQ%3D";

            var testObject = new TableStorageTestObject("Object Name", "1", "Test");

            // Act / Assert
            await Assert.ThrowsAnyAsync<StorageException>(() => _fixture.TableStorageService.Add<TableStorageTestObject>(new Uri(containerAddress), testObject));
        }

        [Fact]
        [Trait("Category", "Storage")]
        public async Task GetFromDifferentTableReturnsEmpty()
        {
            // Arrange
            var tableAddress1 = await _fixture.TableStorageService.CreateTable(_fixture.GetTableName());
            var tableAddress2 = await _fixture.TableStorageService.CreateTable(_fixture.GetTableName());

            var testObject = new TableStorageTestObject("Object Name", "1", "Test");
            var storageName = await _fixture.TableStorageService.Add<TableStorageTestObject>(new Uri(tableAddress1), testObject);

            // Act
            var result = await _fixture.TableStorageService.Get<TableStorageTestObject>(new Uri(tableAddress2), "Test", "1");

            // Assert
            result.Should().BeNull();
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