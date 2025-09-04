# pulse-bridge
Event‑Driven Architecture (Quartz + MassTransit + RabbitMQ + SignalR + Redis)  
Bridges scheduled "pulses" to API to client UIs in real time.

PulseBridge.Web = 8080  
PulseBridge.Api = 8081  
PulseBridge.Scheduler = 8082  
PulseBridge.Worker = 8083


## dockerfile
When **dockerfile** already in place and configured, first step is to build the api image (execute command in solution root directory)

## build image
```docker build -t pulse-bridge-web:1.0 -f PulseBridge.Web/Dockerfile .```  
```docker build -t pulse-bridge-api:1.0 -f PulseBridge.Api/Dockerfile .```  
```docker build -t pulse-bridge-scheduler:1.0 -f PulseBridge.Scheduler/Dockerfile .```  
```docker build -t pulse-bridge-worker:1.0 -f PulseBridge.Worker/Dockerfile .```  

## run container mapping host port 8080 -> container 8080
```docker run -d --name myapi -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production myapi:1.0```

## test from a workstation on the same network:
```http://localhost:8080/```

## .env file 
SA_PASSWORD=<Strong_Password>  
DB_CONN=Server=sql;Database=AppDb;User ID=sa;Password=<Strong_Password>;TrustServerCertificate=true;  
REDIS_CONN=redis:6379  

(keep ```.env``` file out of source control)  


## docker compose
When **docker-compose.yml** already in place and configured, you can run multiple containers (execute command in solution root directory)  
run all services define in docker-compose.yml file  
```docker compose up -d```  
build and run all services define in docker-compose.yml file  
```docker compose up -d --build web api sheduler worker```


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

---


