using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViviStreamWebsite.Helpers;

namespace ViviStreamWebsite.Services
{
    public static class TableStorageService
    {
        public static CloudTable ConnectToTable(string table)
        {
            if (AzureConfiguration.Configuration == null)
                AzureConfiguration.GetConfiguration();

            var connectionString = AzureConfiguration.Configuration[Constants.AzureStorageConnectionString];
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            return cloudTableClient.GetTableReference(table);
        }

        public async static Task<List<T>> RetrieveAllEntities<T> (string tableName) where T : TableEntity, new()
        {
            var table = ConnectToTable(tableName);
            var list = new List<T>();

            TableContinuationToken token = null;
            do
            {
                try
                {
                    var tableQuerySegment = await table.ExecuteQuerySegmentedAsync(new TableQuery<T>(), token);
                    list.AddRange(tableQuerySegment.Results);
                    token = tableQuerySegment.ContinuationToken;
                }
                catch(Exception ex)
                {
                    break;
                }
            }
            while (token != null);

            return list;
        }

        public async static Task<T> RetrieveEntity<T>(string pk, string rk, string tableName) where T : TableEntity
        {
            try
            {
                var table = ConnectToTable(tableName);
                var details = TableOperation.Retrieve<T>(pk, rk);
                var query = await table.ExecuteAsync(details);
                return query.Result as T;
            }
            catch(Exception ex)
            {
                return default(T);
            }
        }

        public static async Task<bool> InsertEntity(TableEntity entity, string tableName)
        {
            try
            {
                var table = ConnectToTable(tableName);
                var insert = TableOperation.Insert(entity);
                await table.ExecuteAsync(insert);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> MergeEntity(TableEntity entity, string tableName)
        {
            try
            {
                var table = ConnectToTable(tableName);
                entity.ETag = "*";
                var merge = TableOperation.Merge(entity);
                await table.ExecuteAsync(merge);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> ReplaceEntity(TableEntity entity, string tableName)
        {
            try
            {
                var table = ConnectToTable(tableName);
                entity.ETag = "*";
                var replace = TableOperation.Replace(entity);
                await table.ExecuteAsync(replace);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> DeleteEntity(TableEntity entity, string tableName)
        {
            try
            {
                var table = ConnectToTable(tableName);
                var delete = TableOperation.Delete(entity);
                await table.ExecuteAsync(delete);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
