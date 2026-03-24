# Baytology - Technical Documentation

## Egyptian Real Estate Chatbot System

**Version:** 1.0  
**Date:** January 2026  
**Authors:** Graduation Project Team

---

# Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [System Architecture](#2-system-architecture)
3. [Technology Stack](#3-technology-stack)
4. [Component Details](#4-component-details)
5. [API Reference](#5-api-reference)
6. [Machine Learning Pipeline](#6-machine-learning-pipeline)
7. [Natural Language Processing](#7-natural-language-processing)
8. [Data Flow](#8-data-flow)
9. [Deployment Guide](#9-deployment-guide)
10. [Recommendation System (FAISS-Based)](#10-recommendation-system-faiss-based)

---

# 1. Executive Summary

## 1.1 Project Overview

**Baytology** is an AI-powered real estate chatbot designed specifically for the Egyptian market. The system enables users to search for properties using natural language queries in **Egyptian Arabic dialect**, making property search accessible and intuitive.

## 1.2 Key Features

| Feature | Description |
|---------|-------------|
| **Arabic NLP** | Understands Egyptian Arabic dialect queries |
| **Smart Questions** | Entropy-based question generation to narrow results |
| **Deal Scoring** | ML model predicts fair prices and ranks best deals |
| **Intent Classification** | Distinguishes between chat and property search |
| **Skip Functionality** | Users can skip irrelevant questions |

## 1.3 Business Problem Solved

Traditional property search requires users to fill multiple form fields. Baytology allows users to simply say:

> "عايز شقة في التجمع ب 3 مليون"  
> (I want an apartment in Tagamoa for 3 million)

The system understands, filters properties, and asks intelligent follow-up questions.

---

# 2. System Architecture

## 2.1 High-Level Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                 │     │                 │     │                 │
│  Angular        │────▶│  .NET Backend   │────▶│  Baytology      │
│  Frontend       │     │  API Gateway    │     │  Python API     │
│                 │◀────│                 │◀────│                 │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                               │                        │
                               ▼                        ▼
                        ┌─────────────┐          ┌─────────────┐
                        │ SQL Database│          │ LLM (Gemma) │
                        └─────────────┘          └─────────────┘
```

## 2.2 Three-Tier Architecture

### Tier 1: Presentation Layer (Angular)
- User interface for chat interaction
- Property card display
- Session management

### Tier 2: Business Logic Layer (.NET + Python)
- **.NET Backend**: API Gateway, Authentication, Database Operations
- **Python (Baytology)**: NLP Processing, ML Ranking, Entropy Questions

### Tier 3: Data Layer
- SQL Database (via .NET)
- CSV Files (development)
- ML Model Files (.pkl)

---

# 3. Technology Stack

## 3.1 Frontend: Angular

### Why Angular?

| Reason | Explanation |
|--------|-------------|
| **Enterprise-Ready** | Built for large-scale applications |
| **TypeScript** | Type safety reduces runtime errors |
| **Component Architecture** | Modular, reusable UI components |
| **Two-Way Data Binding** | Efficient form handling for chat |
| **RxJS Observables** | Handles real-time API responses |

### Angular Components Used

```typescript
// Main Components
- ChatComponent        // Chat interface
- PropertyCardComponent // Property display
- FilterSidebarComponent // Current filters display
```

## 3.2 Backend Gateway: .NET Framework

### Why .NET?

| Reason | Explanation |
|--------|-------------|
| **API Gateway** | Routes requests between frontend and services |
| **Authentication** | JWT-based security for user sessions |
| **Entity Framework** | ORM for SQL database operations |
| **Scalability** | Handles high concurrent connections |
| **Enterprise Integration** | Works well in corporate environments |

### .NET Responsibilities

```csharp
// .NET Backend handles:
1. User Authentication (JWT Tokens)
2. Session Management
3. Database CRUD Operations
4. Request Routing to Python API
5. Response Aggregation
```

## 3.3 AI/ML Backend: Python (FastAPI)

### Why Python?

| Reason | Explanation |
|--------|-------------|
| **ML Libraries** | scikit-learn, pandas, numpy |
| **LangChain** | LLM orchestration framework |
| **FastAPI** | High-performance async API |
| **Data Science** | Best ecosystem for AI/ML |

### Python Packages

```python
# Core
fastapi==0.128.0
uvicorn==0.40.0
pydantic==2.12.4

# ML/Data
scikit-learn==1.7.2
pandas
numpy

# LLM
langchain-core==1.2.5
langchain-openai==1.1.6
openai==2.14.0
```

## 3.4 LLM: Gemma-2-9B

### Model Details

| Property | Value |
|----------|-------|
| **Model** | Google Gemma 2 9B Instruct |
| **Size** | 9 Billion Parameters |
| **Hosting** | Local (LM Studio) |
| **Purpose** | Arabic text parsing, Question generation |

---

# 4. Component Details

## 4.1 Python API (api.py)

### 4.1.1 Intent Classification

```python
PROPERTY_KEYWORDS = [
    "شقة", "فيلا", "عقار", "بيت", "التجمع", "اكتوبر",
    "غرفة", "حمام", "مساحة", "سعر", "مليون", ...
]

def classify_intent(text: str, session: dict) -> str:
    """
    Returns: 'property_search' or 'general_chat'
    """
    # If user has active filters, stay in property search
    if session.get("filters"):
        return "property_search"
    
    # Check for property keywords
    for keyword in PROPERTY_KEYWORDS:
        if keyword in text:
            return "property_search"
    
    return "general_chat"
```

### 4.1.2 Session Management

```python
sessions = {}  # In-memory storage

def get_session(session_id: str) -> dict:
    if session_id not in sessions:
        sessions[session_id] = {
            "filters": {},
            "skipped_attributes": set(),
            "last_asked_attribute": None
        }
    return sessions[session_id]
```

### 4.1.3 Main Chat Flow

```python
@app.post("/chat")
def chat(request: ChatRequest):
    # Step 1: Classify Intent
    intent = classify_intent(message, session)
    
    if intent == "general_chat":
        return get_chat_response(message)
    
    # Step 2: Parse Arabic Text
    filters = parse_user_query(llm, message)
    session["filters"].update(filters)
    
    # Step 3: Search Properties
    matches = filter_properties(df, session["filters"])
    
    # Step 4: Decision Logic
    if len(matches) == 0:
        return fallback_response()
    elif len(matches) > MAX_RESULTS:
        question = generate_entropy_question(matches)
        return ask_question(question)
    else:
        ranked = rank_properties(matches)
        return show_results(ranked)
```

## 4.2 Parser Module (parse_user_txt.py)

### Purpose
Converts Arabic natural language to structured filters.

### Example

**Input:** `"عايز شقة في التجمع ب 3 مليون 3 غرف"`

**Output:**
```json
{
    "property_type": "Apartment",
    "city": "New Cairo City",
    "max_price": 3000000,
    "min_bedrooms": 3
}
```

### Schema (Pydantic)

```python
class RealEstateQuery(BaseModel):
    governorate: Optional[str]
    city: Optional[str]
    district: Optional[str]
    compound: Optional[str]
    property_type: Optional[str]
    min_bedrooms: Optional[int]
    min_bathrooms: Optional[int]
    max_price: Optional[float]
    min_size_sqm: Optional[float]
    payment_method: Optional[str]
```

## 4.3 Search Engine (search_engine.py)

### Filtering Logic

```python
def filter_properties(df, filters):
    result = df.copy()
    
    if filters.get("city"):
        result = result[result["city"] == filters["city"]]
    
    if filters.get("max_price"):
        result = result[result["price"] <= filters["max_price"]]
    
    if filters.get("min_bedrooms"):
        result = result[result["bedrooms"] >= filters["min_bedrooms"]]
    
    # ... more filters
    return result
```

## 4.4 Entropy Calculator (calculate_entropy.py)

### Purpose
Determines the most informative question to ask next.

### Concept
Shannon Entropy measures information gain. We ask about the attribute that will most effectively narrow down results.

```python
def calculate_entropy(column):
    """
    H(X) = -Σ p(x) * log2(p(x))
    Higher entropy = more variety = better question
    """
    value_counts = column.value_counts(normalize=True)
    return -(value_counts * np.log2(value_counts)).sum()
```

### Question Generation

```python
def ask_user_question_Based_on_entropy(df, current_filters, skipped):
    # Calculate entropy for each unfilled attribute
    entropies = {}
    for attr in ATTRIBUTES:
        if attr not in current_filters and attr not in skipped:
            entropies[attr] = calculate_entropy(df[attr])
    
    # Ask about highest entropy attribute
    best_attr = max(entropies, key=entropies.get)
    question = generate_question_for(best_attr)
    
    return question, best_attr
```

## 4.5 ML Ranker (helper_functions.py)

### Purpose
Ranks properties by "deal score" - how much below predicted price.

### Model
- **Algorithm:** Random Forest Regressor
- **Features:** location, type, bedrooms, bathrooms, size, payment
- **Target:** price

### Scoring

```python
def score_and_rank(df, model, encoders):
    # Predict fair price
    df["predicted_price"] = model.predict(features)
    
    # Deal score = how much cheaper than predicted
    df["deal_score"] = df["predicted_price"] - df["price"]
    
    # Sort by best deals first
    return df.sort_values("deal_score", ascending=False)
```

---

# 5. API Reference

## 5.1 Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `GET /` | GET | Health check |
| `POST /chat` | POST | Main conversation endpoint |
| `POST /parse` | POST | Parse text to filters |
| `POST /question` | POST | Generate follow-up question |
| `POST /rank` | POST | Rank properties |
| `POST /search` | POST | Filter properties |
| `DELETE /session/{id}` | DELETE | Clear session |

## 5.2 Main Endpoint: /chat

### Request
```json
{
    "session_id": "user123",
    "message": "عايز شقة في التجمع"
}
```

### Response Types

**Type: question**
```json
{
    "type": "question",
    "message": "أنا لقيت 150 وحدة مناسبة.",
    "question": "ميزانيتك كام تقريباً؟",
    "attribute": "max_price",
    "properties_count": 150
}
```

**Type: results**
```json
{
    "type": "results",
    "message": "تمام! لقيت 18 وحدات ممتازة ليك:",
    "properties": [...],
    "properties_count": 18
}
```

---

# 6. Machine Learning Pipeline

## 6.1 Training Process

```
Raw Data (CSV)
     │
     ▼
Data Preprocessing
(Handle nulls, encode categories)
     │
     ▼
Feature Engineering
(location, type, bedrooms, etc.)
     │
     ▼
Train Random Forest
     │
     ▼
Save Model (.pkl files)
```

## 6.2 Model Files

```
model/brains/
├── price_model.pkl        # Trained RF model
├── location_encoder.pkl   # City/area encoding
├── type_encoder.pkl       # Property type encoding
└── payment_encoder.pkl    # Payment method encoding
```

---

# 7. Natural Language Processing

## 7.1 Arabic Text Processing

### Challenges
1. Right-to-left text direction
2. Egyptian dialect variations
3. Multiple spellings (ة vs ه)
4. Numerical expressions (مليون, ألف)

### Solutions

**Spelling Variations:**
```python
# Handle both ة and ه endings
"القاهرة", "القاهره"  # Cairo
"الجيزة", "الجيزه"    # Giza
```

**Short Response Enhancement:**
```python
# User says "3" after being asked about bedrooms
# System enhances to "3 غرف نوم" before parsing
def enhance_short_response(text, last_asked_attr):
    keywords = {"min_bedrooms": "غرف نوم", "max_price": "جنيه"}
    return f"{text} {keywords.get(last_asked_attr, '')}"
```

---

# 8. Data Flow

## 8.1 Complete Conversation Flow

```
Step 1: User says "عايز شقة في التجمع"
        │
        ▼
Step 2: Angular sends POST /chat
        │
        ▼
Step 3: .NET routes to Baytology API
        │
        ▼
Step 4: Intent Classification → "property_search"
        │
        ▼
Step 5: LLM parses → {city: "New Cairo", type: "Apartment"}
        │
        ▼
Step 6: Filter properties → 150 matches (too many)
        │
        ▼
Step 7: Calculate entropy → best question: "max_price"
        │
        ▼
Step 8: Return question: "ميزانيتك كام؟"
        │
        ▼
Step 9: User says "3 مليون"
        │
        ▼
Step 10: Parse → {max_price: 3000000}
         │
         ▼
Step 11: Filter → 18 matches (good!)
         │
         ▼
Step 12: ML Rank by deal score
         │
         ▼
Step 13: Return ranked properties
```

---

# 9. Deployment Guide

## 9.1 Development Setup

### Prerequisites
- Python 3.11
- Conda
- LM Studio with Gemma-2-9B

### Commands
```bash
conda create -n chatbot_grade_project python=3.11 -y
conda activate chatbot_grade_project
pip install -r requirements.txt
uvicorn api:app --reload --port 8000
```

## 9.2 Production Architecture

```
                    Load Balancer
                         │
         ┌───────────────┼───────────────┐
         ▼               ▼               ▼
    Angular CDN    .NET API Pod    Python API Pod
                         │               │
                         ▼               ▼
                    PostgreSQL      Redis Cache
```

## 9.3 Environment Variables

```env
GOOGLE_API_KEY=xxx
CSV_FILE_PATH=egypt_real_estate_preprocessed.csv
LM_STUDIO_URL=http://localhost:1234/v1
LM_STUDIO_MODEL=gemma-2-9b-it
MAX_RESULTS=20
```

---

# 10. Recommendation System (FAISS-Based)

In addition to the chatbot search, we implemented a **Content-Based Filtering** recommendation system using **Approximate Nearest Neighbors (ANN)** to discover properties based on their mathematical "closeness" to a user's interest.

## 10.1 Data Representation: The Embedding Layer

The model transforms raw real estate data into a high-dimensional mathematical coordinate system.

### Feature Engineering

| Feature Type | Processing Method |
|-------------|-------------------|
| **Numerical** (Price, Area, Rooms, Baths) | Normalized to $[0, 1]$ scale |
| **Categorical** (Location, Property Type) | One-Hot Encoding |

**Result:** Each property is represented as a point (vector) in a **40+ dimensional space**.

```
Property Vector = [price_norm, area_norm, rooms_norm, baths_norm, 
                   loc_cairo, loc_giza, loc_alex, ..., 
                   type_apt, type_villa, type_duplex, ...]
```

## 10.2 The Core Engine: FAISS IVFFlat Architecture

To handle **millions of listings** with **sub-millisecond speed**, we use **FAISS (Facebook AI Similarity Search)** with an **Inverted File Index (IVFFlat)**.

### Two-Stage Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     TRAINING PHASE                          │
│                                                             │
│   All Properties ──▶ K-Means ──▶ 100 Clusters (Voronoi)    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     SEARCH PHASE                            │
│                                                             │
│   Query ──▶ Find Cluster ──▶ Search Nearby Clusters ──▶ KNN│
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Stage A: The "Librarian" (K-Means Clustering)

During training, the model partitions the Egyptian property market into **100 clusters** (Voronoi Cells).

**Optimization Objective:**

$$J = \sum_{j=1}^{k} \sum_{x \in C_j} \|x - \mu_j\|^2$$

Where:
- $k$ = number of clusters (100)
- $C_j$ = set of properties in cluster $j$
- $\mu_j$ = centroid of cluster $j$
- $x$ = property vector

### Stage B: The "Search" (Approximate KNN)

When a user views a property:

1. **Identify Cluster:** Find which cluster the property belongs to
2. **Probe Neighbors:** Search within that cluster + nearby clusters (controlled by `nprobe` parameter)
3. **Return Similar:** Return K nearest neighbors

```python
# Instead of checking ALL properties (Brute Force O(n))
# We only check properties in relevant clusters O(n/k)
index.nprobe = 10  # Search 10 nearest clusters
distances, indices = index.search(query_vector, k=10)
```

## 10.3 Similarity Logic: Euclidean Distance

The model measures "similarity" using the **L₂ (Euclidean) Norm**:

$$d(q, p) = \sqrt{\sum_{i=1}^{n} (q_i - p_i)^2}$$

Where:
- $q$ = query property vector
- $p$ = candidate property vector
- $n$ = number of dimensions (40+)

### Distance Interpretation

| Distance | Meaning |
|----------|---------|
| **0.0** | Exact match |
| **Small** (< 0.1) | Highly relevant (similar price, area, size) |
| **Large** (> 1.0) | Fundamentally different properties |

### Example

```python
# Property A: Apartment, New Cairo, 3 rooms, 3M EGP
# Property B: Apartment, New Cairo, 3 rooms, 3.2M EGP
distance(A, B) = 0.05  # Very similar!

# Property C: Villa, 6th October, 5 rooms, 10M EGP
distance(A, C) = 0.89  # Very different
```

## 10.4 Performance & Business Value

### Benchmark Results

| Metric | Value | Description |
|--------|-------|-------------|
| **Search Latency** | **0.039 ms** | Near-instantaneous response |
| **Speedup** | **12.9x** | Faster than linear brute-force |
| **Accuracy** | **100% Recall@10** | Matches exact brute-force results |
| **Throughput** | **25,000+ RPS** | Handles high traffic |

### Why This Matters

```
Traditional Search:  O(n) - Check every property
FAISS IVFFlat:       O(n/k) - Check only relevant cluster

With 1,000,000 properties and 100 clusters:
- Brute Force: 1,000,000 comparisons
- FAISS:       ~10,000 comparisons (100x faster)
```

## 10.5 Integration with Chatbot

The recommendation system complements the chatbot:

```
┌─────────────────────────────────────────────────────────────┐
│                   USER JOURNEY                              │
│                                                             │
│  Chatbot Search          Recommendation System              │
│  ─────────────           ─────────────────────              │
│  "عايز شقة في التجمع"   User views Property #123           │
│        │                         │                          │
│        ▼                         ▼                          │
│  Filter by criteria      Find similar properties            │
│        │                         │                          │
│        ▼                         ▼                          │
│  Show 20 results         Show "You might also like"         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

# Appendix A: File Structure

```
Baytology/
├── api.py                  # FastAPI main application
├── config.py               # Centralized configuration
├── requirements.txt        # Python dependencies
├── .env                    # Environment variables
│
├── parser/
│   ├── data_validation.py  # Pydantic schemas
│   └── parse_user_txt.py   # Arabic text parser
│
├── search_engine/
│   ├── search_engine.py    # Property filtering
│   └── calculate_entropy.py # Question generation
│
├── model/
│   ├── train_model.py      # ML training script
│   ├── helper_functions.py # Ranking functions
│   └── brains/             # Saved models
│
├── recommendation/
│   └── faiss_recommender.py # FAISS similarity search
│
└── ui/
    └── __init__.py         # Shared UI components
```

---

# Appendix B: Glossary

| Term | Definition |
|------|------------|
| **Entropy** | Measure of information uncertainty |
| **Deal Score** | Predicted price minus actual price |
| **Intent Classification** | Determining user's goal (search vs chat) |
| **LLM** | Large Language Model |
| **FastAPI** | Python web framework for APIs |
| **Pydantic** | Data validation library |
| **FAISS** | Facebook AI Similarity Search library |
| **K-Means** | Clustering algorithm for partitioning data |
| **Euclidean Distance** | Straight-line distance between vectors |
| **ANN** | Approximate Nearest Neighbors |
| **IVFFlat** | Inverted File Index with flat vectors |

---

**End of Documentation**
