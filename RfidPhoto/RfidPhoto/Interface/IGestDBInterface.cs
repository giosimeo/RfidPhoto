using RfidPhoto.Models;

namespace RfidPhoto.Interface
{
    public interface IGestDBInterface
    {
        Task<int> SaveImage(string id, byte[] imageData, int nImage);

        Task<List<Readers>> ListReaders();

        Task<Imballo> GetImballoAttivo(string readerId);

        Task<byte[]> GetImageImballo(string imballoid, int nImage);
    }
}
