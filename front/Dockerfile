# Build Angular app
FROM node:20 as build

WORKDIR /app
COPY . .
RUN npm install
RUN npm run build --prod

# Serve with NGINX
FROM nginx:alpine

# Default vrednost ako nije prosleđeno u compose
ENV APIHOST=http://localhost:5156/apiizdockerfile/

# Kopiraj build output (pazi da je tačan outputPath iz angular.json)
COPY --from=build /app/dist/front/browser /usr/share/nginx/html

# Zameni apiHost string u JS fajlu
RUN apk add --no-cache grep sed && \
    JS_FILE=$(find /usr/share/nginx/html -name 'main*.js') && \
    sed -i "s|apiHost:\"http://localhost:5156/api/\"|apiHost:\"${APIHOST}\"|g" "$JS_FILE"

# Kopiraj nginx template config
COPY nginx.template.conf /etc/nginx/templates/default.conf.template

EXPOSE 80
