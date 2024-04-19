const grpc = require('@grpc/grpc-js');
const express = require('express');
const protoLoader = require('@grpc/proto-loader');
const swaggerUi = require('swagger-ui-express');
const swaggerJsdoc = require('swagger-jsdoc');
const util = require('util');
const YAML = require('yamljs');
const { loadSync, loadPackageDefinition } = require('@grpc/proto-loader');



const app = express();
const PORT = 3000;

const packageDefinition = loadSync(__dirname + '/Protos/temperature.proto');

const protoDescriptor = grpc.loadPackageDefinition(packageDefinition);

const myService = protoDescriptor.temperature.Temperature;

const client = new myService('localhost:5033', grpc.credentials.createInsecure());

const swaggerDocument = YAML.load('./openAPI.yaml');

app.use(express.json());

app.get('/getTemperatureReading/:id', (req, res) => {
    const request = { _id: req.params.id };
    client.GetTemperatureReading(request, (error, response) => {
        if (error) {
            console.log(error.code + grpc.status.INVALID_ARGUMENT );
            if (error.code === grpc.status.INVALID_ARGUMENT) {
                res.status(400).json({ error: 'Invalid ID format' });
            } else if (error.code === grpc.status.NOT_FOUND) {
                res.status(404).json({ error: 'Temperature reading not found' });
            } else {
                res.status(500).json({ error: 'Internal Server Error' });
            }
            return;
        }
        res.json(response);
    });
});

app.post('/createTemperatureReading', (req, res) => {

    const request = {
        NotedDate: req.body.NotedDate,
        Temp: req.body.Temp,
        OutIn: req.body.OutIn,
        RoomId: req.body.RoomId,
    };

    client.CreateTemperatureReading(request, (error, response) => {
        if (error) {
            res.status(500).json({ error: 'Internal Server Error' });
            return;
        }
        res.json(response);
    });
});

app.delete('/deleteTemperatureReading/:id', (req, res) => {
    const id = req.params.id;
    const request = { _id: id };
    client.DeleteTemperatureReading(request, (error, response) => {
        if (error) {
            if (error.code === grpc.status.INVALID_ARGUMENT) {
                res.status(400).json({ error: 'Invalid ID format' });
            } else if (error.code === grpc.status.NOT_FOUND) {
                res.status(404).json({ error: 'Temperature reading not found' });
            } else {
                res.status(500).json({ error: 'Internal Server Error' });
            }
            return;
        }

        res.json(response);
    });
});

app.put('/updateTemperatureReading', (req, res) => {
    const request = {
        _id: req.body.Id,
        NotedDate: req.body.NotedDate,
        Temp: req.body.Temp,
        OutIn: req.body.OutIn,
        RoomId: req.body.RoomId,
    };

    client.UpdateTemperatureReading(request, (error, response) => {
        if (error) {
            if (error.code === grpc.status.INVALID_ARGUMENT) {
                res.status(400).json({ error: 'Invalid ID format' });
            } else if (error.code === grpc.status.NOT_FOUND) {
                res.status(404).json({ error: 'Temperature reading not found' });
            } else {
                res.status(500).json({ error: 'Internal Server Error' });
            }
            return;
        }

        res.json(response);
    });
});

app.get('/aggregationTemperatureReading', (req, res) => {
    const startTimestamp = req.query.startTimestamp;
    const endTimestamp = req.query.endTimestamp;
    const operation = req.query.operation;
    const fieldName = req.query.fieldName;
    

    if (!startTimestamp || !endTimestamp || !fieldName || !operation) {
        res.status(400).json({ error: 'Missing required parameters' });
        return;
    }

    const request = {
        StartTimestamp: startTimestamp,
        EndTimestamp: endTimestamp,
        Operation: operation,
        FieldName: fieldName
    };

    console.log(request);

    client.AggregationTemperatureReading(request, (error, response) => {
        if (error) {
            if (error.code === grpc.status.INVALID_ARGUMENT) {
                res.status(400).json({ error: 'Invalid argument for aggregation' });
            } else {
                res.status(500).json({ error: 'Internal Server Error' });
            }
            return;
        }
        res.json(response);
    });
});
app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerDocument));

app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
});

