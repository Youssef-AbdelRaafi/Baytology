# 🏠 Baytology – Vector-Based Real Estate Recommendation Engine

> **Branch:** `ml-recommendation-engine`  
> **Role:** Machine Learning Engineer – Recommendation Systems  
> **Company:** Baytology  
> **Author:** Omar Ahmed Al-Beltagy

## 📑 Table of Contents

1. [Project Identity & Abstract](#section-1)
2. [Technical Architecture](#section-2)
3. [Feature Engineering & Preprocessing](#section-3)
4. [The Recommendation Model](#section-4)
5. [Data Integrity & Automation](#section-5)
6. [Evaluation & Benchmarks](#section-6)
7. [API Documentation & Usage](#section-7)
8. [Deployment & Reproducibility](#section-8)


## 📂 Project Structure

The repository follows a modular and production-oriented layout to clearly separate
data handling, model logic, evaluation, and API services.

```text
Baytology/
├── Datasets/
│   ├── egypt_real_estate_listings.csv   # Raw real estate data (external source)
│   ├── master_dataset.csv               # Cleaned dataset for API responses
│   └── preprocessed_dataset.csv         # Encoded & normalized data for model training
├── Model/
│   ├── tests/
│   │   └── test_latency.py              # Latency & throughput benchmarks
│   ├── Model_training.py                # Builds the FAISS index
│   └── Model_evaluation.py              # Computes Recall@K metrics
├── app.py                               # FastAPI recommendation service
├── Data_preprocessing.py                # ETL, cleaning & feature engineering
├── main.py                              # End-to-end orchestration pipeline
├── faiss_index.bin                      # Trained FAISS ANN model
└── requirements.txt                     # Python dependencies
```
---

<a id="section-1"></a>

## 📂 Section 1: Project Identity & Abstract

### 🪝 The Hook

**Baytology** is an intelligent real estate discovery platform designed to transform how users find homes in Egypt.  
At its core lies a **high-performance Vector Recommendation Engine** that goes far beyond traditional search filters.

Instead of simply searching for *“3 bedrooms”* or *“price = 2,000,000 EGP”*, the system understands the **“DNA” of a property**—a mathematical representation that captures price, location, size, and amenities.  
This allows Baytology to surface the **most relevant and similar properties in under a millisecond**, enabling true discovery rather than rigid lookup.

---

### ⚠️ The Problem: Limitations of Traditional Search

Most existing real estate platforms rely on **relational database queries** using SQL (`WHERE`, `LIKE`). While functional, this approach breaks down in recommendation scenarios due to two critical limitations:

#### 1️⃣ The “Exact Match” Trap
Traditional filters are binary—you are either *in* or *out*.

- A user searching for a property priced at **2,000,000 EGP**
- A near-perfect option priced at **2,005,000 EGP** is ignored
- No ability to retrieve *similar* or *close alternatives*

This makes discovery impossible and results in missed opportunities for both users and platforms.

#### 2️⃣ Scalability Bottlenecks
As listings scale into the **hundreds of thousands**, SQL-based similarity logic becomes computationally expensive:

- High query latency
- Poor user experience
- Limited real-time personalization

Relational databases are not designed for high-dimensional similarity search.

---

### 💡 The Solution: Vector Embeddings & ANN

To overcome these limitations, Baytology uses **Vector Embeddings** combined with **Approximate Nearest Neighbor (ANN)** search.

#### 🔢 Vectorization
Each property is transformed into a **multi-dimensional numerical vector** representing its features.  
Properties that are similar in the real world are placed **close together in vector space**.

#### ⚡ FAISS (Approximate Nearest Neighbors)
Using **Facebook AI Similarity Search (FAISS)**, the system avoids brute-force comparisons:

- Uses advanced indexing (`IndexIVFFlat`)
- Narrows the search space to the most relevant clusters
- Enables sub-millisecond similarity queries

#### ✅ Result
This architecture enables:

- **Fuzzy Matching** instead of strict filters
- Discovery of high-quality alternatives
- Industry-leading speed and scalability
- A modern, AI-driven real estate search experience

---


<a id="section-2"></a>

## 🛠️ Section 2: Technical Architecture

This section explains the **engine under the hood**—how the Baytology recommendation system is structured and why each technology was chosen to ensure **high performance, scalability, and reliability**.

---

### 🏗️ High-Level System Workflow

The system follows a **Decoupled Pipeline Architecture**, where data processing, model training, and API serving are independent modules that communicate through standardized artifacts (`.csv` and `.bin` files).

This design improves:
- Maintainability
- Reproducibility
- Safe iteration and retraining

Raw Data
↓
ETL / Preprocessing
↓
Vector Indexing (FAISS)
↓
Recommendation API (FastAPI)


---

### 🧱 Architectural Layers

#### 1️⃣ ETL Layer – Data Preprocessing
Responsible for converting raw Egyptian real estate listings into machine-readable vectors.

Key responsibilities:
- Cleaning raw data
- Handling missing values
- Encoding categorical features
- Normalizing numerical attributes

**Output:**  
`preprocessed_dataset.csv`

---

#### 2️⃣ Indexing Layer – FAISS
This layer handles high-speed similarity search.

- Consumes numerical vectors from preprocessing
- Builds a **FAISS IndexIVFFlat**
- Partitions vector space into **Voronoi cells**
- Searches only the most relevant clusters instead of the full dataset

**Output:**  
`faiss_index.bin`

---

#### 3️⃣ Service Layer – FastAPI
Provides real-time recommendations to client applications.

Responsibilities:
- Accepts a property ID as input
- Queries the FAISS index
- Retrieves the top **K** nearest neighbors
- Returns similarity scores and property metadata

---

### 💻 Tech Stack

| Component | Technology | Reason |
|--------|------------|--------|
| Language | Python 3.x | Strong ML & data ecosystem |
| Vector Engine | FAISS (CPU) | Microsecond-scale ANN search |
| Data Processing | Pandas, NumPy | Efficient matrix & feature operations |
| API Framework | FastAPI, Uvicorn | Async, fast, auto-documented |
| ML Utilities | Scikit-learn | Scaling & preprocessing |

---

### 🧠 Algorithm Choice: Why `IndexIVFFlat`?

Instead of using **IndexFlatL2** (exact, brute-force search), this system uses **IndexIVFFlat (Inverted File Index)**.

#### 🔍 How It Works
- Uses a **coarse quantizer** (K-Means clustering)
- Groups vectors into clusters
- At query time:
  - Identifies the closest clusters
  - Searches only within those clusters

#### 🚀 The Benefit
- Drastically fewer distance computations
- **12.9× speedup** compared to exact search
- Maintains **100% Recall@10**

This provides an optimal balance between **accuracy and performance**.

---

### 📁 Component Interaction & Orchestration

The entire system is orchestrated through a central pipeline script: `main.py`.

Execution sequence:

1. `Data_preprocessing.py`  
   → Generates `preprocessed_dataset.csv`

2. `Model_training.py`  
   → Consumes the CSV and builds `faiss_index.bin`

3. `verify_sync.py`  
   → Ensures the FAISS index and dataset share the same ID ordering

4. `app.py`  
   → Loads both artifacts and serves recommendations via API

This orchestration guarantees that **data, model, and service are always synchronized**.

---
<a id="section-3"></a>

## 🧪 Section 3: Feature Engineering & Preprocessing

The success of a recommendation engine depends **less on the algorithm itself and more on how the data is prepared**.  
This section details the **Transformation Pipeline** used to convert raw real estate listings—textual and numerical—into a clean, machine-readable representation suitable for vector-based similarity search.

All steps in this section are implemented in `Data_preprocessing.py`.

---

### 🧹 1. Data Cleaning & Imputation

Raw real estate data often contains noise, inconsistencies, and invalid records. To ensure a stable and meaningful vector space, the pipeline performs:

#### 🔹 Handling Missing Values
- Listings missing **critical attributes** such as:
  - Price
  - Area  
- These records are removed entirely to preserve vector integrity.

This prevents incomplete feature vectors that could distort similarity calculations.

#### 🔹 Outlier Removal
- Extreme or unrealistic listings are filtered out  
  *(e.g., a 10-bedroom apartment priced at 100 EGP)*

Outliers can significantly skew distance metrics and degrade recommendation quality if left untreated.

---

### 🔢 2. Categorical Encoding (The Nominal Map)

Real estate is heavily defined by **non-numeric attributes** such as:
- Location
- Property type

Since mathematical models cannot operate on raw text, these features are transformed using **One-Hot Encoding (OHE)**.

#### 🔹 Mechanism
- Each unique category becomes its own binary column  
  *(e.g., `location_new_cairo`, `location_sheikh_zayed`)*

#### 🔹 Why One-Hot Encoding?
- Prevents false ordinal relationships
- Ensures the model does **not** assume:
  - `"New Cairo" > "Sheikh Zayed"`

Each category is treated as an **independent, equal dimension** in vector space.

---

### 📏 3. Numerical Normalization (The Scale Balancer)

Numerical features in real estate exist on vastly different scales:

- Price → millions
- Bedrooms → typically 1–5
- Bathrooms → typically 1–4

#### 🔴 The Problem
Without scaling, large numeric ranges (e.g., price) dominate the distance metric, making other features effectively irrelevant.

#### ✅ The Solution: Min-Max Scaling
All numerical features are normalized to the range: `[0, 1]`


This ensures:
- A **10% difference in price**
- Has the same mathematical weight as
- A **10% difference in area or room count**

Resulting in balanced, fair similarity comparisons.

---

### ⛓️ 4. The “Feature DNA” Vector

The final output of preprocessing is a **single, wide feature matrix** where:
- Each row represents one property
- Each column represents a normalized or encoded feature

#### 📥 Input
- 19,000+ raw property listings

#### 📤 Output
- A high-dimensional tensor ready for FAISS indexing

#### 🔧 Technical Note
- All preprocessing steps are stored in the `df_encoded` DataFrame
- The final dataset is saved as: `preprocessed_dataset.csv`

This file serves as the **ground truth** for:
- Model training
- FAISS index construction
- Dataset–index verification

---

<a id="section-4"></a>

## 🧠 Section 4: The Recommendation Model (The “AI” Part)

This section describes the **mathematical core** of the Baytology recommendation engine—how the system defines *similarity*, searches efficiently, and produces high-quality property recommendations.

---

### 📍 1. Vector Space Representation

In this system, **each property is represented as a point in a multi-dimensional vector space**.

- If the model only considered **Price** and **Area**, properties would exist in a 2D space.
- In reality, the engine considers:
  - Price
  - Area
  - Bedrooms
  - Bathrooms
  - Property type
  - Location (one-hot encoded)
  - Additional categorical dimensions

As a result, each property lives in a **high-dimensional coordinate system**, where spatial proximity directly corresponds to real-world similarity.

---

### 📏 2. Distance Metric: L2 (Euclidean) Distance

To quantify similarity between a **query property** (*q*) and a **candidate property** (*p*),
the system computes the **Euclidean (L2) distance** between their feature vectors:

**d(q, p) = √(∑ᵢ₌₁ⁿ (qᵢ − pᵢ)²)**

#### Interpretation
- **Small distance** → Properties are highly similar  
  *(e.g., same neighborhood, comparable price and size)*

- **Large distance** → Properties differ significantly

#### This metric provides a **clear, interpretable, and scale-consistent** measure of similarity in high-dimensional vector space, making it well-suited for FAISS-based nearest neighbor search.
---

### 🏗️ 3. Indexing Strategy: IVFFlat

Rather than performing an expensive **flat (brute-force) search**, the system uses a
**FAISS Inverted File Index (IVFFlat)** to significantly accelerate nearest-neighbor retrieval.

#### 🔹 Clustering
- The vector space is partitioned using **K-Means clustering**
- **Number of clusters (k):** 100
- Each cluster represents a **Voronoi cell** in high-dimensional space

#### 🔹 Centroids
- Each cluster is represented by a **centroid** (reference vector)
- All properties are assigned to the nearest centroid during indexing

#### 🔹 Search Process
When a user selects a property:

1. FAISS identifies the closest cluster centroids
2. The search is restricted to vectors within those clusters (the inverted lists)
3. The `nprobe` parameter controls how many neighboring clusters are examined to
   balance **recall vs. latency**

#### This strategy dramatically reduces the number of distance computations from **O(N)** to approximately **O(√N)**, while preserving high retrieval accuracy.
---

### 🎯 4. Recommendation Output

For each query, the model returns:

- **Top 6 nearest neighbors**
- **Distance (similarity) scores** for each result

#### 🔢 Distance Scores
- `0.0` → Exact match (typically the queried property)
- Larger values → Increasing dissimilarity

#### 💡 Why This Matters
This approach enables **true discovery**:
- Users are not limited to rigid filters
- The system surfaces properties the user *did not explicitly search for*
- Recommendations align with the **mathematical profile of the user’s ideal home**

---

<a id="section-5"></a>

## 🛡️ Section 5: Data Integrity & Automation

A recommendation engine is only as reliable as the data it serves.  
This section describes the **Safety Layer** built into Baytology to guarantee that the **vector model and property metadata remain perfectly synchronized at all times**.

---

### 🤝 1. The “Integrity Handshake” (`verify_sync.py`)

In vector-based systems, the **model (FAISS index)** and the **metadata (CSV files)** are stored separately.  
If the row order in the dataset changes without rebuilding the FAISS index, the system may return **incorrect recommendations**—a silent but critical failure.

To prevent this, a custom **Synchronization Auditor** was implemented.

#### 🔍 Validation Checks

The auditor performs a **three-way consistency check**:

1. **Row Count Validation**  
   - Ensures the number of vectors in `faiss_index.bin`
   - Exactly matches the number of rows in `master_dataset.csv`

2. **ID Sequence Matching**  
   - Verifies that **Property IDs** appear in the *exact same order*
   - Across both the encoded dataset and the master dataset

3. **Error Prevention**
   - Any mismatch triggers a **Critical Error**
   - System execution is halted immediately
   - Prevents serving corrupted or misaligned recommendations

This guarantees **data–model alignment by design**, not assumption.

---

### ⚙️ 2. The Orchestration Pipeline (`main.py`)

To make the system **production-ready and developer-friendly**, a centralized automation script was created.

Instead of manually running multiple scripts, a single command executes the entire lifecycle.

#### 🔁 Pipeline Responsibilities

Running `main.py` performs the following steps sequentially:

1. **Data Preprocessing**  
   - Executes `Data_preprocessing.py`
   - Regenerates feature vectors

2. **Model Training**  
   - Executes `Model_training.py`
   - Rebuilds the FAISS index using updated data

3. **Integrity Verification**  
   - Executes `verify_sync.py`
   - Confirms dataset–index synchronization

4. **Final Reporting**
   - Outputs an **“All Systems Go”** status upon success

This orchestration ensures consistency, repeatability, and safe retraining.

---

### 🛠️ 3. Robust Path Management

To maximize portability and reproducibility, **Dynamic Path Resolution** is used across all scripts.

#### 🔹 Implementation
- Paths are computed dynamically using `os.path`
- A centralized `PROJECT_ROOT` is inferred at runtime

#### 🔹 Benefits
- No hard-coded file paths
- Seamless migration between:
  - Local development
  - Cloud servers
  - Academic or demo environments

This makes the project **fully portable and reproducible** without configuration changes.

---

## 🛡️ Section 5: Data Integrity & Automation

A recommendation engine is only as reliable as the data it serves.  
This section describes the **Safety Layer** built into Baytology to guarantee that the **vector model and property metadata remain perfectly synchronized at all times**.

---

### 🤝 1. The “Integrity Handshake” (`verify_sync.py`)

In vector-based systems, the **model (FAISS index)** and the **metadata (CSV files)** are stored separately.  
If the row order in the dataset changes without rebuilding the FAISS index, the system may return **incorrect recommendations**—a silent but critical failure.

To prevent this, a custom **Synchronization Auditor** was implemented.

#### 🔍 Validation Checks

The auditor performs a **three-way consistency check**:

1. **Row Count Validation**  
   - Ensures the number of vectors in `faiss_index.bin`
   - Exactly matches the number of rows in `master_dataset.csv`

2. **ID Sequence Matching**  
   - Verifies that **Property IDs** appear in the *exact same order*
   - Across both the encoded dataset and the master dataset

3. **Error Prevention**
   - Any mismatch triggers a **Critical Error**
   - System execution is halted immediately
   - Prevents serving corrupted or misaligned recommendations

This guarantees **data–model alignment by design**, not assumption.

---

### ⚙️ 2. The Orchestration Pipeline (`main.py`)

To make the system **production-ready and developer-friendly**, a centralized automation script was created.

Instead of manually running multiple scripts, a single command executes the entire lifecycle.

#### 🔁 Pipeline Responsibilities

Running `main.py` performs the following steps sequentially:

1. **Data Preprocessing**  
   - Executes `Data_preprocessing.py`
   - Regenerates feature vectors

2. **Model Training**  
   - Executes `Model_training.py`
   - Rebuilds the FAISS index using updated data

3. **Integrity Verification**  
   - Executes `verify_sync.py`
   - Confirms dataset–index synchronization

4. **Final Reporting**
   - Outputs an **“All Systems Go”** status upon success

This orchestration ensures consistency, repeatability, and safe retraining.

---

### 🛠️ 3. Robust Path Management

To maximize portability and reproducibility, **Dynamic Path Resolution** is used across all scripts.

#### 🔹 Implementation
- Paths are computed dynamically using `os.path`
- A centralized `PROJECT_ROOT` is inferred at runtime

#### 🔹 Benefits
- No hard-coded file paths
- Seamless migration between:
  - Local development
  - Cloud servers
  - Academic or demo environments

This makes the project **fully portable and reproducible** without configuration changes.

---

<a id="section-6"></a>

## 📈 Section 6: Evaluation & Benchmarks (The Evidence)

This section provides **empirical evidence** that the Baytology recommendation engine is not only accurate, but also **optimized for production-grade performance**.  
All evaluations were conducted to validate **correctness, speed, scalability, and resource efficiency**.

---

### 🎯 1. Accuracy: Recall@10

In recommendation systems, **Recall@K** measures how many of the true top-K most similar items are successfully retrieved.

#### 🔬 Evaluation Method
- Compared results from:
  - **Optimized FAISS Index (IVFFlat)**
  - **Brute-Force Search** (exact L2 distance over all rows)

The brute-force approach serves as the ground truth.

#### ✅ Results
- **Recall@10 = 100.00%**

#### 📌 Interpretation
- The optimized ANN index **never missed** a true top-tier recommendation
- Performance optimizations did **not compromise accuracy**
- Confirms correctness of clustering and search parameters

---

### ⚡ 2. Search Latency & Speedup

Latency benchmarks measured the time required to retrieve **10 recommendations** from a dataset containing thousands of properties.

#### ⏱️ Performance Comparison

| Method | Average Latency | Performance |
|------|----------------|-------------|
| Brute Force (Flat Search) | 0.5055 ms | Baseline |
| Optimized FAISS (IVFFlat) | 0.0391 ms | **12.9× Faster** |

#### 🚀 Conclusion
By leveraging FAISS clustering:
- Search time was reduced by **over 92%**
- Enables **near-instantaneous recommendations**
- Maintains low latency even under heavy load

---

### 🚀 3. Throughput & Stress Testing

To simulate real-world usage, the system was stress-tested using **1,000 concurrent similarity queries**.

#### 📊 Results
- **Total execution time:** ~0.039 seconds
- **Throughput:** ~25,554 requests per second

#### 📈 Scalability Impact
This level of throughput ensures that Baytology can:
- Handle sudden traffic spikes
- Support large-scale user activity
- Operate without costly infrastructure scaling

---

### 📉 4. Memory Efficiency

Memory usage was optimized through:
- **Inverted File Index (IVFFlat)**
- Use of **float32** vectors

#### 💾 Memory Footprint
- FAISS index size: **< 5 MB** (current dataset)

#### 💡 Why This Matters
- Enables deployment on:
  - Low-cost cloud instances
  - Lightweight containers
  - Edge or resource-constrained environments

This makes the engine both **high-performance and cost-efficient**.

---

<a id="section-7"></a>

## 💻 Section 7: API Documentation & Usage

The Baytology Recommendation Engine is exposed as a **high-performance REST API** built using **FastAPI**.  
This enables the web front-end and mobile applications to request real-time property recommendations via standard HTTP calls.

---

### 📡 1. API Endpoints

#### `GET /`
**Purpose:**  
Health check and system status.

**Response:**  
Confirms that:
- The FAISS index is successfully loaded
- The recommendation service is running and ready to accept requests

---

#### `GET /recommend/{house_id}`
**Purpose:**  
Core recommendation endpoint.

**Parameters:**
- `house_id` – Unique identifier of the property currently being viewed

**Request Logic:**
1. Retrieve the feature vector corresponding to the provided `house_id`
2. Query the FAISS index for the **top 6 nearest neighbors**
3. Exclude the query property itself from the results
4. Join vector-based results with `master_dataset.csv`
5. Return human-readable property details (price, location, rooms) along with similarity scores

---

### 📥 2. Example Request & Response

#### 🔗 Request
GET http://127.0.0.1:8000/recommend/500


#### ✅ Success Response (JSON)
```json
{
  "target_property": {
    "id": 500,
    "type": "Apartment",
    "price": 2500000,
    "location": "New Cairo"
  },
  "recommendations": [
    {
      "id": 1204,
      "similarity_score": 0.012,
      "details": {
        "price": 2550000,
        "location": "New Cairo",
        "rooms": 3
      }
    },
    {
      "id": 89,
      "similarity_score": 0.045,
      "details": {
        "price": 2400000,
        "location": "New Cairo",
        "rooms": 3
      }
    }
  ]
} 
```
----
### ⚡ 3. Real-Time Performance

FAISS index is fully loaded into RAM

Typical end-to-end API response time: < 10 ms

Ensures a smooth, zero-lag browsing experience for users
---

### 🛠️ 4. Interactive Documentation

FastAPI automatically generates an interactive API documentation interface.

`📍 Documentation URL:`
http://127.0.0.1:8000/docs

`✨ Features:`

Test recommendation endpoints directly from the browser

View request and response schemas
* No client-side code required
---

<a id="section-8"></a>

## 📋 Section 8: Deployment & Reproducibility

This section ensures that the Baytology recommendation engine can be **easily deployed, reproduced, and audited** by other developers or examiners.  
The project follows **Clean Code and MLOps best practices**, enabling safe setup, execution, and evaluation across different environments.

---

### 📥 1. Environment Setup

The project requires **Python 3.8+**.  
Using a virtual environment is strongly recommended to isolate dependencies.

#### ⚠️ Critical Note for Windows Users: > The faiss-cpu library is not natively compatible with standard Windows pip installations in some environments. To run this project on Windows, you must use one of the following methods:
Option A: Anaconda / Miniconda (Recommended)
Conda handles the complex C++ dependencies required by FAISS automatically.

```bash
# Create a conda environment
conda create -n baytology python=3.9
conda activate baytology

# Install FAISS via the pytorch channel
conda install -c pytorch faiss-cpu

# Install remaining requirements
pip install -r requirements.txt
---
```

#### Option B: Windows Subsystem for Linux (WSL)
If you prefer a Linux environment on Windows, use WSL (Ubuntu). FAISS installs smoothly via pip in Linux environments.
 ```bash
# Inside your WSL terminal
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
---
```
#### Option C: Standard Virtual Environment (If applicable)
If you are on a system where FAISS binary wheels are supported:
```bash
# Create a virtual environment
python -m venv venv

# Activate (Windows)
venv\Scripts\activate

# Install libraries
pip install -r requirements.txt
---
```

### 🚀 2. Running the Full Pipeline

The entire system lifecycle: `Preprocessing → Training → Verification` is fully automated.

You do not need to run individual scripts unless debugging or extending the pipeline.

```bash
# Build the model and verify data integrity
python main.py
```
#### This command:

* Regenerates feature vectors

* Rebuilds the FAISS index

* Verifies dataset–index synchronization

* Confirms system readiness
---
### 📡 3. Launching the Service

Once `faiss_index.bin` has been generated successfully, start the FastAPI service.
```bash 
# Run the API with hot-reload enabled (development mode)
uvicorn app:app --reload
```
#### 🔗 Access URLs

API Base: http://127.0.0.1:8000

Interactive Docs: http://127.0.0.1:8000/docs
---
### 🧪 4. Running Performance Tests

To validate the performance and accuracy metrics reported in this documentation, the benchmark suite can be executed.   
##### `In terminal write `
```bash 
# Test Recall@10 accuracy
python Model/Model_evaluation.py

# Run latency and throughput stress tests
python Model/tests/test_latency.py
```
#### These scripts confirm:

* Recommendation correctness

* Search latency improvements

* System throughput under load

----

### 📝 5. Dependencies (requirements.txt)

The project relies on the following industry-standard libraries:

* `fastapi` – Web framework

* `uvicorn` – ASGI server

* `pandas` – Data manipulation

* `faiss-cpu` – Vector similarity search

* `scikit-learn` – Preprocessing and scaling

* `numpy` – Numerical computation
---

## 📝 License

Distributed under the MIT License.

---

---
**Developed by [OmarAhmBelt113](https://github.com/OmarAhmBelt113)**
