<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://cdn.architect.io/logo/horizontal-inverted.png">
    <source media="(prefers-color-scheme: light)" srcset="https://cdn.architect.io/logo/horizontal.png">
    <img width="320" alt="Architect Logo" src="https://cdn.architect.io/logo/horizontal.png">
  </picture>
</p>

<p align="center">
  A dynamic microservices framework for building, connecting, and deploying cloud-native applications.
</p>

---

# Running Kafka in Docker
[Apache Kafka](https://kafka.apache.org/) is a high-throughput, high-availability, and scalable solution chosen by the world’s top companies for uses 
such as event streaming, stream processing, log aggregation, and more. Kafka runs on the platform of your choice, such as 
Kubernetes or ECS, as a cluster of one or more Kafka nodes. A Kafka cluster will be initialized with zero or more topics, 
which you can think of as message channels or queues. Clients can connect to Kafka to publish messages to topics or to 
consume messages from topics the client is subscribed to.

[Docker](https://www.docker.com/) is an application that uses virtualization to run containerized applications on a host machine. Containerization 
enables users to build, run, and test applications completely separately while still allowing them to communicate across 
a network. Importantly, containerization enables application portability so that the same application can be run on your 
local machine, a Kubernetes cluster, AWS, and more.

Both Kafka and Docker are pretty complex technologies, and it can be difficult to determine where to get started once 
you’re sure that they’re the right fit for the problem you’re solving. To keep things simple, we’ll create one producer, 
one consumer, and one Kafka instance.

This repository contains the code referenced in [this blog post](https://www.architect.io/blog/2021-01-26/kafka-docker-tutorial/).

