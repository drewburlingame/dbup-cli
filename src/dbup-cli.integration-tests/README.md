# Integration tests

These integration tests are intended to ensure that the tool can interact with the real databases. Technically, it uses the Docker engine to run Databases, so you should install the Docker first.

Each test fixture creates a container for the given database engine. Each test runs against a different database within the container.