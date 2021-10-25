using System;
using System.Collections.Generic;
using FarmerConnect.Azure.Storage.Table;
using Microsoft.Extensions.Options;

namespace FarmerConnect.Azure.Tests.Table
{
    public class TableStorageFixture : IDisposable
    {
        public TableStorageService TableStorageService { get; private set; }
        public List<string> TableNameList { get; } = new List<string>();

        public TableStorageFixture()
        {
            var options = Options.Create(new TableStorageOptions
            {
                ConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
            });
            TableStorageService = new TableStorageService(options);
        }

        public string GetTableName()
        {
            var name = "test" + Guid.NewGuid().ToString().Replace("-", "");
            TableNameList.Add(name);
            return name;
        }

        public void Dispose()
        {
            foreach (var tableName in TableNameList)
            {
                TableStorageService.DeleteTable(tableName).GetAwaiter().GetResult();
            }
        }
    }
}
