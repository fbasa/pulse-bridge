# ---------- 1) Build stage ----------
FROM node:20-alpine AS builder
WORKDIR /app

# Install deps using lockfile (faster, reproducible)
COPY package*.json ./
RUN npm ci

# Copy source and build
COPY . .
# Choose build config and (optionally) base href at build time
ARG CONFIG=production
ARG BASE_HREF=/
# Use your package.json "build" script if present; otherwise call ng directly
RUN npm run -s build -- --configuration=$CONFIG --base-href=$BASE_HREF \
 || npx ng build --configuration=$CONFIG --base-href=$BASE_HREF

# ---------- 2) Runtime stage ----------
FROM nginx:1.27-alpine

# Minimal tools for healthcheck
RUN apk add --no-cache curl

# SPA-friendly nginx config (no separate file needed)
RUN rm -f /etc/nginx/conf.d/default.conf && \
    printf 'server {\n\
  listen 80;\n\
  server_name _;\n\
  root /usr/share/nginx/html;\n\
  index index.html;\n\
  location / { try_files $uri $uri/ /index.html; }\n\
  location ~* \\.(js|css|png|jpg|jpeg|gif|ico|svg|woff2?)$ {\n\
    expires 7d;\n\
    add_header Cache-Control "public, max-age=604800, immutable" always;\n\
  }\n\
}\n' > /etc/nginx/conf.d/angular.conf

# Copy built files from the builder.
# Angular 15–18+ may output either: dist/<app>/ or dist/<app>/browser
COPY --from=builder /app/dist /tmp/dist

# Flatten dist structure so /usr/share/nginx/html has index.html at its root
RUN set -eux; \
    if distdir="$(find /tmp/dist -type d -name browser -print -quit)"; then \
      cp -r "$distdir"/* /usr/share/nginx/html/; \
    elif appdir="$(find /tmp/dist -maxdepth 1 -mindepth 1 -type d -print -quit)"; then \
      cp -r "$appdir"/* /usr/share/nginx/html/; \
    else \
      cp -r /tmp/dist/* /usr/share/nginx/html/; \
    fi; \
    rm -rf /tmp/dist

# EXPOSE 80
# HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
#   CMD curl -fsS http://localhost/ >/dev/null || exit 1
