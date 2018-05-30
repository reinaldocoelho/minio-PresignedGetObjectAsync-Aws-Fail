using EnContactObjectStorageLib.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace sample.Controllers
{
    public class HomeController : Controller
    {
        const string objectStorageEndpoint = "s3.amazonaws.com";
        const string objectStorageAccessKey = "<CHANGE_TO_ACCESS_KEY>";
        const string objectStorageSecretKey = "<CHANGE_TO_SECRET_KEY>";
        const bool objectStorageSecure = false;
        const string objectStorageBucket = "bucket-presign-test-us-west-2";
        const string objectStorageRegion = "us-west-2";
        const string StorageObjectSfWar = "folder/SFWar.png";
        const string StorageObjectAzure = "folder/Azure-Analytics-SQL cheat sheet.pdf";

        public ActionResult Index()
        {
            // START HERE


            return View();
        }

        public async Task<ActionResult> DownloadImageAsync()
        {
            var client = new MinioStorage(objectStorageEndpoint, objectStorageAccessKey, objectStorageSecretKey, objectStorageSecure, objectStorageRegion);
            client.Connect();

            // Verify if exists Bucket and create if need
            if (!await client.BucketExistsAsync(objectStorageBucket))
            {
                await client.MakeBucketAsync(objectStorageBucket);
            }

            // Verify if exists file
            var sfwarPath = Server.MapPath("~/resources/SFWar.png");
            if (!await client.ObjectExistAsync(objectStorageBucket, StorageObjectSfWar))
            {
                await client.PutObjectAsync(objectStorageBucket, StorageObjectSfWar, sfwarPath, "image/png");
            }

            // Get 15 minutes URI
            var parameters = new Dictionary<string, string>
            {
                { "response-content-disposition", $"inline;filename=SFWar.png;" },
                { "response-content-type", "image/png" }
            };
            var tempUri = await client.PresignedGetObjectAsync(objectStorageBucket, StorageObjectSfWar, 60, parameters);

            // Redirect to Url
            return Redirect(tempUri);
        }

        public async Task<ActionResult> DownloadPDFAsync()
        {
            var client = new MinioStorage(objectStorageEndpoint, objectStorageAccessKey, objectStorageSecretKey, objectStorageSecure, objectStorageRegion);
            client.Connect();

            // Verify if exists Bucket and create if need
            if (!await client.BucketExistsAsync(objectStorageBucket))
            {
                await client.MakeBucketAsync(objectStorageBucket);
            }

            // Verify if exists file
            var azurePath = Server.MapPath("~/resources/Azure-Analytics-SQL cheat sheet.pdf");
            if (!await client.ObjectExistAsync(objectStorageBucket, StorageObjectAzure))
            {
                await client.PutObjectAsync(objectStorageBucket, StorageObjectAzure, azurePath, "application/pdf");
            }

            // Get 15 minutes URI
            var parameters = new Dictionary<string, string>
            {
                { "response-content-disposition", $"inline;filename=Azure-Analytics-SQL cheat sheet.pdf;" },
                { "response-content-type", "application/pdf" }
            };
            var tempUri = await client.PresignedGetObjectAsync(objectStorageBucket, StorageObjectAzure, 60, parameters);

            // Redirect to Url
            return Redirect(tempUri);
        }

    }
}