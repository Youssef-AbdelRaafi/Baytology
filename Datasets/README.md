<a id="datasets-directory"></a>

## 📂 Datasets Directory

This directory contains the **raw data** and **generated artifacts** used to power the **Baytology Recommendation Engine**.

> **Note:** To keep the repository lightweight and Git-friendly, large `.csv` files are intentionally ignored.  
> Follow the instructions below to correctly set up the data environment before running the pipeline.

---

### 📥 1. Original Dataset (Required)

The system is built using the **Egyptian Real Estate Listings** dataset.

**Source:** Kaggle — *Egyptian Real Estate Listings*  
  https://www.kaggle.com/datasets/hassankhaled21/egyptian-real-estate-listings

- **Filename:** `egypt_real_estate_listings.csv`
- **Instruction:**  
  Download the dataset and place the file directly inside the `Datasets/` directory.

```text
Datasets/
 └── egypt_real_estate_listings.csv
```
**⚠️ The pipeline will not run without this file**
---
### ⚙️ 2. Generated Datasets (Automated)

Once the original dataset is in place, running the pipeline:
```bash
python main.py
```
or
```bash
python Data_preprocessing.py
```
will automatically generate the following files inside the same directory:

The following datasets are generated automatically by the preprocessing pipeline:

| Filename                   | Description                                                                |
|----------------------------|----------------------------------------------------------------------------|
| `master_dataset.csv`       | Cleaned, human-readable dataset used by the API for final property display |
| `preprocessed_dataset.csv` | Encoded and normalized dataset used to train the FAISS model               |
---

## 🛠️ Data Pipeline Flow
Input:
  egypt_real_estate_listings.csv  (Raw Data)

Process:
  - Data Cleaning
  - One-Hot Encoding
  - Numerical Normalization

Output:
  - **master_dataset.csv**
- **preprocessed_dataset.csv**

Verification:
 -  **verify_sync.py** → validates alignment with `faiss_index.bin`
 The verification step guarantees that:

- Dataset row order

- Property IDs

- FAISS vector positions 
<br>are perfectly synchronized.</br>
---
## ⚠️ Important Note for Users

If you modify the original dataset or add new property listings, you must re-run the full pipeline:
```bash
python main.py
```
## 🎯 Why This Matters

This structure adds:

* Reproducibility: Anyone cloning the repository knows exactly where to obtain and place the data

* Clarity: Generated files are clearly explained and justified

* Professionalism: Demonstrates a full understanding of the Data Lifecycle
(Raw → Processed → Model → API)
