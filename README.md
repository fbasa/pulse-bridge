# pulse-bridge
Event‑Driven Architecture (Quartz + MassTransit + RabbitMQ + SignalR + Redis)  
Bridges scheduled "pulses" to API to client UIs in real time.

PulseBridge.Web = https://ui.localtest.me/  
PulseBridge.Api = https://api.localtest.me/  
OpenTelemetry = https://otel.localtest.me/search  

https://api.localtest.me/api/jobs  
https://api.localtest.me/api/jobs/insert  

https://github.com/fbasa/pulse-bridge/blob/main/diagram.png

## build all images (docker-bake.hcl)
```docker buildx bake```  
This will create all images (web, api, scheduler & worker)  


## docker compose is to run images built
When **docker-compose.yml** already in place and configured, you can run multiple containers (execute command in solution root directory)  
run all services define in docker-compose.yml file  
```docker compose up -d```  


## build individual image
```docker build -t pulse-bridge-web:1.0 -f PulseBridge.Web/Dockerfile .```  
```docker build -t pulse-bridge-api:1.0 -f PulseBridge.Api/Dockerfile .```  
```docker build -t pulse-bridge-scheduler:1.0 -f PulseBridge.Scheduler/Dockerfile .```  
```docker build -t pulse-bridge-worker:1.0 -f PulseBridge.Worker/Dockerfile .```  

## create database if it doesn't exist
```docker exec sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Strong_Passw0rd!" -C -Q "IF DB_ID('QuartzNet') IS NULL CREATE DATABASE QuartzNet;"```

## run database update inside container
```docker run --rm --network myapp_default -v ${PWD}:/src -w /src mcr.microsoft.com/dotnet/sdk:9.0 dotnet ef database update```

## run container mapping host port 8080 -> container 8080
```docker run -d --name pulse-bridge-api -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production pulse-bridge-api:1.0```

## test from a workstation on the same network:
```http://localhost:8080/```

## .env file 
SA_PASSWORD=<Strong_Password>  
DB_CONN=Server=sql;Database=QuartzNet;User ID=sa;Password=<Strong_Password>;TrustServerCertificate=true;  
REDIS_CONN=redis:6379  

(keep ```.env``` file out of source control)  


## operational tips for on-prem

* **Reverse proxy & TLS**: Put **Nginx/Traefik** in front for HTTPS, compression, HTTP/2, and blue-green switchovers.
* **Secrets**: Prefer a secrets manager (HashiCorp Vault, Kubernetes Secrets, or Swarm secrets). For plain Compose, use environment files and lock down file permissions.
* **Non-root**: As shown, run your app as a non-root user inside the container.
* **Health checks**: Keep `/health` endpoint and use Docker/K8s health checks for self-healing.
* **Persistence**: DBs and message brokers should use **volumes** or live on dedicated servers/services.
* **Logging/metrics**: Emit structured logs to STDOUT; ship them via the Docker logging driver or an agent (e.g., Fluent Bit) to your ELK/Seq/Grafana stack. Add OpenTelemetry for traces/metrics.
* **Image hygiene**: Pin base images by major/minor (e.g., `aspnet:9.0`), rebuild regularly for security patches, and scan images (Trivy/Grype).
* **Scaling choices**:

  * **Single box**: Compose is fine for small/medium workloads.
  * **Multiple boxes/HA**: Move to **Kubernetes** (k3s is a lightweight on-prem favorite).


## Unable to login 'sa'
Option C — Reset by recreating the DB volume (destructive)

This wipes all SQL data; only do in dev.  

Stop and remove containers:  
```docker compose down```  

Remove the SQL data volume:  
```docker volume rm pulse-bridge_mssqldata```  

Ensure .env has the desired SA_PASSWORD, then recreate:  
```docker compose up -d```  

You’ll get a fresh SQL instance with SA set to the .env value.  




## Trusted local TLS with mkcert - Install & trust the local CA
```mkcert -install```   

## Make a cert for your dev hosts
```mkdir -p traefik/certs```  
```mkcert -key-file traefik/certs/dev.key -cert-file traefik/certs/dev.crt ui.localtest.me api.localtest.me otel.localtest.me```


## If mkcert command not found, install choco first
```Set-ExecutionPolicy Bypass -Scope Process -Force;```  
```[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072;```  
```iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))```  

## Then install  mkcert
```choco install mkcert```  
```mkcert -install```

