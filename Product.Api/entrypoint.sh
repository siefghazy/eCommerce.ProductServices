#!/bin/sh
set -e

# Fix ownership of named volume mount points at runtime.
# Docker named volumes mount as root:root, which overrides image-layer permissions.
# We run as root here, fix ownership, then drop to the app user (UID 1654) via gosu.
chown -R 1654:1654 /app/wwwroot/ProductImages /home/app/.aspnet/DataProtection-Keys 2>/dev/null || true

# Drop privileges to the app user and exec the .NET process.
# Using gosu so signals (SIGTERM etc.) are forwarded correctly to dotnet.
exec gosu 1654 dotnet Product.Api.dll "$@"
