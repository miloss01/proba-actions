# Registry Auth Server

A Flask-based authentication server for Docker Registry, supporting JWT token issuance and PostgreSQL-backed user and repository permissions.

## Environment Variables

You can configure the server using the following environment variables:

| Variable                        | Default Value                                              | Description                                                      |
|----------------------------------|------------------------------------------------------------|------------------------------------------------------------------|
| `UKS_AUTH_SERVICE`               | `uks-registry`                                             | Service name (audience) for issued tokens                        |
| `UKS_AUTH_ISSUER`                | `uks.registry-auth`                                        | Token issuer                                                     |
| `UKS_DATABASE_URI`               | `postgresql://admin:admin@localhost:5432/uks-database`      | PostgreSQL connection string                                      |
| `UKS_AUTH_TOKEN_VALID_FOR_SECONDS` | `3600`                                                    | Token validity period in seconds                                 |
| `UKS_AUTH_PRIVILEGED_USERS`      | `vlada:123456`                                             | Comma-separated list of privileged users (`user:pass,user2:pass2`)|
| `UKS_AUTH_HOST`                  | `0.0.0.0`                                                  | Host to bind the Flask server                                    |
| `UKS_AUTH_PORT`                  | `5001`                                                     | Port to bind the Flask server                                    |
| `UKS_AUTH_DEBUG`                 | `1`                                                        | Enable debug mode (`1` for true, `0` for false)                  |

### How to Pass Environment Variables

- **Docker CLI**: Use the `-e` flag for each variable, or `--env-file` for a file.
  
  Example:
  ```sh
  docker run -e UKS_AUTH_PORT=5001 -e UKS_AUTH_DEBUG=0 ...
  ```
  Or with an env file:
  ```sh
  docker run --env-file .env ...
  ```

- **Docker Compose**: Use the `environment` section in your service definition.

  Example:
  ```yaml
  services:
    registry-auth:
      image: your-image
      environment:
        - UKS_AUTH_PORT=5001
        - UKS_AUTH_DEBUG=0
  ```

## Certificates: Passing cert.pem and key.pem

The server requires a certificate and private key for signing JWT tokens. These files must be provided at runtime and mounted into the container at `/app/cert/cert.pem` and `/app/cert/key.pem`.

### How to Mount Certificates

- **Docker CLI**:
  ```sh
  docker run -v /path/to/host/cert:/app/cert:ro ...
  ```

- **Docker Compose**:
  ```yaml
  services:
    registry-auth:
      image: your-image
      volumes:
        - ./cert:/app/cert:ro
  ```

## Running the Server

Build the Docker image:
```sh
docker build -t registry-auth-server .
```

Run the container (example):
```sh
docker run -p 5001:5001 \
    -v "${PWD}\cert\:/app/cert/" \
    -e UKS_DATABASE_URI=postgresql://admin:admin@host.docker.internal:5432/uks-database \
    --name uks-registry-auth-server  registry-auth-server       
```

---
