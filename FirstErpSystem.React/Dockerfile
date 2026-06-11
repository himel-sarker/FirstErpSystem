# ================================================================
# Dockerfile — Added By Himel Sarkar 10-06-2026
# LEARNING: Multi-stage build for React
# Stage 1 = Node.js দিয়ে npm build করা (dist folder তৈরি)
# Stage 2 = Nginx দিয়ে dist folder serve করা
# ================================================================

# Stage 1: Build React App
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . ./
RUN npm run build

# Stage 2: Serve with Nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx/default.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
