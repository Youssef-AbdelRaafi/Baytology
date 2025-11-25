import pandas as pd

# ==========================================
# 1. LOAD AND PREP DATA
# ==========================================
def load_data(filepath):
    """
    Loads data and ensures columns are numeric so filtering works.
    """
    df = pd.read_csv(filepath)
    
    # Ensure numbers are actually numbers (not strings)
    # If you already cleaned the CSV, this is just a safety check
    df['price'] = pd.to_numeric(df['price'], errors='coerce')
    df['bedrooms'] = pd.to_numeric(df['bedrooms'], errors='coerce')
    df['bathrooms'] = pd.to_numeric(df['bathrooms'], errors='coerce')
    df['size_sqm'] = pd.to_numeric(df['size_sqm'], errors='coerce')
    
    return df

# ==========================================
# 2. THE FILTER FUNCTION
# ==========================================
def filter_properties(df: pd.DataFrame, filters: dict):
    """
    Takes the Full DataFrame and the Filter Dictionary.
    Returns a filtered DataFrame.
    """
    # Start with all data
    filtered_df = df.copy()

    # A. Filter by Location (Partial Match)
    # If user said 'Maadi', we match 'Maadi, Cairo' or 'Maadi Corniche'
    if filters.get("location"):
        loc = filters["location"]
        filtered_df = filtered_df[
            filtered_df["location"].str.contains(loc, case=False, na=False)
        ]

    # B. Filter by Price (Budget)
    # If user says max 2M, we want price <= 2M
    if filters.get("max_price"):
        filtered_df = filtered_df[
            filtered_df["price"] <= filters["max_price"]
        ]

    # C. Filter by Bedrooms (Minimum Requirement)
    if filters.get("min_bedrooms"):
        filtered_df = filtered_df[
            filtered_df["bedrooms"] >= filters["min_bedrooms"]
        ]

    # D. Filter by Bathrooms
    if filters.get("min_bathrooms"):
        filtered_df = filtered_df[
            filtered_df["bathrooms"] >= filters["min_bathrooms"]
        ]

    # E. Filter by Property Type (Exact Match)
    # Note: JSON key is 'property_type', but CSV column is 'type'
    if filters.get("property_type"):
        ptype = filters["property_type"]
        filtered_df = filtered_df[
            filtered_df["type"].str.lower() == ptype.lower()
        ]

    # F. Filter by Payment Method (Exact Match)
    if filters.get("payment_method"):
        method = filters["payment_method"]
        filtered_df = filtered_df[
            filtered_df["payment_method"].str.lower() == method.lower()
        ]
        
    # G. Filter by Size (Minimum)
    if filters.get("min_size_sqm"):
        filtered_df = filtered_df[
            filtered_df["size_sqm"] >= filters["min_size_sqm"]
        ]

    return filtered_df

# ==========================================
# 3. TEST ZONE
# ==========================================
if __name__ == "__main__":
    # Load your CSV (Make sure the file exists!)
    # Use the cleaned version you made earlier
    try:
        df = load_data("/home/saad-naeem/Desktop/Level_4/Graduation_project_saad/chatbot/Baytology/egypt_real_estate_preprocessed.csv") 
        print(f"Loaded {len(df)} houses.")
        
        # Mock Input from Phase 1 (The Parser)
        test_filters = {
            "location": "New Cairo",   # Will match 'Fifth Settlement, New Cairo...'
            "max_price": 5000000,
            "min_bedrooms": 3,
            "property_type": "Apartment"
        }
        
        print(f"\nApplying filters: {test_filters}")
        results = filter_properties(df, test_filters)
        
        print(f"Found {len(results)} matches.")
        print(results.head(5)) # Show top 5
        
    except FileNotFoundError:
        print("❌ Error: 'cleaned_houses.csv' not found. Please create it first.")