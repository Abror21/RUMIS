FROM nginx:1.22.1-alpine

## Remove default nginx index page
RUN rm -rf /usr/share/nginx/html/*

COPY ["Izm.Rumis.App/nginx/default.conf", "/etc/nginx/conf.d/"]

EXPOSE 4200
