using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;

namespace FarmerConnect.Azure.Storage.Table
{
    public class TableStorageService
    {
        private const int TableBatchMaxEntries = 100;
        private const string DefaultPolicyName = "default-access";
        private readonly TableStorageOptions _options;

        public TableStorageService(IOptions<TableStorageOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Adds a batch of entities to the table
        /// </summary>
        /// <param name="tableAddress">The full table address.</param>
        /// <param name="listOfEntities">The list of entries to be added.
        /// Object must be an instance of "TableStorageEntity" or implement "Microsoft.Azure.Cosmos.Table.ITableEntity"</param>
        public Task<IEnumerable<T>> AddBatch<T>(Uri tableAddress, IEnumerable<T> listOfEntities) where T : TableStorageEntity, new()
        {
            var tableReference = new CloudTable(tableAddress);

            int rowOffset = 0;

            while (rowOffset < listOfEntities.Count())
            {
                var rows = listOfEntities.Skip(rowOffset).Take(TableBatchMaxEntries).ToList();
                rowOffset += rows.Count;

                var task = Task.Run(() =>
                {
                    var batch = new TableBatchOperation();

                    foreach (var row in rows)
                    {
                        batch.InsertOrReplace(row);
                    }
                    tableReference.ExecuteBatchAsync(batch);
                });
            }

            return Task.FromResult(listOfEntities);
        }

        /// <summary>
        /// Adds an entity to the table
        /// </summary>
        /// <param name="tableAddress">The full table address.</param>
        /// <param name="value">the object to add.
        /// Object must be an instance of "TableStorageEntity" or implement "Microsoft.Azure.Cosmos.Table.ITableEntity"</param>
        public async Task<T> Add<T>(Uri tableAddress, T value) where T : TableStorageEntity, new()
        {
            var tableReference = new CloudTable(tableAddress);

            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(value);
            TableResult result = await tableReference.ExecuteAsync(insertOrReplaceOperation);

            return result.Result as T;
        }

        /// <summary>
        /// Gets all entries that match the provided partitionKey
        /// </summary>
        /// <param name="tableAddress">The full table address.</param>
        /// <param name="partitionKey">value of the partition key from the entries.</param>
        /// <returns>Returns a "TableStorageEntity"</returns>
        public IEnumerable<T> GetByPartitionKey<T>(Uri tableAddress, string partitionKey) where T : TableStorageEntity, new()
        {

            var tableReference = new CloudTable(tableAddress);

            var query = new TableQuery<T>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            return tableReference.ExecuteQuery<T>(query);
        }

        /// <summary>
        /// Gets an entry from the table that matches the provided partitionKey and rowKey
        /// </summary>
        /// <param name="tableAddress">The full table address.</param>
        /// <param name="partitionKey">value of the partition key in which the entry is contained.</param>
        /// <param name="rowKey">value of the row key of the entry.</param>
        /// <returns>Returns a "TableStorageEntity"</returns>
        public async Task<T> Get<T>(Uri tableAddress, string partitionKey, string rowKey) where T : TableStorageEntity, new()
        {
            var tableReference = new CloudTable(tableAddress);

            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            TableResult result = await tableReference.ExecuteAsync(retrieveOperation);

            return result.Result as T;
        }

        /// <summary>
        /// Deletes all entries from a table that match the provided partition key.
        /// </summary>
        /// <param name="tableAddress">The full table address.</param>
        /// <param name="partitionKey">value of the partition key from which the entries should be deleted.</param>
        public async Task DeleteByPartitionKey(Uri tableAddress, string partitionKey)
        {
            var tableReference = new CloudTable(tableAddress);

            var partitionScanQuery =
                new TableQuery()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            TableContinuationToken token = null;

            IList<Task<TableBatchResult>> tableBatchResults = new List<Task<TableBatchResult>>();

            do
            {
                var segment = await tableReference.ExecuteQuerySegmentedAsync(partitionScanQuery, token);
                token = segment.ContinuationToken;

                int rowOffset = 0;
                while (rowOffset < segment.Count())
                {
                    var rows = segment.Skip(rowOffset).Take(TableBatchMaxEntries).ToList();
                    rowOffset += rows.Count;

                    var batchOperation = new TableBatchOperation();

                    foreach (var row in rows)
                    {
                        batchOperation.Delete(row);
                    }
                    tableBatchResults.Add(tableReference.ExecuteBatchAsync(batchOperation));
                }
            }
            while (token != null);

            await Task.WhenAll(tableBatchResults);
        }

        /// <summary>
        /// Creates a new table if it does not exist.
        /// If the table already exists it will return the table's signature.
        /// </summary>
        /// <param name="tableName">The name of the table that will be created.</param>
        public async Task<string> CreateTable(string tableName)
        {
            // Get a reference to the table
            var cloudStorageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var tableReference = cloudTableClient.GetTableReference(tableName);

            if (await tableReference.ExistsAsync())
            {
                return $"{tableReference.Uri}{tableReference.GetSharedAccessSignature(null, DefaultPolicyName)}";
            }

            // create the container
            await tableReference.CreateAsync();

            // create the shared access policy that we will use,
            // with the relevant permissions and expiry time
            TimeSpan clockSkew = TimeSpan.FromMinutes(15d);
            var sharedAccessPolicy = new SharedAccessTablePolicy
            {
                SharedAccessStartTime = DateTimeOffset.UtcNow.Subtract(clockSkew),
                Permissions = SharedAccessTablePermissions.Query |
                  SharedAccessTablePermissions.Add |
                  SharedAccessTablePermissions.Delete |
                  SharedAccessTablePermissions.Update,
                SharedAccessExpiryTime = DateTimeOffset.MaxValue
            };

            // get the existing permissions (alternatively create new BlobContainerPermissions())
            var permissions = await tableReference.GetPermissionsAsync();

            // optionally clear out any existing policies on this container
            permissions.SharedAccessPolicies.Clear();

            // add in the new one
            permissions.SharedAccessPolicies.Add(DefaultPolicyName, sharedAccessPolicy);

            // save back to the container
            await tableReference.SetPermissionsAsync(permissions);

            // Now we are ready to create a shared access signature based on the stored access policy
            var containerSignature = tableReference.GetSharedAccessSignature(null, DefaultPolicyName);

            // create the URI a client can use to get access to just this container
            return $"{tableReference.Uri}{containerSignature}";
        }

        /// <summary>
        /// Deletes a given table from the storage account
        /// </summary>
        /// <param name="tableName">The name of the table that shall be deleted.</param>
        public async Task<bool> DeleteTable(string tableName)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var tableReference = cloudTableClient.GetTableReference(tableName);
            return await tableReference.DeleteIfExistsAsync();
        }
    }
}
