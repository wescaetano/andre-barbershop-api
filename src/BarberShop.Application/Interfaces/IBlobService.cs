using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Application.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadBase64Async(string base64, string originalFileName);
        Task<Stream> DownloadAsync(string fileName);
        Task DeleteAsync(string fileName);
    }
}
