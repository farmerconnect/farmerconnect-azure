using System;
using System.Collections.Generic;
using FarmerConnect.Azure.Storage.Blob;
using Microsoft.Extensions.Options;

namespace FarmerConnect.Azure.Tests.Blob
{
    public class BlobStorageFixture : IDisposable
    {
        public BlobStorageService BlobStorageService { get; private set; }
        public List<string> ContainerNameList { get; } = new List<string>();

        public BlobStorageFixture()
        {
            var options = Options.Create(new BlobStorageOptions
            {
                ConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
            });
            BlobStorageService = new BlobStorageService(options);
        }

        public string GetContainerName()
        {
            var name = Guid.NewGuid().ToString();
            ContainerNameList.Add(name);
            return name;
        }

        public void Dispose()
        {
            foreach (var containerName in ContainerNameList)
            {
                BlobStorageService.DeleteContainer(containerName).GetAwaiter().GetResult();
            }
        }
    }
}
