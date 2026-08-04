## To force the build of the containers, run the following command:
docker compose -f mcp-docker-compose-scalable.yml up -d --build --scale knowledge-mcp=3

## Otherwise, to start the containers without rebuilding, run:
docker compose -f mcp-docker-compose-scalable.yml up -d --scale knowledge-mcp=3

## To check the status of the containers, run:
docker compose -f mcp-docker-compose-scalable.yml ps

## To view the logs of the containers, run:
docker compose -f mcp-docker-compose-scalable.yml logs -f knowledge-mcp