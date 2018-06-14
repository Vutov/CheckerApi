namespace CheckerApi.Services.Interfaces
{
    public interface ICompressService
    {
        byte[] Zip(string data, string innerZipFile);
    }
}
