using Microsoft.Extensions.Configuration;
using BarberShop.Application.Interfaces;
using BarberShop.Infra.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BarberShop.Application.Services
{
    public class BlobService : BaseService, IBlobService
    {
        private readonly BlobContainerClient _containerClient;
        public BlobService(IRepositoryFactory repositoryFactory, IConfiguration configuration) : base(repositoryFactory)
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            var containerName = configuration["AzureBlobStorage:ContainerName"];

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
                throw new InvalidOperationException("As configurações do Azure Blob Storage estão ausentes ou inválidas.");

            _containerClient = new BlobContainerClient(connectionString, containerName);
        }
        public async Task<string> UploadBase64Async(string base64, string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(base64))
                throw new ArgumentException("O conteúdo Base64 é  obrigatório.");

            if (string.IsNullOrWhiteSpace(originalFileName))
                throw new ArgumentException("O nome do arquivo é obrigatório.");

            string contentType = "application/octet-stream"; // padrão genérico
            string extension = Path.GetExtension(originalFileName); // manter extensão

            // Detecta MIME type e limpa Base64
            if (base64.Contains(","))
            {
                var parts = base64.Split(',');
                var meta = parts[0]; // ex: "data:image/png;base64"
                base64 = parts[1]; // remove cabeçalho

                if (meta.Contains(";base64"))
                {
                    var mime = meta.Replace("data:", "").Replace(";base64", "");
                    if (!string.IsNullOrWhiteSpace(mime))
                        contentType = mime;

                    // tenta pegar extensão do MIME se não tiver
                    if (string.IsNullOrWhiteSpace(extension) && mime.Contains("/"))
                        extension = "." + mime.Split('/')[1];
                }
            }

            var fileBytes = Convert.FromBase64String(base64);

            // Gera nome único
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            var blobClient = _containerClient.GetBlobClient(uniqueFileName);

            using (var stream = new MemoryStream(fileBytes))
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });
            }

            return blobClient.Uri.ToString();
        }
        public async Task<Stream> DownloadAsync(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                var download = await blobClient.DownloadAsync();
                return download.Value.Content;
            }

            throw new FileNotFoundException("Arquivo não encontrado.");
        }
        public async Task DeleteAsync(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
