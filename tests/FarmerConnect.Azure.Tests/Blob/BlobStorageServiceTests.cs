using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
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
        [InlineData("./TestFile1.png", "image/png")]
        [InlineData("./TestFile2.csv", "application/octet-stream")]
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
    }
}
