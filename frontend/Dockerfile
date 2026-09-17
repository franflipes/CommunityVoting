# Serve a pre-built Vite bundle — no npm/network during image build.
# Build on the host first:  npm run build   (or pnpm build)
# Then:  podman build -f Dockerfile -t communityvoting-frontend:local .
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY dist/ /usr/share/nginx/html/
EXPOSE 80
