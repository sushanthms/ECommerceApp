# Stage 1: build React
FROM node:22-alpine AS build
WORKDIR /app
COPY ECommerceFrontend/package*.json ./
RUN npm ci
COPY ECommerceFrontend/ ./
RUN npm run build

# Stage 2: nginx serves it
FROM nginx:alpine
COPY Nginx/nginx.conf /etc/nginx/nginx.conf
COPY --from=build /app/dist /usr/share/nginx/html