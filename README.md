# .NET Take Home Task - Car Park

A small .NET 10 solution implementing a car parking test for TDS, the requirements of which can be found in the "TDS - .NET README.md" file.

## Requirements
- .NET 10 SDK
- Visual Studio 2022 or later (or any other IDE that supports .NET 10)

### Setup and Run
1. Clone the repository to your local machine.
2. Open the solution in Visual Studio or your preferred IDE.
3. Run the application. It will start a web server and host the API endpoints.
4. Use the .http file to interact with the API endpoints as defined in the requirements. Alternatively, you can use tools like Postman or curl to test the endpoints.

## Project Structure
```bash
├───CarPark.Api			# Contains the API controllers and endpoint definitions.
│   ├───Models
├───CarPark.Core		# Contains the core business logic, models, and services.
│   ├───Exceptions
│   ├───Models
│   └───Services
├───CarPark.UnitTests		# Contains unit tests for the application, testing the core logic only.
│   ├───Mocks
```

## Assumptions
- The parking lot has a fixed number of spaces, which can be configured in `CarPark.Core.Configuration.cs`.
- All parking spaces are the same size (potentially, they could be different sizes in a future implementation, say motorbike parking for small vehicles, and large extended parking bays for large vehicles, such as buses / lorries.)
- Anyone parking does so properly within the lines! ;) 

## Future Improvements
- Implement a database to persist parking data instead of using in-memory storage, allowing for integration tests and data persistence across application restarts.
- Move database context into CarPark.Infrastructure to separate concerns and improve maintainability.