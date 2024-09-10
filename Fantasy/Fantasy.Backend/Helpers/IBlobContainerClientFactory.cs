namespace Fantasy.Backend.Helpers;

public interface IBlobContainerClientFactory
{
    IBlobContainerClient CreateBlobContainerClient(string connectionString, string containerName);
}