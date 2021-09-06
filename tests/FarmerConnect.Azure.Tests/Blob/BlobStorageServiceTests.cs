using System;
using System.Threading.Tasks;
using FarmerConnect.Azure.Storage.Blob;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FarmerConnect.Azure.Tests.Blob
{
    /// <summary>
    /// NOTE: These tests require a blob storage account.
    /// </summary>
    public class BlobStorageServiceTests
    {
        private readonly BlobStorageService _blobStorageService;

        public BlobStorageServiceTests()
        {
            var options = Options.Create(new BlobStorageOptions
            {
                ConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
            });
            _blobStorageService = new BlobStorageService(options);
        }

        [Fact]
        public async Task Test1()
        {
            // Arrange 
            var name = Guid.NewGuid().ToString();

            // Act
            var containerAddress = await _blobStorageService.CreateContainer(name);

            // Assert
            containerAddress.Should().NotBeNullOrEmpty();
        }
    }
}
