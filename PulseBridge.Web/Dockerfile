# ---- Build stage -------------------------------------------------------------
FROM node:20-alpine AS build
WORKDIR /app

# Install deps first (better layer caching)
COPY package*.json ./
# If you use pnpm/yarn, swap the install command accordingly
RUN npm ci

# Copy the rest and build
COPY . .
# Build args let you tweak build config at image build time
ARG CONFIG=production
ARG BASE_HREF=/
# Force a stable output folder so we don't have to guess Angular's dist path
RUN npm run build -- --configuration=$CONFIG --base-href=$BASE_HREF --output-path=build

# ---- Runtime stage (NGINX) --------------------------------------------------
FROM nginx:1.27-alpine AS runtime
# Replace default site with an SPA-friendly config (below)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Static site goes here
COPY --from=build /app/build/ /usr/share/nginx/html/

EXPOSE 80
# Simple healthcheck (ok if index.html is readable)
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD wget -qO- http://127.0.0.1/ > /dev/null 2>&1 || exit 1

# NGINX is the default entrypoint/cmd in this image
