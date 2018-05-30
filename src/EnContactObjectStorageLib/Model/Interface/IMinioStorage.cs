using Minio.DataModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EnContactObjectStorageLib.Model.Interface
{
    public interface IMinioStorage
    {
        string EndPoint { get; }
        string AccessKey { get; }
        string SecretKey { get; }
        string Region { get; }
        bool Secure { get; }

        void Connect();
        Task<bool> BucketExistsAsync(string bucketName);
        Task MakeBucketAsync(string bucketName);
        Task<ListAllMyBucketsResult> ListAllAsync();
        Task RemoveBucketAsync(string bucketName);
        Task PutObjectAsync(string bucketName, string objectName, string filePath, string contentType);
        Task RemoveObjectAsync(string bucketName, string objectName);
        Task<ObjectStat> StatObjectAsync(string bucketName, string objectName);
        Task<bool> ObjectExistAsync(string bucketName, string objectName);
        Task GetObjectAsync(string bucketName, string objectName, Action<Stream> action);

    }
}
