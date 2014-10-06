using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Directory = Lucene.Net.Store.Directory;
using Resources = AzureLucene.Properties.Resources;

namespace Mindscape.Lightspeed.Extensions
{
    public class AzureLuceneSearch : LuceneSearch
    {

        private const String BlobConnectionStringKey = "LuceneBlobStorage";
        private const String BlobContainerKey = "LuceneContainer";
        private const String DefaultContainerName = "lucenesearchindex";

        public AzureLuceneSearch()
        {
            CloudStorageAccount cloudStorageAccount;
            String blobConnectionString = CloudConfigurationManager.GetSetting(BlobConnectionStringKey);

            if (String.IsNullOrWhiteSpace(blobConnectionString))
            {
                throw new ConfigurationValueEmptyException(Resources.ConfigurationMissingKey, BlobConnectionStringKey);
            }
            Boolean isConnectionStringParsed = CloudStorageAccount.TryParse(blobConnectionString, out cloudStorageAccount);

            if (!isConnectionStringParsed)
            {
                throw new ConfigurationInvalidException(Resources.BlobConnectionStringIsInvalid);
            }


            string containerName = CloudConfigurationManager.GetSetting(BlobContainerKey);
            containerName = (containerName ?? DefaultContainerName).ToLowerInvariant();

            LuceneDirectory = new AzureDirectory(cloudStorageAccount, containerName);

        }

        public AzureLuceneSearch(Analyzer analyzer)
            : this()
        {
            Analyzer = analyzer;
        }

       

       


    }
}
