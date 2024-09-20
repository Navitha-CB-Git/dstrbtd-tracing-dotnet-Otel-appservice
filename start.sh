#!/bin/bash

# Start ServiceA on port 5000
dotnet /app/publish/ServiceA/ServiceA.dll --urls http://localhost:5000 &

# Start ServiceB on port 5001
dotnet /app/publish/ServiceB/ServiceB.dll --urls http://localhost:5001 &

# Start Nginx to route traffic
nginx -g 'daemon off;'

