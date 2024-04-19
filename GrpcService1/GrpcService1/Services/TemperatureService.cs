using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService1.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GrpcService1.Services
{
    public class TemperatureService : Temperature.TemperatureBase
    {
        private readonly IMongoCollection<TemperatureReadingModel> _collection;

        public TemperatureService(IMongoDatabase database)
        {
            _collection = database.GetCollection<TemperatureReadingModel>("tempdata");
        }

        public override async Task<Poruka> CreateTemperatureReading(TemperatureReading request, ServerCallContext context)
        {
            try
            {
                var newValue = new TemperatureReadingModel
                {
                    noted_date = request.NotedDate,
                    temp = (int)request.Temp,
                    outin = request.OutIn,
                    room_id = request.RoomId
                };
                await _collection.InsertOneAsync(newValue);

                return await Task.FromResult(new Poruka
                {
                    Text = "Uspesno dodavanje! "
                });
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Error performing CreateTemperatureReading: {ex.Message}"));
            }
        }


        public override async Task<TemperatureReading> GetTemperatureReading(ReadingId request, ServerCallContext context)
        {
            
            var Id = request.Id;

            Console.WriteLine(Id);

            ObjectId objectId;
            try
            {
                objectId = ObjectId.Parse(Id);
            }
            catch (FormatException)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ObjectId format"));
            }

            var filter = Builders<TemperatureReadingModel>.Filter.Eq(x => x._id, objectId);

            var temperature = await _collection.Find(filter).FirstOrDefaultAsync();

            if (temperature == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Not found"));
            }


            return await Task.FromResult(new TemperatureReading
            {
                Id = temperature._id.ToString(),
                NotedDate = temperature.noted_date,
                OutIn = temperature.outin,
                RoomId = temperature.room_id,
                Temp = temperature.temp

            }); ;           
}


        public override async Task<Poruka> UpdateTemperatureReading(TemperatureReading request, ServerCallContext context)
        {
                var Id = request.Id;
                ObjectId objectId;
                try
                {
                    objectId = ObjectId.Parse(Id);
                }
                catch (FormatException)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ObjectId format"));
                }

                var filter = Builders<TemperatureReadingModel>.Filter.Eq(x => x._id, objectId);

                var temp = await _collection.Find(filter).FirstOrDefaultAsync();

                if (temp == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "There is no electricity consumption with the same Id in database"));
                }

                temp.room_id = request.RoomId;
                temp.noted_date = request.NotedDate;
                temp.outin = request.OutIn;
                temp.temp = (int)request.Temp;

                await _collection.ReplaceOneAsync(filter, temp);

                return new Poruka
                {
                    Text = "Uspesno Updateovano"
                };
        }

        public override async Task<Empty> DeleteTemperatureReading(ReadingId request, ServerCallContext context)
        {
            
                var Id = request.Id;

                ObjectId objectId;
                try
                {
                    objectId = ObjectId.Parse(Id);
                }
                catch (FormatException)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ObjectId format"));
                }
                var filter = Builders<TemperatureReadingModel>.Filter.Eq(x => x._id, objectId);

                var electricityConsumptionValue = await _collection.FindOneAndDeleteAsync<TemperatureReadingModel>(filter);

                if (electricityConsumptionValue == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "Temperature reading with that Id doesn't exist!"));
                }

                return new Empty();
            
           
        }

        public override async Task<AggregationValue> AggregationTemperatureReading(AggregationRequest request, ServerCallContext context)
        {
            try
            {
                var startTimestamp = DateTime.Parse(request.StartTimestamp);
                var endTimestamp = DateTime.Parse(request.EndTimestamp);

                var projection = Builders<TemperatureReadingModel>.Projection.Include(request.FieldName);
                var filter = Builders<TemperatureReadingModel>.Filter.And(
                 Builders<TemperatureReadingModel>.Filter.Gte("noted_date", startTimestamp),
                 Builders<TemperatureReadingModel>.Filter.Lte("noted_date", endTimestamp)
             );

                var values = await _collection.Find(filter)
                                              .Project(projection)
                                              .ToListAsync();

                var fieldValues = new List<double>();

                foreach (var document in values)
                {
                    if (document.TryGetValue(request.FieldName, out BsonValue fieldValue))
                    {
                        if (fieldValue.IsNumeric)
                        {
                            if (fieldValue.IsInt32)
                            {
                                fieldValues.Add(fieldValue.AsInt32);
                            }
                            else if (fieldValue.IsDouble)
                            {
                                fieldValues.Add(fieldValue.AsDouble);
                            }
                        }
                    }
                }

                double result;

                switch (request.Operation.ToLower())
                {
                    case "min":
                        if (fieldValues.Any())
                        {
                            result = fieldValues.Min();
                        }
                        else
                        {
                            throw new RpcException(new Status(StatusCode.InvalidArgument, "No values found for minimum operation"));
                        }
                        break;
                    case "max":
                        if (fieldValues.Any())
                        {
                            result = fieldValues.Max();
                        }
                        else
                        {
                            throw new RpcException(new Status(StatusCode.InvalidArgument, "No values found for maximum operation"));
                        }
                        break;
                    case "avg":
                        if (fieldValues.Any())
                        {
                            result = fieldValues.Average();
                        }
                        else
                        {
                            throw new RpcException(new Status(StatusCode.InvalidArgument, "No values found for average operation"));
                        }
                        break;
                    case "sum":
                        result = fieldValues.Sum();
                        break;
                    default:
                        throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid aggregation operation type"));
                }

                return new AggregationValue
                {
                    Result = result
                };
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Error performing aggregation: {ex.Message}"));
            }
        }


    }
}