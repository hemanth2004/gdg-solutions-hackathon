package main

import (
	"context"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
	"strings"
)

func (s *Server) getObjectFromGCS(ctx context.Context, objectPath string) ([]byte, error) {
	obj := s.client.Bucket(s.bucketName).Object(objectPath)
	reader, err := obj.NewReader(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to read object %s: %v", objectPath, err)
	}
	defer reader.Close()

	data, err := io.ReadAll(reader)
	if err != nil {
		return nil, fmt.Errorf("failed to read data from object %s: %v", objectPath, err)
	}

	return data, nil
}

func (s *Server) handleApparatus(w http.ResponseWriter, r *http.Request) {
	name := r.URL.Query().Get("name")
	if name == "" {
		http.Error(w, "name parameter is required", http.StatusBadRequest)
		return
	}

	// Construct the object path: apparatus/{name}.json
	objectPath := fmt.Sprintf("apparatus/%s.json", name)

	ctx := r.Context()
	data, err := s.getObjectFromGCS(ctx, objectPath)
	// If error while retrieving object
	if err != nil {
		if strings.Contains(err.Error(), "object doesn't exist") {
			http.Error(w, fmt.Sprintf("apparatus '%s' not found", name), http.StatusNotFound)
			return
		}
		log.Printf("Error retrieving apparatus %s: %v", name, err)
		http.Error(w, "internal server error", http.StatusInternalServerError)
		return
	}

	// Validate JSON structure
	var apparatus Apparatus
	if err := json.Unmarshal(data, &apparatus); err != nil {
		log.Printf("Error unmarshaling apparatus %s: %v", name, err)
		http.Error(w, "invalid apparatus data format", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.Write(data)
}

func (s *Server) handleExperiment(w http.ResponseWriter, r *http.Request) {
	name := r.URL.Query().Get("name")
	if name == "" {
		http.Error(w, "name parameter is required", http.StatusBadRequest)
		return
	}

	// Construct the object path: experiments/{name}.json
	objectPath := fmt.Sprintf("experiments/%s.json", name)

	ctx := r.Context()
	data, err := s.getObjectFromGCS(ctx, objectPath)
	if err != nil {
		if strings.Contains(err.Error(), "object doesn't exist") {
			http.Error(w, fmt.Sprintf("experiment '%s' not found", name), http.StatusNotFound)
			return
		}
		log.Printf("Error retrieving experiment %s: %v", name, err)
		http.Error(w, "internal server error", http.StatusInternalServerError)
		return
	}

	// Validate JSON structure
	var experiment Experiment
	if err := json.Unmarshal(data, &experiment); err != nil {
		log.Printf("Error unmarshaling experiment %s: %v", name, err)
		http.Error(w, "invalid experiment data format", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.Write(data)
}

func (s *Server) handleVisualization(w http.ResponseWriter, r *http.Request) {
	name := r.URL.Query().Get("name")
	if name == "" {
		http.Error(w, "name parameter is required", http.StatusBadRequest)
		return
	}

	// Construct the object path: visualizations/{name}.json
	objectPath := fmt.Sprintf("visualizations/%s.json", name)

	ctx := r.Context()
	data, err := s.getObjectFromGCS(ctx, objectPath)
	if err != nil {
		if strings.Contains(err.Error(), "object doesn't exist") {
			http.Error(w, fmt.Sprintf("visualization '%s' not found", name), http.StatusNotFound)
			return
		}
		log.Printf("Error retrieving visualization %s: %v", name, err)
		http.Error(w, "internal server error", http.StatusInternalServerError)
		return
	}

	// Validate JSON structure
	var visualization Visualization
	if err := json.Unmarshal(data, &visualization); err != nil {
		log.Printf("Error unmarshaling visualization %s: %v", name, err)
		http.Error(w, "invalid visualization data format", http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.Write(data)
}

func (s *Server) healthCheck(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"status": "healthy"})
}
