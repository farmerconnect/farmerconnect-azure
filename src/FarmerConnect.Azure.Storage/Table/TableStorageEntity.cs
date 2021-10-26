using Microsoft.Azure.Cosmos.Table;

namespace FarmerConnect.Azure.Storage.Table
{
    public abstract class TableStorageEntity : TableEntity
    {
        //helper class to not require consumers to depend on "Microsoft.Azure.Cosmos.Table.TableEntity"
    }
}
