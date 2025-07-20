# Build Angular app
FROM node:20 as build

WORKDIR /app
COPY . .
RUN npm install
RUN npm run build --prod

# Serve with NGINX
FROM nginx:alpine

ENV APIHOST=http://localhost:5156/apiizdockerfile/

# Instaliraj potrebne alate
RUN apk add --no-cache grep sed bash

# Kopiraj build output
COPY --from=build /app/dist/front/browser /usr/share/nginx/html

# Kopiraj entrypoint skriptu
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Kopiraj nginx config (ako koristiš proxy)
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

ENTRYPOINT ["/entrypoint.sh"]
