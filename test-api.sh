#!/bin/bash

# Car API Test Script
# Creates 7 car records, then queries with filters

BASE_URL="${CAR_API_URL:-http://localhost:8080}"

echo "🚗 Car API Test Script"
echo "======================"
echo "Base URL: $BASE_URL"
echo ""

# Car 1
echo "Creating Car 1: Toyota Camry..."
curl -s -X POST "$BASE_URL/api/cars" \
  -F "make=Toyota" \
  -F "model=Camry" \
  -F "year=2023" \
  -F "price=28000" \
  -F "color=White" | jq .

# Car 2
echo "Creating Car 2: Honda Civic..."
curl -s -X POST "$BASE_URL/api/cars" \
  -F "make=Honda" \
  -F "model=Civic" \
  -F "year=2022" \
  -F "price=25000" \
  -F "color=Blue" | jq .

# Car 3
echo "Creating Car 3: Toyota Corolla..."
curl -s -X POST "$BASE_URL/api/cars" \
  -F "make=Toyota" \
  -F "model=Corolla" \
  -F "year=2021" \
  -F "price=22000" \
  -F "color=Black" | jq .

# Car 4
echo "Creating Car 4: Ford Mustang..."
curl -s -X POST "$BASE_URL/api/cars" \
  -F "make=Ford" \
  -F "model=Mustang" \
  -F "year=2024" \
  -F "price=55000" \
  -F "color=Red" | jq .

# Car 5
echo "Creating Car 5: BMW X5..."
curl -s -X POST "$BASE_URL/api/cars" \
  -F "make=BMW" \
  -F "model=X5" \
  -F "year=2023" \
  -F "price=65000" \
  -F "color=Silver" | jq .

# Car 6
echo "Creating Car 6: Honda Accord..."
curl -s -X POST "$BASE_URL/api/cars" \
  -F "make=Honda" \
  -F "model=Accord" \
  -F "year=2023" \
  -F "price=30000" \
  -F "color=Gray" | jq .

# Car 7
echo "Creating Car 7: Toyota RAV4..."
curl -s -X POST "$BASE_URL/api/cars" \
  -F "make=Toyota" \
  -F "model=RAV4" \
  -F "year=2022" \
  -F "price=32000" \
  -F "color=Green" | jq .

echo ""
echo "✅ All 7 cars created!"
echo ""

# Query 1: All cars
echo "📋 Query 1: Get all cars"
curl -s "$BASE_URL/api/cars" | jq .

echo ""

# Query 2: Filter by make=Toyota
echo "📋 Query 2: Get all Toyotas (make=Toyota)"
curl -s "$BASE_URL/api/cars?make=Toyota" | jq .

echo ""

# Query 3: Filter by maxPrice=30000
echo "📋 Query 3: Get cars under $30k (maxPrice=30000)"
curl -s "$BASE_URL/api/cars?maxPrice=30000" | jq .

echo ""

# Query 4: Multiple filters
echo "📋 Query 4: Toyotas under $30k (make=Toyota&maxPrice=30000)"
curl -s "$BASE_URL/api/cars?make=Toyota&maxPrice=30000" | jq .

echo ""
echo "🎉 Test complete!"
