using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnContactObjectStorageLib.Model.Interface;
using Minio;
using Minio.DataModel;

namespace EnContactObjectStorageLib.Model
{
    public class MinioStorage : IMinioStorage
    {
        private MinioClient _minioClient;

        public string EndPoint { get; private set; }
        public string AccessKey { get; private set; }
        public string SecretKey { get; private set; }
        public string Region { get; private set; }
        public bool UseRegion
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Region);
            }
        }
        public bool Secure { get; private set; }

        private MinioStorage(string endpoint, string accesskey, string secretKey, bool secure)
        {
            EndPoint = endpoint;
            AccessKey = accesskey;
            SecretKey = secretKey;
            Secure = secure;
            Region = "";
        }

        public MinioStorage(string endpoint, string accesskey, string secretKey, bool secure, string region)
            : this(endpoint, accesskey, secretKey, secure)
        {
            Region = region;
        }

        /// <summary>
        /// Efetua a conexão com o servidor Minio/S3 a partir dos dados do construtor.
        /// </summary>
        public void Connect()
        {
            if (_minioClient != null) return;

            // TODO: Ao utilizar region na conexão, o sistema apresenta a falha abaixo, por este motivo, a region deve ser informada 
            //       nos outros pontos, porém não deve ser informada na conexão.
            // Issue relatando caso: https://github.com/minio/minio-js/issues/619
            // Notar que exemplo de connect no GitHub do Minio.DotNet não inclui region no construtor, mas apresenta nas chamadas de bucket.
            _minioClient = new MinioClient(EndPoint, AccessKey, SecretKey);
        }

        /// <summary>
        /// Valida se um balde existe de forma assincrona.
        /// </summary>
        /// <param name="bucketName">Nome da carteira a ser pesquisada.</param>
        /// <returns>Tarefa indicando sucesso ou falha ao terminar.</returns>
        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            ValidateInstance();
            return await _minioClient.BucketExistsAsync(bucketName).ConfigureAwait(false);
        }

        /// <summary>
        /// Valida se um balde existe de forma assincrona.
        /// </summary>
        /// <param name="bucketName">Nome da carteira a ser pesquisada.</param>
        /// <returns>Tarefa indicando sucesso ou falha ao terminar.</returns>
        public bool BucketExists(string bucketName)
        {
            ValidateInstance();
            return _minioClient.BucketExistsAsync(bucketName).Result;
        }

        /// <summary>
        /// Cria um balde de forma assincrona no servidor.
        /// </summary>
        /// <param name="bucketName">Nome da carteira a ser criada.</param>
        public Task MakeBucketAsync(string bucketName)
        {
            ValidateInstance();
            if (UseRegion)
            {
                return _minioClient.MakeBucketAsync(bucketName, Region);
            }

            return _minioClient.MakeBucketAsync(bucketName);
        }

        /// <summary>
        /// Cria um balde de forma assincrona no servidor.
        /// </summary>
        /// <param name="bucketName">Nome da carteira a ser criada.</param>
        public Task<ListAllMyBucketsResult> ListAllAsync()
        {
            ValidateInstance();
            return _minioClient.ListBucketsAsync();
        }

        /// <summary>
        /// Exclui um balde no servidor
        /// </summary>
        /// <param name="bucketName">Nome do balde a ser removido</param>
        public Task RemoveBucketAsync(string bucketName)
        {
            ValidateInstance();
            return _minioClient.RemoveBucketAsync(bucketName);
        }

        /// <summary>
        /// Efetua o upload de um arquivo a partir do servidor.
        /// </summary>
        /// <param name="bucketName">Nome do balde</param>
        /// <param name="objectName">Nome do objeto</param>
        /// <param name="filePath">Caminho do arquivo no servidor</param>
        /// <param name="contentType">Tipo do conteúdo do arquivo</param>
        /// <returns>Tarefa em execução.</returns>
        public Task PutObjectAsync(string bucketName, string objectName, string filePath, string contentType)
        {
            ValidateInstance();
            return _minioClient.PutObjectAsync(bucketName, objectName, filePath, contentType);
        }

        /// <summary>
        /// Efetua o upload de um arquivo a partir de um Stream em memória.
        /// </summary>
        /// <param name="bucketName">Nome do balde</param>
        /// <param name="objectName">Nome do objeto</param>
        /// <param name="data">Stream com conteúdo</param>
        /// <param name="size">Tamanho do conteúdo</param>
        /// <param name="contentType">Tipo do conteudo</param>
        /// <returns>Tarefa em execução.</returns>
        public Task PutObjectAsync(string bucketName, string objectName, Stream data, long size, string contentType)
        {
            ValidateInstance();
            return _minioClient.PutObjectAsync(bucketName, objectName, data, size, contentType);
        }

        /// <summary>
        /// Remove um arquivo contido num balde
        /// </summary>
        /// <param name="bucketName">Nome do balde onde o arquivo se encontra.</param>
        /// <param name="objectName">Nome do objeto a ser removido.</param>
        /// <returns>Tarefa em execução.</returns>
        public Task RemoveObjectAsync(string bucketName, string objectName)
        {
            ValidateInstance();
            return _minioClient.RemoveObjectAsync(bucketName, objectName);
        }

        /// <summary>
        /// Valida se um objeto existe ou não no balde.
        /// Se o arquivo não existir no servidor, será retornada uma Exception.
        /// </summary>
        /// <param name="bucketName">Nome do balde</param>
        /// <param name="objectName">Nome do objeto</param>
        /// <returns>Tarefa com o status do objeto.</returns>
        public Task<ObjectStat> StatObjectAsync(string bucketName, string objectName)
        {
            ValidateInstance();
            return _minioClient.StatObjectAsync(bucketName, objectName);
        }

        /// <summary>
        /// Valida se um objeto existe ou não no balde.
        /// </summary>
        /// <param name="bucketName">Nome do balde</param>
        /// <param name="objectName">Nome do objeto</param>
        /// <returns>True se existe e False se não existe.</returns>
        public async Task<bool> ObjectExistAsync(string bucketName, string objectName)
        {
            ValidateInstance();
            try
            {
                await _minioClient.StatObjectAsync(bucketName, objectName);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Recupera um objeto do balde.
        /// </summary>
        /// <param name="bucketName">Nome do balde.</param>
        /// <param name="objectName">Nome do objeto a ser recuperado.</param>
        /// <param name="action">Função de callback com a Stream recuperada do servidor.</param>
        public Task GetObjectAsync(string bucketName, string objectName, Action<Stream> action)
        {
            ValidateInstance();
            return _minioClient.GetObjectAsync(bucketName, objectName, action);
        }

        /// <summary>
        /// Efetua a copia de um objeto no servidor, evitando a necessidade de efetuar um upload.
        /// </summary>
        /// <param name="bucketName">Nome do balde de origem da copia.</param>
        /// <param name="objectName">Nome do objecto de origem da copia.</param>
        /// <param name="destBucketName">Balde de destino</param>
        /// <param name="destObjectName">Objeto de destino</param>
        /// <returns>Tarefa sendo executada.</returns>
        public Task CopyObjectAsync(string bucketName, string objectName, string destBucketName, string destObjectName)
        {
            ValidateInstance();
            return _minioClient.CopyObjectAsync(bucketName, objectName, destBucketName, destObjectName);
        }

        /// <summary>
        /// Obtém uma url de acesso temporário ao anexo.
        /// </summary>
        /// <param name="bucketName">Nome do balde de origem da copia.</param>
        /// <param name="objectName">Nome do objecto de origem da copia.</param>
        /// <param name="expiresInt">Tempo de expiração em segundos.</param>
        /// <param name="reqParams">Parametros adicionais do Header a serem utilizados. Suporta os Headers: response-expires, response-content-type, response-cache-control, response-content-disposition</param>
        /// <returns>Url para obtenção do arquivo.</returns>
        public async Task<string> PresignedGetObjectAsync(string bucketName, string objectName, int expiresInt, Dictionary<string, string> reqParams = null)
        {
            ValidateInstance();
            return await _minioClient.PresignedGetObjectAsync(bucketName, objectName, expiresInt, reqParams).ConfigureAwait(false);
        }

        /// <summary>
        /// Indica se deve lidar o trace para os comandos do mínio.
        /// </summary>
        /// <param name="situation">True para ligar o trace e False para desligar.</param>
        public void SetTrace(bool situation)
        {
            ValidateInstance();

            if(situation) _minioClient.SetTraceOn();
            _minioClient.SetTraceOff();
        }

        /// <summary>
        /// Valida se o bucket tem um nome válido para ser utilizado.
        /// 
        /// Bucket names should not contain upper-case letters
        /// Bucket names should not contain underscores(_)
        /// Bucket names should not end with a dash
        /// Bucket names should be between 3 and 63 characters long
        /// Bucket names cannot contain dashes next to periods(e.g., my-.bucket.com and my.-bucket are invalid)
        /// Bucket names cannot contain periods - Due to our S3 client utilizing SSL/HTTPS, Amazon documentation indicates that a bucket name cannot contain a period, otherwise you will not be able to upload files from our S3 browser in the dashboard.
        /// </summary>
        /// <param name="bucketName">Nome do bucket a ser verificado.</param>
        /// <returns>True se é valido e False se não é</returns>
        public static bool IsValidBucketName(string bucketName)
        {
            var pattern = "^[a-z0-9-]{2,63}[a-z0-9]$";
            var regex = new Regex(pattern, RegexOptions.Singleline);
            return regex.IsMatch(bucketName);
        }

        /// <summary>
        /// Valida se o nome do objeto é valido para ser utilizado.
        /// Segue as regras do AWS S3 (http://docs.aws.amazon.com/AmazonS3/latest/dev/UsingMetadata.html#object-keys)
        /// </summary>
        /// <param name="objectName">Nome do objeto para validação.</param>
        /// <returns>True se é valido e False se não é.</returns>
        public static bool IsValidObjectName(string objectName)
        {
            var pattern = @"^[0-9a-zA-Z!&$=;:+,\?\-_.*'@/]{2,500}$";
            var regex = new Regex(pattern, RegexOptions.Singleline);
            return regex.IsMatch(objectName);
        }

        /// <summary>
        /// Efetua a validação se o usuário efetuou a conexão antes de executar as ações.
        /// </summary>
        private void ValidateInstance()
        {
            if (_minioClient == null)
                throw new Exception("Não foi efetuada conexão com o servidor. Utilize a função Connect() antes de chamar as ações.");
        }

    }
}
