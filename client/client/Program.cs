using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, world!");

            // Create a gRPC channel
            using var channel = GrpcChannel.ForAddress("https://localhost:7172");

            // Create a client for the gRPC service
            var client = new Temperature.TemperatureClient(channel);

            // Test retrieving a temperature reading
            await TestGetTemperatureReading(client);

            // Test creating a temperature reading
            // await TestCreateTemperatureReading(client);

            // Test updating a temperature reading
            // await TestUpdateTemperatureReading(client);

            // Test deleting a temperature reading
            // await TestDeleteTemperatureReading(client);
        }
        static async Task TestDeleteTemperatureReading(Temperature.TemperatureClient client)
        {
            try
            {
                // Specify the ID of the temperature reading to delete
                var readingId = "6612f2d526f59ecdc5151c15"; // Replace with the actual ID

                // Call the DeleteTemperatureReading RPC method
                var response = await client.DeleteTemperatureReadingAsync(new ReadingId { Id = readingId });

                Console.WriteLine("DeleteTemperatureReading Successful");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Error calling DeleteTemperatureReading: {ex.Status}");
            }
        }
        static async Task TestGetTemperatureReading(Temperature.TemperatureClient client)
        {
            try
            {
                var response = await client.GetTemperatureReadingAsync(new ReadingId { Id = "6612f2d526f59ecdc5151c1a" });
                Console.WriteLine($"GetTemperatureReading Response: {response}");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Error calling GetTemperatureReading: {ex.Status}");
            }
        }

        static async Task TestAggregation(Temperature.TemperatureClient client, string fieldName, string operation, string startTimestamp, string endTimestamp)
        {
            try
            {
                var request = new AggregationRequest
                {
                    FieldName = fieldName,
                    Operation = operation,
                    StartTimestamp = startTimestamp,
                    EndTimestamp = endTimestamp
                };

                var response = await client.AggregationTemperatureReadingAsync(request);

                Console.WriteLine($"Aggregation result for field '{fieldName}', operation '{operation}': {response.Result}");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Error calling AggregationTemperatureReading: {ex.Status}");
            }
        }
    }
}
