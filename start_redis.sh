#!/bin/bash
podman run -it --rm -p 6379:6379 'docker.io/redis:6-alpine'
