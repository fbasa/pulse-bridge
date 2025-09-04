group "default" { 
    targets = ["api","worker","scheduler"] 
}
target "web" { 
    context = "."
    dockerfile = "PulseBridge.Web/Dockerfile"
    tags = ["pulse-bridge-web:1.0"] 
}
target "api" { 
    context = "."
    dockerfile = "PulseBridge.Api/Dockerfile"
    tags = ["pulse-bridge-api:1.0"] 
}
target "worker" { 
    context = "."
    dockerfile = "PulseBridge.Worker/Dockerfile"
    tags = ["pulse-bridge-worker:1.0"] 
}
target "scheduler" { 
    context = "."
    dockerfile = "PulseBridge.Scheduler/Dockerfile"
    tags = ["pulse-bridge-scheduler:1.0"] 
}
