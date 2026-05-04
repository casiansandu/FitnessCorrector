# Google Cloud App Engine Deployment Guide

This guide will help you deploy the Fitness Corrector backend to Google Cloud Platform.

## Prerequisites

1. **Google Cloud Account**: Create an account at [cloud.google.com](https://cloud.google.com)
2. **Google Cloud CLI**: Install from [cloud.google.com/sdk/docs/install](https://cloud.google.com/sdk/docs/install)
3. **Project Setup**: Create a new Google Cloud project

## Step 1: Initial Google Cloud Setup

### 1.1 Install and Initialize gcloud CLI

```bash
# Install gcloud CLI (follow the link above for your OS)

# Initialize gcloud
gcloud init

# Login to your Google account
gcloud auth login

# Set your project (replace YOUR_PROJECT_ID with your actual project ID)
gcloud config set project YOUR_PROJECT_ID

# Enable required APIs
gcloud services enable sqladmin.googleapis.com
gcloud services enable storage-api.googleapis.com
gcloud services enable secretmanager.googleapis.com
gcloud services enable appengine.googleapis.com
```

### 1.2 Create App Engine Application

```bash
# Create App Engine app (choose a region close to your users)
gcloud app create --region=europe-west3
```

## Step 2: Set Up Cloud SQL (PostgreSQL)

### 2.1 Create Cloud SQL Instance

```bash
# Create PostgreSQL instance (adjust tier and region as needed)
gcloud sql instances create fitness-db-instance \
    --database-version=POSTGRES_16 \
    --tier=db-f1-micro \
    --region=europe-west3

# Set root password
gcloud sql users set-password postgres \
    --instance=fitness-db-instance \
    --password=YOUR_STRONG_PASSWORD
```

### 2.2 Create Database

```bash
# Create the database
gcloud sql databases create FitnessDb --instance=fitness-db-instance
```

### 2.3 Get Connection Name

```bash
# Get the instance connection name (format: PROJECT_ID:REGION:INSTANCE_NAME)
gcloud sql instances describe fitness-db-instance --format="value(connectionName)"
```

**Save this connection name!** You'll need it for Secret Manager.

## Step 3: Set Up Cloud Storage

### 3.1 Create Storage Bucket

```bash
# Create bucket (bucket name must be globally unique)
gsutil mb -l europe-west3 gs://YOUR_PROJECT_ID-fitness-uploads

# Make bucket private (recommended for user uploads)
gsutil iam ch allUsers:objectViewer gs://YOUR_PROJECT_ID-fitness-uploads
```

## Step 4: Configure Secret Manager

### 4.1 Create Secrets

```bash
# Connection string for Cloud SQL
echo -n "Host=/cloudsql/YOUR_CONNECTION_NAME;Database=FitnessDb;Username=postgres;Password=YOUR_DB_PASSWORD" | \
    gcloud secrets create DB_CONNECTION_STRING --data-file=-

# JWT Secret
echo -n "YourSuperSecretKeyThatIsAtLeast32CharactersLong!Change_This_In_Production" | \
    gcloud secrets create JWT_SECRET --data-file=-

# Stripe Secret Key
echo -n "sk_test_YOUR_STRIPE_SECRET_KEY" | \
    gcloud secrets create STRIPE_SECRET_KEY --data-file=-

# Stripe Webhook Secret
echo -n "whsec_YOUR_STRIPE_WEBHOOK_SECRET" | \
    gcloud secrets create STRIPE_WEBHOOK_SECRET --data-file=-

# Admin credentials
echo -n "admin@fitness.com" | \
    gcloud secrets create ADMIN_EMAIL --data-file=-

echo -n "Admin123!" | \
    gcloud secrets create ADMIN_PASSWORD --data-file=-

# Frontend URL (your deployed frontend URL or localhost for testing)
echo -n "https://your-frontend-url.com" | \
    gcloud secrets create FRONTEND_URL --data-file=-

# Google Cloud Project ID
echo -n "YOUR_PROJECT_ID" | \
    gcloud secrets create GCP_PROJECT_ID --data-file=-

# Storage Bucket Name
echo -n "YOUR_PROJECT_ID-fitness-uploads" | \
    gcloud secrets create STORAGE_BUCKET --data-file=-
```

### 4.2 Grant App Engine Access to Secrets

```bash
# Get the App Engine service account
export PROJECT_ID=$(gcloud config get-value project)
export APP_ENGINE_SA="${PROJECT_ID}@appspot.gserviceaccount.com"

# Grant access to each secret
gcloud secrets add-iam-policy-binding DB_CONNECTION_STRING \
    --member="serviceAccount:${APP_ENGINE_SA}" \
    --role="roles/secretmanager.secretAccessor"

gcloud secrets add-iam-policy-binding JWT_SECRET \
    --member="serviceAccount:${APP_ENGINE_SA}" \
    --role="roles/secretmanager.secretAccessor"

gcloud secrets add-iam-policy-binding STRIPE_SECRET_KEY \
    --member="serviceAccount:${APP_ENGINE_SA}" \
    --role="roles/secretmanager.secretAccessor"

gcloud secrets add-iam-policy-binding STRIPE_WEBHOOK_SECRET \
    --member="serviceAccount:${APP_ENGINE_SA}" \
    --role="roles/secretmanager.secretAccessor"

gcloud secrets add-iam-policy-binding ADMIN_EMAIL \
    --member="serviceAccount:${APP_ENGINE_SA}" \
    --role="roles/secretmanager.secretAccessor"

gcloud secrets add-iam-policy-binding ADMIN_PASSWORD \
    --member="serviceAccount:${APP_ENGINE_SA}" \
    --role="roles/secretmanager.secretAccessor"

gcloud secrets add-iam-policy-binding FRONTEND_URL \
    --member="serviceAccount:${APP_ENGINE_SA}" \
    --role="roles/secretmanager.secretAccessor"

gcloud secrets add-iam-policy-binding GCP_PROJECT_ID \
    --member="serviceAccount:${APP_ENGINE_SA}" \
    --role="roles/secretmanager.secretAccessor"

gcloud secrets add-iam-policy-binding STORAGE_BUCKET \
    --member="serviceAccount:${APP_ENGINE_SA}" \
    --role="roles/secretmanager.secretAccessor"
```

## Step 5: Update app.yaml

Update your `app.yaml` file to reference the secrets:

```yaml
runtime: custom
env: flex

resources:
  cpu: 1
  memory_gb: 2
  disk_size_gb: 10

automatic_scaling:
  min_num_instances: 1
  max_num_instances: 2
  cpu_utilization:
    target_utilization: 0.6

env_variables:
  ASPNETCORE_ENVIRONMENT: "Production"
  CLOUD_SQL_CONNECTION_NAME: "YOUR_PROJECT_ID:europe-west3:fitness-db-instance"

  # Secret Manager references
  DB_CONNECTION_STRING: ${DB_CONNECTION_STRING}
  JWT_SECRET: ${JWT_SECRET}
  STRIPE_SECRET_KEY: ${STRIPE_SECRET_KEY}
  STRIPE_WEBHOOK_SECRET: ${STRIPE_WEBHOOK_SECRET}
  ADMIN_EMAIL: ${ADMIN_EMAIL}
  ADMIN_PASSWORD: ${ADMIN_PASSWORD}
  FRONTEND_URL: ${FRONTEND_URL}
  GCP_PROJECT_ID: ${GCP_PROJECT_ID}
  STORAGE_BUCKET: ${STORAGE_BUCKET}

beta_settings:
  cloud_sql_instances: "YOUR_PROJECT_ID:europe-west3:fitness-db-instance"

liveness_check:
  path: "/liveness"
  check_interval_sec: 30
  timeout_sec: 4
  failure_threshold: 2
  success_threshold: 2

readiness_check:
  path: "/readiness"
  check_interval_sec: 5
  timeout_sec: 4
  failure_threshold: 2
  success_threshold: 2
  app_start_timeout_sec: 300
```

## Step 6: Run Database Migrations

Before deploying, you need to run EF Core migrations against your Cloud SQL database.

### Option A: Run from local machine

```bash
# Install Cloud SQL Proxy
curl -o cloud-sql-proxy https://storage.googleapis.com/cloud-sql-connectors/cloud-sql-proxy/v2.13.0/cloud-sql-proxy.linux.amd64
chmod +x cloud-sql-proxy

# Start Cloud SQL Proxy
./cloud-sql-proxy YOUR_PROJECT_ID:europe-west3:fitness-db-instance

# In another terminal, run migrations
cd FitnessCorrector.Infrastructure
dotnet ef database update --startup-project ../FitnessCorrector.WebAPI
```

### Option B: Run from Cloud Shell

```bash
# Open Cloud Shell in Google Cloud Console
# Clone your repository
git clone YOUR_REPOSITORY_URL
cd ServerFitnessCorrector

# Run migrations
dotnet ef database update --startup-project FitnessCorrector.WebAPI --connection "Host=/cloudsql/YOUR_CONNECTION_NAME;Database=FitnessDb;Username=postgres;Password=YOUR_PASSWORD"
```

## Step 7: Deploy to App Engine

```bash
# Navigate to the project root (where Dockerfile and app.yaml are)
cd /path/to/ServerFitnessCorrector

# Deploy to App Engine
gcloud app deploy

# View logs
gcloud app logs tail -s default

# Open your app in browser
gcloud app browse
```

## Step 8: Configure Stripe Webhook

After deployment, you need to update your Stripe webhook URL:

1. Go to [Stripe Dashboard](https://dashboard.stripe.com)
2. Navigate to **Developers** → **Webhooks**
3. Click on your webhook endpoint (or create new one)
4. Set the endpoint URL to: `https://YOUR_PROJECT_ID.appspot.com/api/stripe/webhook`
5. Select events: `checkout.session.completed`, `customer.subscription.deleted`, `customer.subscription.updated`
6. Copy the **Signing secret** and update it in Secret Manager:

```bash
echo -n "whsec_YOUR_NEW_WEBHOOK_SECRET" | \
    gcloud secrets versions add STRIPE_WEBHOOK_SECRET --data-file=-
```

7. Redeploy the app: `gcloud app deploy`

## Step 9: Update Frontend Configuration

Update your frontend to point to the deployed backend:

- API Base URL: `https://YOUR_PROJECT_ID.appspot.com`
- Stripe Frontend URL: Update in Secret Manager if needed

## Monitoring and Maintenance

### View Logs

```bash
# Real-time logs
gcloud app logs tail -s default

# Logs from last hour
gcloud app logs read --limit 50
```

### View App Engine Dashboard

```bash
# Open in browser
gcloud app browse

# Or visit: https://console.cloud.google.com/appengine
```

### Update Secrets

```bash
# Update a secret
echo -n "NEW_VALUE" | gcloud secrets versions add SECRET_NAME --data-file=-

# Redeploy to pick up new values
gcloud app deploy
```

### Scale Your App

Edit `app.yaml` and modify:
- `min_num_instances` / `max_num_instances`
- `cpu` / `memory_gb`
- `target_utilization`

Then redeploy: `gcloud app deploy`

## Troubleshooting

### Check Build Logs

```bash
gcloud app logs tail -s default
```

### Connect to Cloud SQL

```bash
gcloud sql connect fitness-db-instance --user=postgres
```

### Test Health Endpoints

```bash
curl https://YOUR_PROJECT_ID.appspot.com/liveness
curl https://YOUR_PROJECT_ID.appspot.com/readiness
```

### Common Issues

1. **Database connection fails**: Check Cloud SQL instance is running and connection name is correct
2. **Secrets not accessible**: Verify IAM permissions for App Engine service account
3. **Build fails**: Check Dockerfile and ensure all projects compile locally first
4. **App crashes**: Check logs with `gcloud app logs tail`

## Cost Optimization

### Free Tier (for testing)

- Use `db-f1-micro` for Cloud SQL
- Set `min_num_instances: 0` (app sleeps when idle)
- Use `europe-west3` or `us-central1` (cheaper regions)

### Production

- Upgrade Cloud SQL tier based on load
- Set appropriate scaling limits
- Monitor costs in Google Cloud Console

## Python AI Service (Future Enhancement)

Currently, the Python AI service runs in the same container. For better scalability:

1. Extract Python code to separate Cloud Run service
2. Call it via HTTP from the .NET backend
3. This allows independent scaling of AI workload

## Security Checklist

- [ ] All secrets are in Secret Manager (not in code or app.yaml)
- [ ] Cloud SQL has strong password
- [ ] Storage bucket has proper IAM permissions
- [ ] CORS is configured for your frontend domain only
- [ ] JWT secret is long and random
- [ ] Stripe webhook signature is verified
- [ ] HTTPS is enforced (default in App Engine)

## Next Steps

1. Set up CI/CD with Cloud Build
2. Configure custom domain
3. Set up monitoring and alerts
4. Implement backup strategy for Cloud SQL
5. Set up Cloud CDN for static assets

## Support

For issues, check:
- [Google Cloud Documentation](https://cloud.google.com/docs)
- [App Engine Flexible Docs](https://cloud.google.com/appengine/docs/flexible)
- [Cloud SQL Docs](https://cloud.google.com/sql/docs)
