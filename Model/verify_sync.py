"""
Baytology: Data Integrity Auditor
---------------------------------
Ensures that the FAISS index, the Encoded features, and the Master metadata
are perfectly aligned in terms of row count and ID sequence.
"""

import faiss
import pandas as pd
import os
import sys

def check_alignment():
    # --- 1. DYNAMIC PATH SETUP ---
    # Finds the absolute path to this script
    current_script_path = os.path.abspath(__file__)
    # Goes up one level (Model/) and then another to reach (Project Root/)
    root_dir = os.path.dirname(os.path.dirname(current_script_path))

    index_path = os.path.join(root_dir, 'faiss_index.bin')
    master_path = os.path.join(root_dir, 'Datasets', 'master_dataset.csv')
    encoded_path = os.path.join(root_dir, 'Datasets', 'preprocessed_dataset.csv')

    print(f"📂 Project Root: {root_dir}")
    
    # --- 2. FILE EXISTENCE CHECK ---
    files = {
        "FAISS Index": index_path,
        "Master CSV": master_path,
        "Encoded CSV": encoded_path
    }

    for name, path in files.items():
        if not os.path.exists(path):
            print(f"❌ MISSING {name} at: {path}")
            return

    # --- 3. LOAD & AUDIT ---
    try:
        # Load only necessary columns to save RAM
        index = faiss.read_index(index_path)
        df_master = pd.read_csv(master_path, usecols=['id'])
        df_encoded = pd.read_csv(encoded_path, usecols=['id'])

        faiss_count = index.ntotal
        master_count = len(df_master)
        encoded_count = len(df_encoded)

        print("-" * 40)
        print(f"📊 INTEGRITY REPORT")
        print("-" * 40)
        print(f"🔹 FAISS Index:    {faiss_count} vectors")
        print(f"🔹 Master CSV:     {master_count} records")
        print(f"🔹 Encoded CSV:    {encoded_count} records")
        
        # --- 4. DEEP ALIGNMENT CHECK ---
        # Check if the IDs are identical and in the same order
        ids_match = df_master['id'].equals(df_encoded['id'])

        errors = []
        if faiss_count != master_count:
            errors.append(f"Row Mismatch: FAISS ({faiss_count}) != Master ({master_count})")
        if not ids_match:
            errors.append("Sequence Mismatch: IDs in Master and Encoded files do not align!")

        # --- 5. THE VERDICT ---
        if not errors:
            print("-" * 40)
            print("✨ SUCCESS: Data is perfectly synchronized!")
            print("✅ Row counts match.")
            print("✅ ID sequences match.")
            print("-" * 40)
        else:
            print("-" * 40)
            print("⚠️ CRITICAL ALIGNMENT ERROR!")
            for err in errors:
                print(f"👉 {err}")
            print("\nREQUIRED ACTION:")
            print("1. Delete 'faiss_index.bin' and both CSVs in 'Datasets/'.")
            print("2. Re-run 'python Data_preprocessing.py'.")
            print("3. Re-run 'python Model/Model_training.py'.")
            sys.exit(1) # Exit with error code for the main pipeline

    except Exception as e:
        print(f"❌ Integrity Check Failed: {e}")
        sys.exit(1)

if __name__ == "__main__":
    check_alignment()