# Bruinen

Bruinen is a small web application built with ASP.NET Core for learning and experimenting with user authentication and account management.

The project uses PostgreSQL for storage and runs in a Docker-based development environment. It includes login, and account management features.

## Getting Started

### Prerequisites

Add the following entries to your `/etc/hosts` file:

```
127.0.0.1 auth.home.bruinen
127.0.0.1 app.home.bruinen
```

### Development Commands

Use the Makefile for common development tasks:

```sh
make help    # Show all available commands
make up      # Start development environment (PostgreSQL)
make run     # Run the application
make down    # Stop development environment
```

See `make help` for the full list of available commands.
