#  Publisher-Subscriber messaging application using Kafka.
This is a simple application demonstrating asynchronous service to service communication. Messages is produced by the Publisher application to a topic. It is immediately received by the Subscriber application that has subscribed to the same topic. The Subscriber application simply passes the message to a (mock) API https://mockbin.com/request. 

Have used Polly library to implement retry policy. It retries(for 3 times) to send the event data to the mock API endpoint.If still there is failure, the data/mesage is published to DLQ topic (by the subscriber application).

The application is built using microservices architecture.MS Unit Test cases has been included.Console logging has been implemented.

How to Run the project?
1) Clone the code to local from GitHub.
2) Ensure Docker Desktop is running.
3) Navigate to the docker-compose.yml file path.It will be inside the Application folder.
4) From location(3) run 'docker-compose up --build' command 
5) It will create and start the following containers/images
  - zookeeper
  - kafka
  -publisher application
  -consumer application
6) Now, go to the Logs(in Docker Desktop) of publisher application to see the messages/data that is produced in topic.
7) Navigate to logs of consumer application to see the same message.

Few things done/can be done to prevent memory leakage
- Use latest version of Confluent kafka library
- Dispose kafka consumer object when no longer needed. Can be done that ujsing 'using'
- Ensure consuming message in timely manner without creating backlog. Too many messages in buffur can cause memory issues.
- limit the number of messages that are retrieved.
- set limit on amount of memory.
- avoid creating new instances of consumer object by using Singleton.
- while consuming process the messages in batches insted of one by one.
