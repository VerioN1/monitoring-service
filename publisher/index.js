const kafka = require('kafka-node');
const client = new kafka.KafkaClient({
  kafkaHost:
    process.env.ENVIRONMENT === 'local'
      ? "apps.varcode.com:29092"
      : process.env.EXTERNAL_KAFKA_ADDR,
});

const Producer = kafka.Producer;
const producer = new Producer(client);

producer.on('ready', () => {
  setInterval(() => {
    const payloads = [
      {
        topic: process.env.TOPIC,
        messages: [`${process.env.TOPIC}_message_${Date.now()}`],
      },
    ];

    producer.send(payloads, (err, data) => {
      if (err) {
        console.log(err);
      }
      console.log(data);
    });
  }, 5000);
});

producer.on('error', err => {
  console.log(err);
});
