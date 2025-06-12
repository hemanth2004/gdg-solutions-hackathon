# Use official Go image
FROM golang:1.21

WORKDIR /app

# Copy files and build
COPY . .
RUN go mod tidy
RUN go build -o main .

# Set port for Cloud Run
ENV PORT=8080
EXPOSE 8080

# Run the app
CMD ["./main"] 