FROM node:20-alpine

WORKDIR /app

ENV NODE_OPTIONS="--max-old-space-size=4096"

RUN npm config set fetch-retry-maxtimeout 600000 \
 && npm config set fetch-timeout 600000 \
 && npm config set registry https://registry.npmmirror.com

COPY package*.json ./

RUN npm install --prefer-offline --no-audit

COPY . .

EXPOSE 4200

CMD ["npm", "run", "docker"]