#!/bin/sh

echo "Zamenjujem APIHOST u main*.js..."

JS_PATH="/usr/share/nginx/html"
APIHOST=${APIHOST:-http://localhost:5156/api/}

# Nađi main*.js fajl i zameni apiHost
for file in $JS_PATH/main*.js; do
  if grep -q 'apiHost:' "$file"; then
    echo "Menjam apiHost u fajlu $file"
    sed -i "s|apiHost:.*|apiHost:\"$APIHOST\"|g" "$file"
  fi
done

echo "Pokrećem NGINX..."
nginx -g "daemon off;"
