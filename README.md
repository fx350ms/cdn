# FileService.Api

A lightweight file management service built with .NET 9, designed for internal use with minimal resource consumption.

## Features

- File upload/download/delete with metadata storage
- JWT-based authentication with fixed token
- MongoDB for metadata storage
- Local disk storage for files
- Minimal resource usage
- Fast startup and simple deployment

## Prerequisites

- .NET 9 SDK
- MongoDB instance

## Configuration

Update [appsettings.json](./appsettings.json) with your MongoDB connection string:

```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "FileServiceDb"
  },
  "Jwt": {
    "Key": "SUPER_SECRET_KEY_123456789",
    "Issuer": "InternalSystem",
    "Audience": "InternalServices",
    "ExpireMinutes": 43200
  }
}
```

## Getting Started

### Running locally

1. Install dependencies: `dotnet restore`
2. Start MongoDB (if running locally)
3. Run the application: `dotnet run`
4. Navigate to `https://localhost:5001/swagger` for API documentation

### Obtaining a JWT token

Make a POST request to `/api/auth/token` to get your JWT token:

```bash
curl -X POST https://localhost:5001/api/auth/token
```

### Using the API

All file operations require authentication. Include the JWT token in the Authorization header:

```
Authorization: Bearer {your-token-here}
```

## API Endpoints

- `POST /api/auth/token` - Get JWT token
- `POST /api/files/upload` - Upload a file
- `GET /api/files/{id}` - Download a file
- `DELETE /api/files/{id}` - Delete a file
- `GET /api/files/meta/{id}` - Get file metadata
- `GET /health` - Health check

## File Storage

Uploaded files are stored in the following structure:
```
/uploads/{serviceName}/{yyyy}/{MM}/{dd}/{guid.ext}
```

For example:
```
/uploads/OrderService/2026/04/28/abc123.png
```

## Supported File Types

- Images: `.jpg`, `.jpeg`, `.png`
- Documents: `.pdf`, `.docx`, `.xlsx`
- Archives: `.zip`

Maximum file size: 20MB

## Docker Deployment

Build and run with Docker:

```bash
# Build the image
docker build -t file-service .

# Run the container (ensure MongoDB is accessible)
docker run -p 8080:80 -e Mongo__ConnectionString="mongodb://your-mongo-host:27017" file-service
```