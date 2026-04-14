# import subprocess
# import os
# import sys

# def run_script(script_path):
#     """Executes a sub-script and handles failures."""
#     print(f"▶️ Executing: {script_path}...")
#     try:
#         # Standardize path for cross-platform (Windows/Linux)
#         script_full_path = os.path.join(os.getcwd(), script_path)
        
#         # Add project root to PYTHONPATH so scripts can find each other
#         env = os.environ.copy()
#         env["PYTHONPATH"] = os.getcwd() 
        
#         # Run the script and wait for exit code
#         subprocess.run([sys.executable, script_full_path], check=True, env=env)
#         print(f"✅ Success: {script_path}\n")
#     except subprocess.CalledProcessError:
#         print(f"\n❌ PIPELINE HALTED: {script_path} reported an error.")
#         print("Please fix the issue above before running the pipeline again.")
#         sys.exit(1)

# def main():
#     print("="*50)
#     print("🏠 BAYTOLOGY AUTOMATION PIPELINE")
#     print("="*50)

#     # 1. Clean and Encode Data
#     run_script('Data_preprocessing.py')

#     # 2. Train the FAISS Vector Index
#     run_script(os.path.join('Model', 'Model_training.py'))

#     # 3. Perform Integrity Handshake (The New verify_sync)
#     run_script(os.path.join('Model', 'verify_sync.py'))

#     print("="*50)
#     print("🎉 ALL SYSTEMS GO!")
#     print("Data is cleaned, Model is trained, and Sync is verified.")
#     print("You can now start the API with: uvicorn app:app --reload")
#     print("="*50)

# if __name__ == "__main__":
#     main()




"""
Baytology: Main Orchestration Pipeline
--------------------------------------
Automates Preprocessing -> Training -> Integrity Verification.
"""

import subprocess
import os
import sys

def run_script(script_path):
    """Executes a sub-script and handles failures."""
    print(f"▶️ Executing: {script_path}...")
    try:
        # Standardize path for cross-platform (Windows/Linux)
        script_full_path = os.path.join(os.getcwd(), script_path)
        
        # Add project root to PYTHONPATH so scripts can find each other
        env = os.environ.copy()
        env["PYTHONPATH"] = os.getcwd() 
        
        # Run the script and wait for exit code
        subprocess.run([sys.executable, script_full_path], check=True, env=env)
        print(f"✅ Success: {script_path}\n")
    except subprocess.CalledProcessError:
        print(f"\n❌ PIPELINE HALTED: {script_path} reported an error.")
        print("Please fix the issue above before running the pipeline again.")
        sys.exit(1)

def main():
    print("="*50)
    print("🏠 BAYTOLOGY AUTOMATION PIPELINE")
    print("="*50)

    # 1. Clean and Encode Data
    run_script('Data_preprocessing.py')

    # 2. Train the FAISS Vector Index
    run_script(os.path.join('Model', 'Model_training.py'))

    # 3. Perform Integrity Handshake (The New verify_sync)
    run_script(os.path.join('Model', 'verify_sync.py'))

    print("="*50)
    print("🎉 ALL SYSTEMS GO!")
    print("Data is cleaned, Model is trained, and Sync is verified.")
    print("You can now start the API with: uvicorn app:app --reload")
    print("="*50)

if __name__ == "__main__":
    main()