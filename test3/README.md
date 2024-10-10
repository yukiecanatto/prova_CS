# Instructions for TEST 3

This test builds upon TEST 2, adding a new feature for random jokes.

Do not use AI assistance to solve this test.

## Setup and Running

1. Server Setup:
   - Navigate to the `test3/Server` directory.
   - Run `dotnet build` to build the project.
   - Start the server with `dotnet run`.

2. Client Setup:
   - Navigate to the `test3/Client` directory.
   - Run `npm install` to install dependencies.
   - Start the client with `node client.js`.

3. Running Tests:
   - Navigate to the `test3/ServerTests` directory.
   - Run `dotnet test` to execute all unit tests.

## New Feature: Random Jokes

1. Implement a new `JokesProducer` class in the Server project:
   - Implement logic to generate and send random jokes every 5 seconds.
   - You can use a predefined list of jokes or integrate with a jokes API.

2. Modify the Client (`client.js`):
   - Subscribe to the "JOKES" topic.
   - Ensure it only receives "JOKES" topic messages and nothing else, fix any issues on Server if necessary.
   - Display received jokes in the console.

4. Create a Unit Test:
   - Add a new test method in `ServerTests/UnitTests.cs` for `JokesProducer`.
   - Verify that the producer logs messages correctly.

## Expected Behavior

- The server should now broadcast random jokes every 5 seconds on the "JOKES" topic.
- The client should receive and display these jokes.
- All unit tests, including the new test for `JokesProducer`, should pass.

## Verification

1. Run the server and client.
2. Observe that the client receives and displays jokes.
3. Run `dotnet test` in the ServerTests directory and ensure all tests pass.

Good luck with the implementation!
