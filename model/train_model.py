import pandas as pd
import joblib
from sklearn.ensemble import RandomForestRegressor
from sklearn.preprocessing import LabelEncoder

# 1. LOAD DATA
df = pd.read_csv("/home/saad-naeem/Desktop/Level_4/Graduation_project_saad/chatbot/Baytology/egypt_real_estate_preprocessed.csv")

# 1. Force convert price to numbers (turns bad text into NaN)
df['price'] = pd.to_numeric(df['price'], errors='coerce')

# 2. Drop rows where Price is missing (We can't train on them)
df.dropna(subset=['price'], inplace=True)

# 3. Fill missing values in other columns (Features cannot be NaN either)
# If bedrooms/bathrooms are missing, assume 1 or 0 to save the row
df['bedrooms'] = df['bedrooms'].fillna(1)
df['bathrooms'] = df['bathrooms'].fillna(1)
df['size_sqm'] = df['size_sqm'].fillna(df['size_sqm'].mean()) # Fill size with average

print(f"✅ Data Cleaned. Rows remaining for training: {len(df)}")

# 2. PREPROCESSING (Text -> Numbers)
le_location = LabelEncoder()
le_type = LabelEncoder()
le_payment = LabelEncoder()

# Use 'astype(str)' to handle any remaining weird values safely
df['location_code'] = le_location.fit_transform(df['location'].astype(str))
df['type_code'] = le_type.fit_transform(df['type'].astype(str))
df['payment_code'] = le_payment.fit_transform(df['payment_method'].astype(str))

# 3. DEFINE FEATURES & TARGET
features = [
    'location_code', 
    'type_code', 
    'bedrooms', 
    'bathrooms', 
    'size_sqm', 
    'payment_code'
]

X = df[features]
y = df['price']

# 4. TRAIN THE MODEL
print("⏳ Training the Deal Hunter Model...")
model = RandomForestRegressor(n_estimators=100, random_state=42)
model.fit(X, y)
print("✅ Training Complete!")

# 5. SAVE THE BRAIN
joblib.dump(model, 'price_model.pkl')
joblib.dump(le_location, 'location_encoder.pkl')
joblib.dump(le_type, 'type_encoder.pkl')
joblib.dump(le_payment, 'payment_encoder.pkl')

print("💾 All files saved successfully.")