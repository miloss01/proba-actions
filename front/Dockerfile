# Build Angular app
FROM node:20 as build

WORKDIR /app
COPY . .
RUN npm install

ARG API_HOST=http://localhost:5156/api1/
RUN sed -i "s|apiHost: '.*'|apiHost: '${API_HOST}'|g" src/app/env/environment.ts

RUN npm run build --prod

# Serve with NGINX
FROM nginx:alpine
COPY --from=build /app/dist/front/browser /usr/share/nginx/html

COPY /nginx.conf  /etc/nginx/conf.d/default.conf

EXPOSE 80
