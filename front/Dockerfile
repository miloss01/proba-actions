# Build Angular app
FROM node:20 as build

WORKDIR /app
COPY . .
RUN npm install
RUN npm run build --prod

# Serve with NGINX
FROM nginx:alpine

# Kopiraj build output (pazi da je tačan outputPath iz angular.json)
COPY --from=build /app/dist/front/browser /usr/share/nginx/html

# Kopiraj nginx template config
COPY nginx.template.conf /etc/nginx/templates/default.conf.template

# Default vrednost ako nije prosleđeno u compose
ENV APIHOST=http://localhost:5156/api/

# Pokreni NGINX nakon što zameniš varijablu u template configu
CMD ["/bin/sh", "-c", "envsubst '$$APIHOST' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]

EXPOSE 80
