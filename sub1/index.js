const kafka = require('kafka-node');
const client = new kafka.KafkaClient({
  kafkaHost:
    process.env.ENVIRONMENT === 'local'
      ? process.env.INTERNAL_KAFKA_ADDR
      : process.env.EXTERNAL_KAFKA_ADDR,
});
const Consumer = kafka.Consumer;

console.log(process.env.INTERNAL_KAFKA_ADDR);
console.log(process.env.TOPIC);

const consumer = new Consumer(
  client,
  [
    {
      topic: process.env.TOPIC,
      partition: 0,
    },
  ],
  {
    autoCommit: false,
  },
);

consumer.on('message', message => {
  console.log(`new message arrived - ${JSON.stringify(message)}`);
});

consumer.on('error', err => {
  console.log(`error - ${JSON.stringify(err)}`);
});
