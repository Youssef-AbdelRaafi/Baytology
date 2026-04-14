"""
Baytology: Real Estate Recommendation API
-----------------------------------------
A high-performance FastAPI service that uses FAISS (Facebook AI Similarity Search)
to provide instant property recommendations based on numerical similarity.

Endpoints:
- GET /: Health check and service status.
- GET /recommend/{id}: Returns N similar properties for a given property ID.
"""

import os
import faiss
import pandas as pd
import numpy as np
from fastapi import FastAPI, HTTPException, Query
from contextlib import asynccontextmanager

# --- Global Resource Storage ---
# Stores the index and dataframes in memory to avoid redundant disk I/O.
resources = {
    "index": None,
    "master_df": None,
    "feature_vectors": None
}

@asynccontextmanager
async def lifespan(app: FastAPI):
    """
    Lifecycle manager: Executes initialization logic on startup and 
    cleanup logic on shutdown.
    """
    # Define paths relative to the project root
    BASE_DIR = os.path.dirname(os.path.abspath(__file__))
    INDEX_PATH = os.path.join(BASE_DIR, 'faiss_index.bin')
    MASTER_PATH = os.path.join(BASE_DIR, 'Datasets', 'master_dataset.csv')
    ENCODED_PATH = os.path.join(BASE_DIR, 'Datasets', 'preprocessed_dataset.csv')

    # Ensure all required artifacts exist before starting
    files = [INDEX_PATH, MASTER_PATH, ENCODED_PATH]
    if not all(os.path.exists(p) for p in files):
        print("❌ CRITICAL ERROR: Startup failed. Missing model or dataset files.")
    else:
        print("🔍 Loading Baytology Recommendation Engine...")
        
        # 1. Load FAISS Index (The Vector Search Engine)
        resources["index"] = faiss.read_index(INDEX_PATH)
        
        # 2. Load Master Metadata (Human-readable data for display)
        resources["master_df"] = pd.read_csv(MASTER_PATH)
        
        # 3. Load Preprocessed Vectors 
        # We need these to retrieve the 'query vector' for a specific property ID.
        df_enc = pd.read_csv(ENCODED_PATH)
        # Drop ID as it is a label, not a searchable feature
        resources["feature_vectors"] = df_enc.drop(columns=['id']).values.astype('float32')
        
        print(f"✅ Engine Online: {len(resources['master_df'])} properties indexed.")
    
    yield
    # Clear memory resources on server shutdown
    resources.clear()

# Initialize FastAPI with metadata for Swagger UI (/docs)
app = FastAPI(
    title="Baytology API",
    description="Vector-based property recommendation engine for the Egyptian real estate market.",
    version="1.0.0",
    lifespan=lifespan
)

# --- Endpoints ---

@app.get("/", tags=["Health"])
def health_check():
    """Returns the current status of the API."""
    return {
        "status": "Online", 
        "engine": "FAISS",
        "region": "Egypt"
    }

@app.get("/recommend/{house_id}", tags=["Recommendation"])
async def get_recommendations(
    house_id: int, 
    n: int = Query(default=5, ge=1, le=20, description="Number of properties")
):
    if resources["index"] is None:
        raise HTTPException(status_code=503, detail="Search engine initializing.")

    master_df = resources["master_df"]
    
    # 1. Map ID to internal index
    try:
        internal_idx = master_df[master_df['id'] == house_id].index[0]
    except IndexError:
        raise HTTPException(status_code=404, detail=f"ID {house_id} not found.")

    # 2. Search (Requesting n+1 to skip the source property)
    query_vector = resources["feature_vectors"][internal_idx].reshape(1, -1)
    distances, indices = resources["index"].search(query_vector, n + 1)

    # 3. Process indices and distances (Skip the first one)
    # We zip them together so each property knows its 'distance' score
    raw_results = []
    for dist, idx in zip(distances[0][1:], indices[0][1:]):
        item = master_df.iloc[idx].to_dict()
        # A distance of 0.0 means a perfect match; higher means less similar.
        item["similarity_score"] = round(float(dist), 4)
        raw_results.append(item)

    # 4. Final Response Structure
    return {
        "metadata": {
            "query_property_id": house_id,
            "total_recommendations_found": len(raw_results),
            "engine": "FAISS-IVFFlat"
        },
        "best_match": raw_results[0] if raw_results else None,
        "all_recommendations": raw_results
    }

# --- Execution Instructions ---
# Run via terminal: uvicorn app:app --reload               "Application_name:app"