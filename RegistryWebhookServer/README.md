# Registry Webhook Server

A Flask-based webhook server for Docker Registry, supporting PostgreSQL-backed event processing.

## Environment Variables

You can configure the server using the following environment variables:

| Variable                | Default Value                                              | Description                                      |
|-------------------------|------------------------------------------------------------|--------------------------------------------------|
| `UKS_WEBHOOK_HOST`      | `0.0.0.0`                                                  | Host to bind the Flask server                    |
| `UKS_WEBHOOK_PORT`      | `5002`                                                     | Port to bind the Flask server                    |
| `UKS_WEBHOOK_DEBUG`     | `True`                                                     | Enable debug mode (`True`/`False`)               |
| `UKS_DATABASE_URI`      | `postgresql://admin:admin@localhost:5432/uks-database`     | PostgreSQL connection string                     |

### How to Pass Environment Variables

- **Docker CLI**: Use the `-e` flag for each variable, or `--env-file` for a file.
  
  Example:
  ```sh
  docker run -e UKS_WEBHOOK_PORT=5002 -e UKS_WEBHOOK_DEBUG=False ...
  ```
  Or with an env file:
  ```sh
  docker run --env-file .env ...
  ```

- **Docker Compose**: Use the `environment` section in your service definition.

  Example:
  ```yaml
  services:
    registry-webhook:
      image: your-image
      environment:
        - UKS_WEBHOOK_PORT=5002
        - UKS_WEBHOOK_DEBUG=False
  ```

## Running the Server

Build the Docker image:
```sh
docker build -t registry-webhook-server .
```

Run the container (example):
```sh
docker run -p 5002:5002 \
    -e UKS_DATABASE_URI=postgresql://admin:admin@host.docker.internal:5432/uks-database \
    --name uks-registry-webhook-server registry-webhook-server
```

---
