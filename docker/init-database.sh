#!/bin/sh
set -eu

SQLCMD="/opt/mssql-tools/bin/sqlcmd"
SERVER="premiervenue-db"
DATABASE="PremierVenueDb"

"$SQLCMD" -S "$SERVER" -U sa -P "$MSSQL_SA_PASSWORD" -C -b \
  -Q "IF DB_ID(N'$DATABASE') IS NULL CREATE DATABASE [$DATABASE]"

if "$SQLCMD" -S "$SERVER" -U sa -P "$MSSQL_SA_PASSWORD" -d "$DATABASE" -C -b \
  -Q "IF OBJECT_ID(N'[Users]', N'U') IS NULL OR OBJECT_ID(N'[Venues]', N'U') IS NULL OR OBJECT_ID(N'[Bookings]', N'U') IS NULL OR OBJECT_ID(N'[VenueEventTypes]', N'U') IS NULL OR OBJECT_ID(N'[SavedVenues]', N'U') IS NULL OR COL_LENGTH(N'[Venues]', N'SupportedServices') IS NULL OR COL_LENGTH(N'[Bookings]', N'CancellationFeeDueAt') IS NULL OR COL_LENGTH(N'[VenuePhotos]', N'Content') IS NULL THROW 50000, 'Schema is not initialized', 1"; then
  exit 0
fi

if "$SQLCMD" -S "$SERVER" -U sa -P "$MSSQL_SA_PASSWORD" -d "$DATABASE" -C -b \
  -Q "IF OBJECT_ID(N'[Users]', N'U') IS NOT NULL THROW 50001, 'Existing database schema is incomplete; reset the database or upgrade it before starting', 1"; then
  :
else
  exit 1
fi

"$SQLCMD" -S "$SERVER" -U sa -P "$MSSQL_SA_PASSWORD" -d "$DATABASE" -C -b \
  -i /schema/initial.sql
