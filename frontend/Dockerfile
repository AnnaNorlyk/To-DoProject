# Stage 1: Build
FROM node:18 AS builder

WORKDIR /app
COPY . .

# Ensure .env.local is included in the image
COPY .env.local .env.local

RUN npm install
RUN npm run build

# Stage 2: Serve
FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html

# Optional: custom Nginx config
# COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
