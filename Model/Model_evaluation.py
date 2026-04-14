"""
Baytology: Model Evaluation Suite
---------------------------------
This script evaluates the trade-off between Search Speed and Accuracy (Recall).
It compares our Approximate Nearest Neighbor (ANN) index against an 
'Exact Search' (Ground Truth) to see how many true neighbors we might be missing.

Metrics:
- Recall@10: The percentage of true nearest neighbors found by the model.
- Latency: Average time taken per recommendation request.
"""

import sys 
import os
import time
import numpy as np
import faiss
import pandas as pd

# Path setup to access preprocessing logic
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))
from Data_preprocessing import df_encoded

def evaluate_model():
    # --- 1. PREPARE DATA ---
    # Convert features to float32 for FAISS compatibility
    data = df_encoded.drop(columns=['id']).values.astype('float32')
    dim = data.shape[1]
    
    # Use a sample of 100 queries to measure performance
    query_sample = data[:100] 

    print("📊 Starting Evaluation...")
    print(f"Testing against {len(data)} properties with {dim} dimensions.\n")

    # --- 2. GENERATE GROUND TRUTH (Exact Search) ---
    # IndexFlatL2 provides 100% accuracy but is slow for large data.
    exact_index = faiss.IndexFlatL2(dim)
    exact_index.add(data)
    
    start_exact = time.time()
    _, ground_truth_indices = exact_index.search(query_sample, 10)
    exact_latency = (time.time() - start_exact) / len(query_sample)

    # --- 3. EVALUATE OUR ANN MODEL (Approximate Search) ---
    # We rebuild the IndexIVFFlat used in production
    nlist = 100
    quantizer = faiss.IndexFlatL2(dim)
    ann_index = faiss.IndexIVFFlat(quantizer, dim, nlist)
    ann_index.train(data)
    ann_index.add(data)
    
    # Sensitivity Tuning: nprobe determines how many clusters to search
    ann_index.nprobe = 10 

    start_ann = time.time()
    _, ann_indices = ann_index.search(query_sample, 10)
    ann_latency = (time.time() - start_ann) / len(query_sample)

    # --- 4. CALCULATE RECALL@10 ---
    def calculate_recall(true_indices, pred_indices):
        total_overlap = 0
        for true, pred in zip(true_indices, pred_indices):
            # Intersection: How many of our results are in the 'perfect' results?
            total_overlap += len(np.intersect1d(true, pred))
        
        # Recall = (Found Relevant) / (Total Relevant)
        recall = total_overlap / (true_indices.shape[0] * true_indices.shape[1])
        return recall

    recall_score = calculate_recall(ground_truth_indices, ann_indices)

    # --- 5. RESULTS REPORT ---
    print("="*40)
    print("       MODEL EVALUATION REPORT")
    print("="*40)
    print(f"✅ Recall@10:      {recall_score * 100:.2f}%")
    print(f"⚡ ANN Latency:    {ann_latency * 1000:.4f} ms per query")
    print(f"🐢 Exact Latency:  {exact_latency * 1000:.4f} ms per query")
    print(f"🚀 Speedup Factor: {exact_latency / ann_latency:.1f}x faster")
    print("-" * 40)
    print("INTERPRETATION:")
    if recall_score > 0.95:
        print("The model is highly accurate and production-ready.")
    elif recall_score > 0.80:
        print("Good balance. Minor neighbors might be missed for speed.")
    else:
        print("Consider increasing 'nprobe' to improve search quality.")
    print("="*40)

if __name__ == "__main__":
    evaluate_model()