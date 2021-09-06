using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.WindowsAzure.Storage;
using Xunit;

namespace FarmerConnect.Azure.Tests.Blob
{
    public class BlobStorageServiceTests : IClassFixture<BlobStorageFixture>
    {
        private readonly BlobStorageFixture _fixture;

        public BlobStorageServiceTests(BlobStorageFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "Storage")]
        public async Task CreatingAContainerReturnsTheContainerAddress()
        {
            // Arrange 
            var name = _fixture.GetContainerName();

            // Act
            var containerAddress = await _fixture.BlobStorageService.CreateContainer(name);

            // Assert
            containerAddress.Should().NotBeNullOrEmpty();
            containerAddress.Should().Contain(name);
        }

        [Fact]
        [Trait("Category", "Storage")]
        public async Task DeleteContainerReturns()
        {
            // Arrange 
            var name = _fixture.GetContainerName();
            _ = await _fixture.BlobStorageService.CreateContainer(name);

            // Act
            var result = await _fixture.BlobStorageService.DeleteContainer(name);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("./TestFile1.png")]
        [InlineData("./TestFile2.csv")]
        [Trait("Category", "Storage")]
        public async Task UploadFileReturnsARandomFileName(string filepath)
        {
            // Arrange
            var name = _fixture.GetContainerName();
            var containerAddress = await _fixture.BlobStorageService.CreateContainer(name);

            using var fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Act
            var storageName = await _fixture.BlobStorageService.Upload(new Uri(containerAddress), Path.GetFileName(filepath), fileStream);

            // Assert
            storageName.Should().NotBeNullOrEmpty();
            storageName.Should().NotBe(Path.GetFileName(filepath));
        }

        [Theory]
        [InlineData("./TestFile1.png", @"\test")]
        [InlineData("./TestFile2.csv", @"\test2\test3")]
        [Trait("Category", "Storage")]
        public async Task UploadFileWithPathReturnsARandomFileName(string filepath, string expectedFolderPath)
        {
            // Arrange
            var name = _fixture.GetContainerName();
            var containerAddress = await _fixture.BlobStorageService.CreateContainer(name);

            using var fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Act
            var storageName = await _fixture.BlobStorageService.Upload(new Uri(containerAddress), Path.Combine(expectedFolderPath, Path.GetFileName(filepath)), fileStream);

            // Assert
            storageName.Should().NotBeNullOrEmpty();
            storageName.Should().NotBe(Path.GetFileName(filepath));
            storageName.Should().StartWith(expectedFolderPath);
        }

        [Theory]
        [InlineData("./TestFile1.png", "image/png")]
        [InlineData("./TestFile2.csv", "application/octet-stream")]
        [Trait("Category", "Storage")]
        public async Task UploadedFileReturnsCorrectStreamAndContentType(string filepath, string expectedContentType)
        {
            // Arrange
            var name = _fixture.GetContainerName();
            var containerAddress = await _fixture.BlobStorageService.CreateContainer(name);

            using var fileStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var storageName = await _fixture.BlobStorageService.Upload(new Uri(containerAddress), Path.GetFileName(filepath), fileStream);

            // Act
            var (stream, contentType) = await _fixture.BlobStorageService.OpenRead(new Uri(containerAddress), storageName);

            // Assert
            contentType.Should().Be(expectedContentType);
        }

        [Fact]
        [Trait("Category", "Storage")]
        public async Task DeleteExistingFileReturnsTrue()
        {
            // Arrange
            var name = _fixture.GetContainerName();
            var containerAddress = await _fixture.BlobStorageService.CreateContainer(name);

            using var fileStream = File.Open("./TestFile1.png", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var storageName = await _fixture.BlobStorageService.Upload(new Uri(containerAddress), "TestFile1.png", fileStream);

            // Act
            var result = await _fixture.BlobStorageService.Delete(new Uri(containerAddress), storageName);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Storage")]
        public async Task DeleteNotExistingFileReturnsFalse()
        {
            // Arrange
            var name = _fixture.GetContainerName();
            var containerAddress = await _fixture.BlobStorageService.CreateContainer(name);

            // Act
            var result = await _fixture.BlobStorageService.Delete(new Uri(containerAddress), "TestFile1.png");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Storage")]
        public async Task UploadFileToAIncorrectContainerAddressThrowsAnException()
        {
            // Arrange
            var containerAddress = $"http://127.0.0.1:10000/devstoreaccount1/{Guid.NewGuid()}?sv=2018-03-28&sr=c&si=default-access&sig=J1NAQzGLkAFrP5gIHyEeKCsmz6MoBvEm1Vq%2F6ZyGoBQ%3D";

            using var fileStream = File.Open("./TestFile1.png", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            // Act / Assert
            await Assert.ThrowsAnyAsync<StorageException>(() => _fixture.BlobStorageService.Upload(new Uri(containerAddress), "TestFile1.png", fileStream));
        }
    }
}
