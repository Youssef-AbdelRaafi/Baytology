"""
Baytology Data Preprocessing Pipeline
--------------------------------------
Purpose: Cleans raw real estate data, handles missing values, removes outliers, 
and encodes features for similarity search (FAISS).

Outputs:
1. master_dataset.csv: Human-readable data for display in the API.
2. preprocessed_dataset.csv: Scaled, numerical features for the FAISS index.
"""

import os
import pandas as pd
import numpy as np
from sklearn.preprocessing import StandardScaler, LabelEncoder

# --- 1. DATA LOADING ---
def load_data():
    try:
        data = pd.read_csv('Datasets/egypt_real_estate_listings.csv')
        print(f"✅ Successfully loaded {len(data)} raw records.")
        return data
    except FileNotFoundError:
        print("❌ Error: Raw dataset not found in 'Datasets/'.")
        exit()

df = load_data()

# --- 2. ID SYNCHRONIZATION ---
# Ensures every property has a unique, sequential ID for easy lookup.
if 'id' not in df.columns:
    df.insert(0, 'id', np.arange(1, len(df) + 1))
else:
    df['id'] = np.arange(1, len(df) + 1)

# --- 3. NUMERIC CLEANING & TYPE CASTING ---
# Removes commas and currency symbols, then converts to float.
numeric_columns = ['price', 'size', 'bedrooms', 'bathrooms', 'down_payment']
for col in numeric_columns:
    df[col] = pd.to_numeric(
        df[col].astype(str).str.replace(',', '').str.extract(r'(\d+\.?\d*)')[0], 
        errors='coerce'
    )

# --- 4. NULL VALUE MANAGEMENT ---
# Assume missing down_payment is a cash payment (0 down).
df['down_payment'] = df['down_payment'].fillna(0)

# Drop records missing critical information required for search or display.
df = df.dropna(subset=['price', 'size', 'location'])

# --- 5. OUTLIER REMOVAL ---
# Uses the Interquartile Range (IQR) method to remove unrealistic data points.
# Formula applied: $[Q1 - 1.5 \times IQR, Q3 + 1.5 \times IQR]$
def remove_outliers(data, columns):
    for col in columns:
        Q1 = data[col].quantile(0.05)  # 5th percentile
        Q3 = data[col].quantile(0.95)  # 95th percentile
        IQR = Q3 - Q1
        data = data[(data[col] >= Q1 - 1.5 * IQR) & (data[col] <= Q3 + 1.5 * IQR)]
    return data

df = remove_outliers(df, ['price', 'size'])

# --- 6. CATEGORICAL ENCODING ---
# We create a separate dataframe for features to avoid messing up display data.
df_encoded = df.copy()

# Label Encoding for high-cardinality location data.
le = LabelEncoder()
df_encoded['location'] = le.fit_transform(df_encoded['location'].fillna('Unknown'))

# One-Hot Encoding for small-variety categories (Type and Payment Method).
categorical_columns = ['type', 'payment_method']
df_encoded = pd.get_dummies(df_encoded, columns=categorical_columns)

# Drop non-numeric columns that cannot be processed by the FAISS index.
df_encoded.drop(['available_from', 'url', 'description'], axis=1, inplace=True, errors='ignore')

# --- 7. FINAL SYNCHRONIZATION (The "Source of Truth" Step) ---
# This ensures that df_encoded and df have the EXACT same rows in the EXACT same order.
df_encoded = df_encoded.dropna()
df = df.loc[df_encoded.index].reset_index(drop=True)
df_encoded = df_encoded.reset_index(drop=True)

# --- 8. FEATURE SCALING ---
# Standardizes features (mean=0, variance=1) so one variable doesn't dominate the search.
scaler = StandardScaler()
features_to_scale = df_encoded.columns.drop('id')
df_encoded[features_to_scale] = scaler.fit_transform(df_encoded[features_to_scale])



# --- 9. EXPORTING DATASETS ---
if __name__ == "__main__":
    os.makedirs('Datasets', exist_ok=True)
    
    # Save the feature set for FAISS Model Training.
    df_encoded.to_csv('Datasets/preprocessed_dataset.csv', index=False, encoding='utf-8')
    
    # Save the master set for API Display.
    df.to_csv('Datasets/master_dataset.csv', index=False, encoding='utf-8')
    
    print("-" * 30)
    print(f"🚀 Preprocessing Complete!")
    print(f"Total Synchronized Rows: {len(df)}")
    print("-" * 30)