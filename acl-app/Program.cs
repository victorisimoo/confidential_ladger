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
        //Se crea el Confidence Ledger previamente en Azure
        const string ledgerName = "laboratoriois";
        var ledgerUri = $"https://{ledgerName}.confidential-ledger.azure.com";

        //Se utilizan las credenciales de Azure ingresadas en la Azure CLI
        var ledgerClient = new ConfidentialLedgerClient(new Uri(ledgerUri), new DefaultAzureCredential());

        //Se crea la nueva entrada
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

        //Se debe esperar hasta que la operación termine de cargar
        while (!loaded)
        {
            getByCollectionResponse = ledgerClient.GetLedgerEntry(transactionId, collectionId);
            rootElement = JsonDocument.Parse(getByCollectionResponse.Content).RootElement;
            loaded = rootElement.GetProperty("state").GetString() != "Loading";
        }

        //Se carga y se muestra el contenido de la entrada
        string contents = rootElement
            .GetProperty("entry")
            .GetProperty("contents")
            .GetString();

        Console.WriteLine(contents); // Hello world, Laboratorio IS Azure!"
    }
}