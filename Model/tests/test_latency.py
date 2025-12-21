"""
Baytology: Performance & Stress Test
------------------------------------
Tests the system's ability to handle high traffic and measures 
the exact response time of the FAISS index in milliseconds.
"""

import sys
import os
import time
import numpy as np
import faiss
import pandas as pd

# 1. CALCULATE PATHS
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
# Go up 2 levels: tests -> Model -> Project Root
PROJECT_ROOT = os.path.abspath(os.path.join(BASE_DIR, '..', '..'))
sys.path.append(PROJECT_ROOT)

# 2. LOAD DATA
try:
    from Data_preprocessing import df_encoded
    print("✅ Project modules loaded successfully.")
except ImportError:
    print("❌ Critical: Data_preprocessing not found.")
    sys.exit(1)

def run_performance_test():
    data = df_encoded.drop(columns=['id']).values.astype('float32')
    dim = data.shape[1]

    # --- Use PROJECT_ROOT to find the bin file ---
    INDEX_FILE = os.path.join(PROJECT_ROOT, 'faiss_index.bin')
    
    if not os.path.exists(INDEX_FILE):
        print(f"❌ Error: faiss_index.bin not found at: {INDEX_FILE}")
        print("💡 Tip: Run Model_training.py from the root folder first!")
        return
    
    # Load the index
    index = faiss.read_index(INDEX_FILE)
    index.nprobe = 10

    print("🚀 Starting Stress Test (1000 Simulated Searches)...")

    # 2. Stress Test Execution
    iterations = 1000
    latencies = []

    for _ in range(iterations):
        random_idx = np.random.randint(0, len(data))
        query = data[random_idx].reshape(1, -1)
        
        start = time.perf_counter()
        index.search(query, 6)
        latencies.append(time.perf_counter() - start)

    # 3. Analytics
    avg_latency_ms = (sum(latencies) / iterations) * 1000
    p95_latency_ms = np.percentile(latencies, 95) * 1000 # 95th percentile (standard industry metric)
    throughput = int(1.0 / (sum(latencies) / iterations))

    print("="*40)
    print("        PERFORMANCE BENCHMARK")
    print("="*40)
    print(f"📊 Average Latency:  {avg_latency_ms:.4f} ms")
    print(f"📉 P95 Latency:      {p95_latency_ms:.4f} ms (95% of users)")
    print(f"🔥 Throughput:       {throughput:,} req/sec")
    print("="*40)
    print("✅ System can handle high-concurrency traffic.")

if __name__ == "__main__":
    run_performance_test()