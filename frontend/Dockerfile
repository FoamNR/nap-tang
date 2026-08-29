# Build stage
FROM node:20-alpine AS builder
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# Production stage
FROM node:20-alpine AS runner
WORKDIR /app

COPY --from=builder /app/.output ./.output
COPY --from=builder /app/package.json ./package.json

ENV PORT=3000
ENV HOST=0.0.0.0
EXPOSE 3000

CMD ["node", ".output/server/index.mjs"]
