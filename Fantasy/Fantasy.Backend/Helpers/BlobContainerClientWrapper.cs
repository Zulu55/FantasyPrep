﻿using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System.Diagnostics.CodeAnalysis;

namespace Fantasy.Backend.Helpers;

[ExcludeFromCodeCoverage(Justification = "It is a wrapper used to test other classes. There is no way to prove it.")]
public class BlobContainerClientWrapper : IBlobContainerClient
{
    private readonly BlobContainerClient _blobContainerClient;

    public BlobContainerClientWrapper(string connectionString, string containerName)
    {
        _blobContainerClient = new BlobContainerClient(connectionString, containerName);
    }

    public Task<BlobClient> GetBlobClientAsync(string name) => Task.FromResult(_blobContainerClient.GetBlobClient(name));

    public Task CreateIfNotExistsAsync() => _blobContainerClient.CreateIfNotExistsAsync();

    public Task SetAccessPolicyAsync(PublicAccessType accessType) => _blobContainerClient.SetAccessPolicyAsync(accessType);
}