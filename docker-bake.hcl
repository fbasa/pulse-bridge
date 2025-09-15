group "default" { 
    targets = ["idp","api","worker","scheduler","web","payapi","acctapi"] 
}
target "web" { 
    context = "PulseBridge.Web"
    dockerfile = "Dockerfile"
    tags = ["pulse-bridge-web:1.0"] 
}
target "idp" { 
    context = "."
    dockerfile = "PulseBridge.OpenIddict.Idp/Dockerfile"
    tags = ["pulse-bridge-idp:1.0"] 
}
target "api" { 
    context = "."
    dockerfile = "PulseBridge.Api/Dockerfile"
    tags = ["pulse-bridge-api:1.0"] 
}
target "payapi" { 
    context = "."
    dockerfile = "PulseBridge.Payment.Api/Dockerfile"
    tags = ["pulse-bridge-payapi:1.0"] 
}
target "acctapi" { 
    context = "."
    dockerfile = "PulseBridge.Accounting.Api/Dockerfile"
    tags = ["pulse-bridge-acctapi:1.0"] 
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
