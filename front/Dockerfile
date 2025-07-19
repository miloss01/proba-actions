# Build Angular app
FROM node:20 as build

WORKDIR /app
COPY . .
RUN npm install
RUN npm run build --prod

# Serve with NGINX
FROM nginx:alpine
COPY --from=build /app/dist/front/browser /usr/share/nginx/html

COPY /nginx.conf  /etc/nginx/conf.d/default.conf

EXPOSE 80

# Replace API_HOST at runtime using envsubst
# COPY nginx.template.conf /etc/nginx/templates/default.conf.template
# ENV APIHOST=http://localhost:3001/api/
# CMD ["/bin/sh", "-c", "envsubst '$$APIHOST' < /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]
