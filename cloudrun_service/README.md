# Cloud Run Service for GCS Data Retrieval

A simple Go service that retrieves JSON data from Google Cloud Storage for apparatus, experiments, and visualizations.

## API Endpoints

- `GET /api/apparatus?name={name}` - Retrieve apparatus data by name
- `GET /api/experiment?name={name}` - Retrieve experiment data by name  
- `GET /api/visualization?name={name}` - Retrieve visualization data by name
- `GET /health` - Health check endpoint

## Environment Variables

- `GCS_BUCKET_NAME` - Name of your Google Cloud Storage bucket (required)
- `PORT` - Port to run the service on (defaults to 8080)

## GCS Bucket Structure

Your bucket should have the following structure:
```
your-bucket/
├── apparatus/
│   ├── {name1}.json
│   └── {name2}.json
├── experiments/
│   ├── {name1}.json
│   └── {name2}.json
└── visualizations/
    ├── {name1}.json
    └── {name2}.json
```

## Local Development

1. Set up your GCS bucket and upload your JSON files
2. Set the environment variable:
   ```bash
   export GCS_BUCKET_NAME=your-bucket-name
   ```
3. Run the service:
   ```bash
   go run main.go
   ```

## Cloud Run Deployment

1. Build and deploy using gcloud:
   ```bash
   gcloud run deploy cloudrun-service \
     --source . \
     --platform managed \
     --region us-central1 \
     --allow-unauthenticated \
     --set-env-vars GCS_BUCKET_NAME=your-bucket-name
   ```

2. The service will automatically have the necessary permissions to access GCS when deployed to Cloud Run.

## Example Usage

```bash
# Get apparatus data
curl "https://your-service-url/api/apparatus?name=multimeter"

# Get experiment data  
curl "https://your-service-url/api/experiment?name=ohms-law"

# Get visualization data
curl "https://your-service-url/api/visualization?name=voltage-graph"
``` 