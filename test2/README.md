# Instructions for the Test

Prepare your environment by following these steps:

1. Run `npm install` in the `test2\Client` directory.
2. Run `dotnet build` in the `test2\Server` and `test2\ServerTests` directories.

To check the current issue, run `dotnet test` in the `test2\ServerTests` directory. You will see that the test fails.

To see the code running, follow these steps:

1. In the `test2\Server` directory, run `dotnet run` and observe the output.
2. Then, in the `test2\Client` directory, run `node client.js`. Ensure that the server is started before the client attempts to connect.

## Brief Explanation

This C# application acts as a server to which clients (Node.js application) connect to subscribe to notifications for a given topic. In this example, the server provides notifications about CPU usage every 5 seconds and about TIME every 10 seconds.

Use ChatGPT 4o-mini to solve this test, remember to use https://chatgpt.com/ website and share your conversation history.

## Problem Description

There is a bug in the code: if you run the test, you will see that it fails. Additionally, if you run the client, you will notice it is receiving notifications for topics to which it did not subscribe.

## Task

Your task is to fix the code so that:

1. The test passes successfully.
2. The client receives only the notifications for the topics it has subscribed to.
