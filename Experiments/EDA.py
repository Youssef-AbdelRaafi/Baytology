import os
import pandas as pd
import numpy as np
from sklearn.preprocessing import LabelEncoder, StandardScaler

# 1. Read dataset
df = pd.read_csv('../Datasets/egypt_real_estate_listings.csv')

# 2. Add/Sync ID column
if 'id' not in df.columns:
    df.insert(0, 'id', np.arange(1, len(df) + 1))
else:
    df['id'] = np.arange(1, len(df) + 1)

# 3. Clean numeric columns
numeric_columns = ['price', 'size', 'bedrooms', 'bathrooms', 'down_payment']
for col in numeric_columns:
    df[col] = pd.to_numeric(
        df[col].astype(str).str.replace(',', '').str.extract(r'(\d+\.?\d*)')[0], 
        errors='coerce'
    )

# 4. Prepare Encoding
categorical_columns = ['location', 'type', 'payment_method']
df_encoded = df.copy()

# Fill NaNs and Encode
for col in categorical_columns:
    le = LabelEncoder()
    df_encoded[col] = df_encoded[col].fillna('Unknown')
    df_encoded[col] = le.fit_transform(df_encoded[col])

# Drop unnecessary columns
df_encoded.drop(['available_from', 'url', 'description'], axis=1, inplace=True)

# 5. SYNC DATA (Crucial for FAISS)
# We drop NaNs from encoded data, then make sure 'df' matches those same rows
df_encoded = df_encoded.dropna()
df = df.loc[df_encoded.index].reset_index(drop=True)
df_encoded = df_encoded.reset_index(drop=True)

# 6. Feature Scaling
scaler = StandardScaler()
numerical_features = ['price', 'size', 'bedrooms', 'bathrooms', 'down_payment']
df_encoded[numerical_features] = scaler.fit_transform(df_encoded[numerical_features])

# --- Logic to run only when executing this file directly ---
if __name__ == "__main__":
    file_path = 'Datasets/preprocessed_dataset.csv'
    master_path = 'Datasets/master_dataset.csv'
    os.makedirs('Datasets', exist_ok=True)

    # Save Encoded Data (For FAISS)
    if not os.path.exists(file_path):
        df_encoded.to_csv(file_path, index=False, encoding='utf-8')
        print(f"SUCCESS: Encoded data saved to {file_path}")
    
    # Save Master Data (For Website Display)
    if not os.path.exists(master_path):
        df.to_csv(master_path, index=False, encoding='utf-8')
        print(f"SUCCESS: Master data saved to {master_path}")
    
    print(f"EDA Complete. Total synchronized rows: {len(df)}")