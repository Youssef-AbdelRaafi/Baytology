"""
Baytology: FAISS Model Training
-------------------------------
This script builds the vector similarity index. It uses an IVFFlat (Inverted File Index)
which clusters data to ensure fast search times as the dataset grows.

Input: preprocessed_dataset.csv
Output: faiss_index.bin
"""

import sys
import pandas as pd
import faiss
import numpy as np
import os

def build_index():
    # --- 1. CONFIGURATION & PATHS ---
    # Ensure we find the CSVs relative to the project root
    BASE_DIR = os.path.dirname(os.path.abspath(__file__))
    ENCODED_PATH = os.path.join(BASE_DIR, '..', 'Datasets', 'preprocessed_dataset.csv')
    MASTER_PATH = os.path.join(BASE_DIR, '..', 'Datasets', 'master_dataset.csv')
    INDEX_EXPORT_PATH = os.path.join(BASE_DIR, '..', 'faiss_index.bin')

    if not os.path.exists(ENCODED_PATH):
        print(f"❌ Error: Preprocessed data not found at {ENCODED_PATH}")
        return

    # --- 2. DATA PREPARATION ---
    # Load the features prepared by Data_preprocessing.py
    df_encoded = pd.read_csv(ENCODED_PATH)
    
    # Drop 'id' to isolate pure numerical features for the vector space
    features = df_encoded.drop(columns=['id'])
    data = features.values.astype('float32')
    dim = data.shape[1]

    print(f"📊 Features detected: {dim}")
    print(f"🏠 Total properties to index: {len(data)}")

    # --- 3. CONSTRUCTING THE FAISS INDEX ---
    # IVFFlat (Inverted File Index) is great for balancing speed and accuracy.
    # It partitions the vector space into clusters (nlist).
    nlist = 100  # Number of clusters (centroids)
    quantizer = faiss.IndexFlatL2(dim)
    index = faiss.IndexIVFFlat(quantizer, dim, nlist)

    # Train the index: The index needs to 'see' the distribution 
    # of the data to create the clusters (centroids).
    print("🧠 Training vector clusters...")
    index.train(data)

    # Add the vectors into their respective clusters
    index.add(data)

    # --- 4. SAVING THE MODEL ---
    try:
        faiss.write_index(index, INDEX_EXPORT_PATH)
        print(f"✅ FAISS index successfully saved to: {INDEX_EXPORT_PATH}")
    except Exception as e:
        print(f"❌ Error saving index: {e}")

    # --- 5. VERIFICATION TEST ---
    # Test-drive the index with a sample record
    test_recommendation(data, index, MASTER_PATH)

def test_recommendation(data, index, master_path):
    """Simple test function to verify search quality after training."""
    try:
        df_master = pd.read_csv(master_path)
        sample_idx = 0  # Test first record
        query = data[sample_idx].reshape(1, -1)
        
        # Search for top 3 similar properties
        distances, indices = index.search(query, 3)
        
        print("\n🧪 Verification Test:")
        print(f"Target Property: {df_master.iloc[sample_idx]['location']} - {df_master.iloc[sample_idx]['price']} EGP")
        print(f"Top Recommendation Index: {indices[0][1]}") # Skip first as it's the target itself
    except Exception as e:
        print(f"⚠️ Test failed: {e}")

if __name__ == "__main__":
    build_index()