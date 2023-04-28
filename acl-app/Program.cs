using System;
using System.Text.Json;
using System.Transactions;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.ConfidentialLedger;
using Azure.Security.ConfidentialLedger.Certificate;

namespace acl_app;

class Program
{
    static void Main(string[] args)
    {
        const string ledgerName = "laboratoriois";
        var ledgerUri = $"https://{ledgerName}.confidential-ledger.azure.com";

        var ledgerClient = new ConfidentialLedgerClient(new Uri(ledgerUri), new DefaultAzureCredential());

        Operation postOperation = ledgerClient.PostLedgerEntry(
                waitUntil: WaitUntil.Completed,
                RequestContent.Create(
                new { contents = "Hello world, Laboratorio IS Azure!" }));

        string content = postOperation.GetRawResponse().Content.ToString();
        var transactionId = postOperation.Id;
        string collectionId = "subledger:0";

        Response getByCollectionResponse = default;
        JsonElement rootElement = default;
        bool loaded = false;

        while (!loaded)
        {
            getByCollectionResponse = ledgerClient.GetLedgerEntry(transactionId, collectionId);
            rootElement = JsonDocument.Parse(getByCollectionResponse.Content).RootElement;
            loaded = rootElement.GetProperty("state").GetString() != "Loading";
        }

        string contents = rootElement
            .GetProperty("entry")
            .GetProperty("contents")
            .GetString();

        Console.WriteLine(contents);
}