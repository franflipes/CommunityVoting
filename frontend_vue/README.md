# CommunityVoting Vue Frontend

Vue 3 and Vuetify 3 implementation of the CommunityVoting web application. It preserves the existing API contracts and Spanish product interface while using a centralized service layer, reactive authentication store, lazy routes, global notifications, queued confirmations, and responsive Vuetify components.

## Requirements

- Node.js 22 or newer
- The CommunityVoting APIs running on their configured URLs

## Local setup

```bash
cp .env.example .env
npm install
npm run dev
```

The default development URL is `http://localhost:5173`.

## Configuration

Vite reads these values from `.env` at build time:

- `VITE_API_BASE_URL` — identity, community, and meeting API
- `VITE_VOTING_API_BASE_URL` — voting API
- `VITE_DOCUMENT_API_BASE_URL` — document API
- `VITE_VOTING_HUB_URL` — SignalR voting hub

Authentication uses the existing `cv_token` local storage key.

## Quality checks

```bash
npm run lint
npm run build
```

## Container image

Build-time API URLs can be passed as Docker build arguments:

```bash
docker build \
  --build-arg VITE_API_BASE_URL=http://localhost:5004/api \
  --build-arg VITE_VOTING_API_BASE_URL=http://localhost:5222/api/voting \
  --build-arg VITE_DOCUMENT_API_BASE_URL=http://localhost:5088/api \
  --build-arg VITE_VOTING_HUB_URL=http://localhost:5222/hubs/voting \
  -t community-voting-vue .
```

The resulting nginx image listens on port 80 and supports Vue Router history fallback.
