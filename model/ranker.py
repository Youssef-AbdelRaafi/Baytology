# ranker.py
import pandas as pd
import joblib
import numpy as np
import os


os.path.dirname(__file__)

# 1. LOAD THE SAVED BRAINS
# We load the model and the translators we just created
try:
    model = joblib.load('price_model.pkl')
    le_location = joblib.load('location_encoder.pkl')
    le_type = joblib.load('type_encoder.pkl')
    le_payment = joblib.load('payment_encoder.pkl')
    print("✅ AI Judge loaded successfully.")
except FileNotFoundError:
    print("❌ Error: Model files not found. Run train_model.py first.")

# ==========================================
# 2. HELPER: SAFE ENCODING
# ==========================================
def safe_transform(encoder, value):
    """
    Tries to convert text to number. 
    If the text was never seen during training (e.g., a new compound), 
    we return a default value (like 0) so the app doesn't crash.
    """
    try:
        return encoder.transform([str(value)])[0]
    except:
        # If the model doesn't know this location, use the code for the first location it knows
        # (Not perfect, but prevents crashing in a demo)
        return 0 

# ==========================================
# 3. THE RANKING FUNCTION
# ==========================================
def score_and_rank(candidates_df):
    """
    Input: DataFrame of houses (e.g., 10 houses matching user filter).
    Output: Top 5 houses sorted by 'Deal Score'.
    """
    if candidates_df.empty:
        return candidates_df

    # A. Prepare Data for Prediction (Text -> Numbers)
    # We create a temporary DataFrame just for the model to read
    X_pred = pd.DataFrame()
    
    # Apply the "Code Book" to every row
    X_pred['location_code'] = candidates_df['location'].apply(lambda x: safe_transform(le_location, x))
    X_pred['type_code'] = candidates_df['type'].apply(lambda x: safe_transform(le_type, x))
    X_pred['bedrooms'] = candidates_df['bedrooms']
    X_pred['bathrooms'] = candidates_df['bathrooms']
    X_pred['size_sqm'] = candidates_df['size_sqm']
    X_pred['payment_code'] = candidates_df['payment_method'].apply(lambda x: safe_transform(le_payment, x))

    # B. Ask the Model: "What is the fair price?"
    predicted_prices = model.predict(X_pred)
    
    # C. Calculate the Score
    # Score = (Fair Price - Actual Price)
    # Example: Worth 5M - Seller asks 4M = +1M Score (Great Deal)
    candidates_df = candidates_df.copy() # Avoid pandas warnings
    candidates_df['fair_price'] = predicted_prices
    candidates_df['deal_score'] = candidates_df['fair_price'] - candidates_df['price']
    
    # D. Sort: Highest score first (Best deals on top)
    # We take top 5
    top_picks = candidates_df.sort_values(by='deal_score', ascending=False).head(5)
    
    return top_picks