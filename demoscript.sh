#!/bin/bash
echo "Demo Script Started!"
cd dev-challenge-backend
cd dev-challenge-backend
docker-compose up -d
cd ..
cd ..
cd AscatuTask
docker-compose up -d
start tail_logs.sh
sleep 5
echo "GetPersonList"
echo ""
curl http://localhost:8080/api/v1/person
echo ""
echo "CreatePerson"
echo ""
curl -X POST http://localhost:8080/api/v1/person \
  -H "Content-Type: application/json" \
  -d '{
    "city": "Berlin",
    "country": "Germany",
    "extensionFields": {},
    "firstName": "John",
    "houseNumber": "42",
    "id": "1060f9e4-be45-4516-bb71-5cd97c304254",
    "lastName": "Doe",
    "streetAddress": "Street",
    "zip": "10115"
  }'
echo ""
echo "SearchPerson"
echo ""
curl -X POST http://localhost:8080/api/v1/person/search/ \
  -H "Content-Type: application/json" \
  -d '{
    "city": "Berlin",
    "country": "Germany",
    "extensionFields": {},
    "firstName": "John",
    "houseNumber": "42",
    "lastName": "Doe",
    "streetAddress": "Street",
    "zip": "10115"
  }'
echo ""
echo "GetOrderList"
echo ""
curl http://localhost:8081/api/v1/order/
echo ""
echo "DeleteAllOrders"
echo ""
curl -X DELETE http://localhost:8081/api/v1/order/
echo ""
echo "GetOrderList"
echo ""
curl http://localhost:8081/api/v1/order/
echo ""
echo "Demo Script Ended!"
