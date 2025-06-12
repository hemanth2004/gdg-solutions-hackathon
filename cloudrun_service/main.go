package main

import (
	"context"
	"fmt"
	"log"
	"net/http"
	"os"

	"cloud.google.com/go/storage"
	"github.com/gorilla/mux"
)

type Server struct {
	client     *storage.Client
	bucketName string
}

func NewServer() (*Server, error) {
	log.Println("Initializing server...")

	bucketName := os.Getenv("GCS_BUCKET_NAME")
	log.Printf("GCS_BUCKET_NAME environment variable: %s", bucketName)

	if bucketName == "" {
		return nil, fmt.Errorf("GCS_BUCKET_NAME is not set")
	}

	ctx := context.Background()
	log.Println("Creating storage client...")

	client, err := storage.NewClient(ctx)
	if err != nil {
		log.Printf("Failed to create storage client: %v", err)
		return nil, fmt.Errorf("failed to create storage client: %v", err)
	}

	log.Println("Storage client created successfully")

	return &Server{
		client:     client,
		bucketName: bucketName,
	}, nil
}

func main() {
	log.Println("Starting application...")

	// First, let's start the HTTP server even if storage fails
	// This will help with debugging
	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	log.Printf("PORT environment variable: %s", port)

	server, err := NewServer()
	if err != nil {
		log.Printf("Warning: Failed to create server with storage: %v", err)
		log.Println("Starting server without storage client for debugging...")

		// Create a minimal server for debugging
		r := mux.NewRouter()
		r.HandleFunc("/health", func(w http.ResponseWriter, r *http.Request) {
			w.Header().Set("Content-Type", "application/json")
			w.WriteHeader(http.StatusOK)
			fmt.Fprintf(w, `{"status": "healthy", "error": "storage not available: %s"}`, err.Error())
		}).Methods("GET")

		r.HandleFunc("/debug", func(w http.ResponseWriter, r *http.Request) {
			w.Header().Set("Content-Type", "application/json")
			w.WriteHeader(http.StatusOK)
			fmt.Fprintf(w, `{"port": "%s", "bucket": "%s", "error": "%s"}`,
				port, os.Getenv("GCS_BUCKET_NAME"), err.Error())
		}).Methods("GET")

		log.Printf("Server starting on port %s (debug mode)", port)
		if err := http.ListenAndServe(":"+port, r); err != nil {
			log.Fatalf("Server failed to start: %v", err)
		}
		return
	}

	defer server.client.Close()

	r := mux.NewRouter()

	// API endpoints
	r.HandleFunc("/api/apparatus", server.handleApparatus).Methods("GET")
	r.HandleFunc("/api/experiment", server.handleExperiment).Methods("GET")
	r.HandleFunc("/api/visualization", server.handleVisualization).Methods("GET")

	// Health check endpoint
	r.HandleFunc("/health", server.healthCheck).Methods("GET")

	// Debug endpoint
	r.HandleFunc("/debug", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		w.WriteHeader(http.StatusOK)
		fmt.Fprintf(w, `{"status": "ok", "port": "%s", "bucket": "%s"}`,
			port, server.bucketName)
	}).Methods("GET")

	log.Printf("Server starting on port %s", port)
	log.Printf("Using GCS bucket: %s", server.bucketName)

	if err := http.ListenAndServe(":"+port, r); err != nil {
		log.Fatalf("Server failed to start: %v", err)
	}
}
