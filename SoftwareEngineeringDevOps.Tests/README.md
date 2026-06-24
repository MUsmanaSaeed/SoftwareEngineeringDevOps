# Test Suite Documentation

## Overview
This comprehensive test suite ensures the quality and reliability of the SoftwareEngineeringDevOps Blazor Server application. The tests are designed to run automatically via GitHub Actions prior to Render deployment.

## Test Framework Stack
- **xUnit**: Primary testing framework
- **FluentAssertions**: Readable assertions
- **bUnit**: Blazor component testing
- **Playwright**: End-to-end browser automation
- **Moq**: Mocking framework for dependencies
- **.NET 10**: Target framework

## Test Structure

### 1. Unit Tests (`/UnitTests`)
Isolated tests for individual components with mocked dependencies.

#### Domain Tests
- `BricksMediatorTests.cs`: Tests CRUD operations for bricks with repository mocking
  - Happy path scenarios
  - Null/invalid input handling
  - Boundary value testing

#### Validation Tests
- `InputValidatorTests.cs`: Comprehensive validation rule testing
  - Required field validation
  - Format validation (email, phone)
  - Range and boundary checks
  - Password strength validation
  - Multiple error scenarios

#### ViewModel Tests
- `BricksViewModelTests.cs`: Business logic and state management
  - CRUD operation flows
  - Validation integration
  - Role-based authorization
  - Search and filtering
  - Edge case handling

### 2. Integration Tests (`/IntegrationTests`)

#### Component Tests (`/Components`)
- `BricksComponentTests.cs`: bUnit tests for Blazor components
  - Component rendering
  - User interactions
  - Authentication scenarios
  - Error handling

### 3. End-to-End Tests (`/E2E`)
Playwright-based browser automation tests running headlessly.

#### Authentication E2E
- `AuthenticationE2ETests.cs`: Full authentication flows
  - Login (valid/invalid credentials)
  - Registration
  - Logout
  - Protected route access
  - XSS prevention

#### CRUD E2E
- `BricksCRUDE2ETests.cs`: Complete CRUD workflows
  - Create with valid/invalid data
  - Read and search operations
  - Update operations
  - Delete with dependency checking
  - Authorization enforcement
  - Boundary value testing
  - Network error simulation

## Test Coverage

### Happy Paths ✅
- Successful CRUD operations
- Valid form submissions
- Expected state changes
- User authentication flows

### Boundary Conditions ✅
- Minimum/maximum input limits (0, 10000mm for dimensions)
- Null and empty data handling
- Collection boundaries
- Edge values (0, negative numbers, maximum decimals)

### Error Handling ✅
- Database connection failures (mocked)
- Invalid input validation
- Dependency constraint violations (e.g., deleting bricks with orders)
- Network timeouts (simulated)
- Authentication failures

### Security/Authentication ✅
- Unauthorized access attempts
- Role-based access control (Admin, Editor, Standard)
- XSS prevention testing
- Protected component/page access

## Running Tests Locally

### All Tests
```bash
dotnet test SoftwareEngineeringDevOps.Tests/SoftwareEngineeringDevOps.Tests.csproj
```

### Unit Tests Only
```bash
dotnet test --filter "FullyQualifiedName~UnitTests"
```

### Integration Tests Only
```bash
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

### E2E Tests Only (requires running application)
```bash
# Start the application first
cd SoftwareEngineeringDevOps
dotnet run

# In another terminal
dotnet test --filter "FullyQualifiedName~E2E"
```

### With Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## CI/CD Integration

### GitHub Actions Workflows

#### 1. `test-suite.yml`
Runs on every push and pull request:
- Unit tests (isolated, fast)
- Integration tests (with mocked services)
- E2E tests (with PostgreSQL service container)
- Generates test reports and coverage

#### 2. `pre-deployment-tests.yml`
Runs before Render deployment:
- Full test suite validation
- Acts as deployment gate
- Only allows deployment if all tests pass

### Running in CI
Tests are configured to run headlessly:
- No UI dependencies
- Isolated test environment
- Mocked external dependencies
- Deterministic results

## Test Configuration

### `xunit.runner.json`
- Parallel test execution enabled
- Optimized for CI/CD performance
- Class and method display names

### Environment Variables
- `ASPNETCORE_ENVIRONMENT=Test`
- `BASE_URL=https://acs-brick-management.onrender.com` (for E2E)
- PostgreSQL connection strings (CI only)

## Best Practices Followed

1. **AAA Pattern**: Arrange, Act, Assert clearly separated
2. **Isolation**: Tests don't depend on external state
3. **Mocking**: External dependencies mocked for determinism
4. **Naming**: Descriptive test names following `Method_Should_Scenario_When_Condition`
5. **Coverage**: Happy paths, edge cases, and error scenarios
6. **Speed**: Fast unit tests, selective E2E tests
7. **Maintainability**: Shared test utilities and data factories

## Maintenance

### Adding New Tests
1. Follow existing structure and naming conventions
2. Use `MockDataFactory` for test data generation
3. Add assertions using FluentAssertions for readability
4. Update this README with significant additions

### Updating Tests
When modifying application code:
1. Update corresponding tests first (TDD approach)
2. Ensure all existing tests pass
3. Add new tests for new functionality
4. Run full suite before committing

## Troubleshooting

### Tests Failing Locally
1. Ensure .NET 10 SDK is installed
2. Restore packages: `dotnet restore`
3. Clean and rebuild: `dotnet clean && dotnet build`

### E2E Tests Timing Out
1. Increase timeout values in test attributes
2. Check application startup time
3. Verify PostgreSQL is running (if needed)

### Playwright Issues
1. Install browsers: `playwright install chromium --with-deps`
2. Ensure headless mode is enabled
3. Check for conflicting processes on port 5000

## Future Enhancements

- [ ] Performance testing
- [ ] Load testing
- [ ] Additional component coverage
- [ ] Visual regression testing
- [ ] Mutation testing
- [ ] Integration with code quality tools

## Support

For issues or questions about the test suite:
1. Check test output logs
2. Review GitHub Actions workflow runs
3. Consult existing test examples
4. Update test documentation as needed
