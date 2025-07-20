#!/bin/sh

echo "Zamenjujem vrednost apiHost u main*.js fajlu..."

for file in /usr/share/nginx/html/main*.js; do
  sed -i "s|apiHost:\"[^\"]*\"|apiHost:\"${APIHOST}\"|g" "$file"
done

echo "Pokrećem NGINX..."
nginx -g "daemon off;"
