FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
ARG VITE_API_BASE_URL=http://localhost:5004/api
ARG VITE_VOTING_API_BASE_URL=http://localhost:5222/api/voting
ARG VITE_DOCUMENT_API_BASE_URL=http://localhost:5088/api
ARG VITE_VOTING_HUB_URL=http://localhost:5222/hubs/voting
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_VOTING_API_BASE_URL=$VITE_VOTING_API_BASE_URL
ENV VITE_DOCUMENT_API_BASE_URL=$VITE_DOCUMENT_API_BASE_URL
ENV VITE_VOTING_HUB_URL=$VITE_VOTING_HUB_URL
RUN npm run build

FROM nginx:1.29-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
