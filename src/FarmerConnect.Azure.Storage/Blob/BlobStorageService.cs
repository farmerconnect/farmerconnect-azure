using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FarmerConnect.Azure.Storage.Blob
{
    public class BlobStorageService
    {
        private const string DefaultPolicyName = "default-access";
        private readonly BlobStorageOptions _options;

        public BlobStorageService(IOptions<BlobStorageOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Upload a file to blob storage. This method uses a stream and request stream 
        /// can be piped through the server and directly to the blob storage. The downside 
        /// is that we do not check the content (Extension, FileSize, Signature, Virus Scanning).
        /// </summary>
        /// <param name="containerAddress">The full container address.</param>
        /// <param name="blobName">The name of the blob inside of the container (can also be a path).</param>
        /// <param name="content">The content of the blob as a stream.</param>
        public async Task<string> Upload(Uri containerAddress, string blobName, Stream content)
        {
            var containerReference = new CloudBlobContainer(containerAddress);
            var trustedFileNameForFileStorage = Path.GetRandomFileName();
            var blockBlobReference = containerReference.GetBlockBlobReference(trustedFileNameForFileStorage);

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(blobName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            blockBlobReference.Properties.ContentType = contentType;

            await blockBlobReference.UploadFromStreamAsync(content);

            return trustedFileNameForFileStorage;
        }

        /// <summary>
        /// Opens a stream to read from the blob.
        /// </summary>
        /// <param name="containerAddress">The full container address.</param>
        /// <param name="blobName">Name of the blob that should be opened.</param>
        public async Task<(Stream stream, string contentType)> OpenRead(Uri containerAddress, string blobName)
        {
            var containerReference = new CloudBlobContainer(containerAddress);
            var blockBlobReference = containerReference.GetBlockBlobReference(blobName);
            await blockBlobReference.FetchAttributesAsync();
            var contentType = blockBlobReference.Properties.ContentType;
            return (await blockBlobReference.OpenReadAsync(), contentType);
        }

        /// <summary>
        /// Deletes the blob from the container.
        /// </summary>
        /// <param name="containerAddress">The full container address.</param>
        /// <param name="blobName">Name of the blob that should be deleted.</param>
        public async Task<bool> Delete(Uri containerAddress, string blobName)
        {
            var containerReference = new CloudBlobContainer(containerAddress);
            var cloudBlockBlob = containerReference.GetBlockBlobReference(blobName);
            return await cloudBlockBlob.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Creates a new container if it does not exist.
        /// If the container already exists it will return the containers signature.
        /// </summary>
        /// <param name="containerName">The name of the container that will be created.</param>
        public async Task<string> CreateContainer(string containerName)
        {
            // Get a reference to the container
            var cloudStorageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var containerReference = cloudBlobClient.GetContainerReference(containerName);

            if (await containerReference.ExistsAsync())
            {
                return $"{containerReference.Uri}{containerReference.GetSharedAccessSignature(null, DefaultPolicyName)}";
            }

            // create the container
            await containerReference.CreateAsync();

            // create the shared access policy that we will use,
            // with the relevant permissions and expiry time
            var sharedAccessPolicy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read |
                  SharedAccessBlobPermissions.Write |
                  SharedAccessBlobPermissions.Delete |
                  SharedAccessBlobPermissions.List |
                  SharedAccessBlobPermissions.Add |
                  SharedAccessBlobPermissions.Create,
                SharedAccessExpiryTime = DateTimeOffset.MaxValue
            };

            // get the existing permissions (alternatively create new BlobContainerPermissions())
            var permissions = await containerReference.GetPermissionsAsync();

            // optionally clear out any existing policies on this container
            permissions.SharedAccessPolicies.Clear();

            // add in the new one
            permissions.SharedAccessPolicies.Add(DefaultPolicyName, sharedAccessPolicy);

            // save back to the container
            await containerReference.SetPermissionsAsync(permissions);

            // Now we are ready to create a shared access signature based on the stored access policy
            var containerSignature = containerReference.GetSharedAccessSignature(null, DefaultPolicyName);

            // create the URI a client can use to get access to just this container
            return $"{containerReference.Uri}{containerSignature}";
        }

        /// <summary>
        /// Deletes a given container from the storage account
        /// </summary>
        /// <param name="containerName">The name of the container that shall be deleted.</param>
        public async Task<bool> DeleteContainer(string containerName)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var containerReference = cloudBlobClient.GetContainerReference(containerName);
            return await containerReference.DeleteIfExistsAsync();
        }
    }
}
