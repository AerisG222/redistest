#!/bin/bash
podman run -it --rm 'docker.io/redis:6-alpine' redis-cli -h 192.168.1.234
