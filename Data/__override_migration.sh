#!/usr/bin/env bash

# undo last migration
dotnet ef database update 0

# remove all migrations
dotnet ef migrations remove

# create a new migration
dotnet ef migrations add InitialSetup

# update the database
dotnet ef database update
