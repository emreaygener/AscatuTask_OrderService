#!/bin/bash
echo "Tailing Docker Logs"
docker-compose logs -f &
wait
