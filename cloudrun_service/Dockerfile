# Use official Go base image
FROM golang:1.21

WORKDIR /app

COPY . .
RUN go mod tidy
RUN go build -o main .

# Expose default port for cloud run = 8080
ENV PORT=8080
EXPOSE 8080

CMD ["./main"] 