// app.js

const { connect } = require('nats');
const { InfluxDB, Point } = require('@influxdata/influxdb-client');

const natsUrl = 'nats://nats:4222';
const influxUrl = 'http://influxdb:8086';
const influxToken = process.env.INFLUXDB_TOKEN;
const subject = 'avg';

console.log(typeof(influxUrl));
console.log(typeof(influxToken));
var obj={url:influxUrl,token:influxToken};
console.log(obj);
const writeApi = new InfluxDB(obj).getWriteApi("elfak", "junctions", 's');

async function startMicroservice() {
    console.log('started');
  const nc = await connect({ servers: natsUrl });
  console.log('connected');
  const subscription = nc.subscribe(subject);
  
  for await (const msg of subscription) {
    const data = JSON.parse(msg.data);
    console.log(data);
    console.log(1);
    console.log(data.vehicles);
    console.log(data.timestamp);
    console.log(new Date(data.timestamp));
    const point = new Point('junctions')
      .tag('junction', 1)
      .floatField('vehicles', data.vehicles)
      .timestamp(new Date(data.timestamp));
    console.log(point);

    writeApi.writePoint(point);
    writeApi
        .flush().then(() => {})
        .catch(e => {
            console.log('\nFinished ERROR: ' + e);
        });
    console.log('sent');
  }
}

startMicroservice().catch(console.error);