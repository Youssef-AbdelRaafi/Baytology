import pandas as pd
import os
from dotenv import load_dotenv

load_dotenv()

os.path.dirname(__file__)

# ==========================================
# 1. LOAD AND PREP DATA
# ==========================================
# def load_data(filepath):
#     """
#     Loads data and ensures columns are numeric so filtering works.
#     """
#     df = pd.read_csv(filepath)
    
#     # Ensure numbers are actually numbers (not strings)
#     # If you already cleaned the CSV, this is just a safety check
#     df['price'] = pd.to_numeric(df['price'], errors='coerce')
#     df['bedrooms'] = pd.to_numeric(df['bedrooms'], errors='coerce')
#     df['bathrooms'] = pd.to_numeric(df['bathrooms'], errors='coerce')
#     df['size_sqm'] = pd.to_numeric(df['size_sqm'], errors='coerce')
    
#     return df

# # ==========================================
# # 2. THE FILTER FUNCTION
# # ==========================================
# def filter_properties(df: pd.DataFrame, filters: dict):
#     """
#     Takes the Full DataFrame and the Filter Dictionary.
#     Returns a filtered DataFrame.
#     """
#     # Start with all data
#     filtered_df = df.copy()

#     # A. Filter by Location (Partial Match)
#     # If user said 'Maadi', we match 'Maadi, Cairo' or 'Maadi Corniche'
#     if filters.get("location"):
#         loc = filters["location"]
#         filtered_df = filtered_df[
#             filtered_df["location"].str.contains(loc, case=False, na=False)
#         ]

#     # B. Filter by Price (Budget)
#     # If user says max 2M, we want price <= 2M
#     if filters.get("max_price"):
#         filtered_df = filtered_df[
#             filtered_df["price"] <= filters["max_price"]
#         ]

#     # C. Filter by Bedrooms (Minimum Requirement)
#     if filters.get("min_bedrooms"):
#         filtered_df = filtered_df[
#             filtered_df["bedrooms"] >= filters["min_bedrooms"]
#         ]

#     # D. Filter by Bathrooms
#     if filters.get("min_bathrooms"):
#         filtered_df = filtered_df[
#             filtered_df["bathrooms"] >= filters["min_bathrooms"]
#         ]

#     # E. Filter by Property Type (Exact Match)
#     # Note: JSON key is 'property_type', but CSV column is 'type'
#     if filters.get("property_type"):
#         ptype = filters["property_type"]
#         filtered_df = filtered_df[
#             filtered_df["type"].str.lower() == ptype.lower()
#         ]

#     # F. Filter by Payment Method (Exact Match)
#     if filters.get("payment_method"):
#         method = filters["payment_method"]
#         filtered_df = filtered_df[
#             filtered_df["payment_method"].str.lower() == method.lower()
#         ]
        
#     # G. Filter by Size (Minimum)
#     if filters.get("min_size_sqm"):
#         filtered_df = filtered_df[
#             filtered_df["size_sqm"] >= filters["min_size_sqm"]
#         ]

#     return filtered_df




def load_data(filepath):
    """
    Loads data and ensures specific columns are numeric/clean for filtering.
    """
    df = pd.read_csv(filepath)
    
    # 1. Clean Numbers (Force to float/int, turn errors to NaN)
    cols_to_numeric = ['price', 'bedrooms', 'bathrooms', 'size_sqm', 'mid_room']
    
    for col in cols_to_numeric:
        if col in df.columns:
            df[col] = pd.to_numeric(df[col], errors='coerce')
    
    # 2. Clean Strings (Optional but good for search: fill NaNs with empty string)
    str_cols = ['governorate', 'city', 'district', 'compound', 'type', 'payment_method']
    for col in str_cols:
        if col in df.columns:
            df[col] = df[col].fillna("").astype(str)

    return df




def filter_properties(df: pd.DataFrame, filters: dict):
    """
    Takes the Full DataFrame and the Pydantic Filter Dictionary.
    Returns a filtered DataFrame based on RealEstateQuery V2 logic.
    """
    # Start with a copy to avoid SettingWithCopy warnings
    filtered_df = df.copy()

    # ==========================================
    # A. LOCATION HIERARCHY FILTERS
    # ==========================================
    
    # 1. Governorate (Partial match to handle "New Cairo, Cairo")
    if filters.get("governorate"):
        val = filters["governorate"]
        filtered_df = filtered_df[
            filtered_df["governorate"].str.contains(val, case=False, na=False)
        ]

    # 2. City (Partial match to handle "New Cairo City")
    if filters.get("city"):
        val = filters["city"]
        filtered_df = filtered_df[
            filtered_df["city"].str.contains(val, case=False, na=False)
        ]

    # 3. District
    if filters.get("district"):
        val = filters["district"]
        filtered_df = filtered_df[
            filtered_df["district"].str.contains(val, case=False, na=False)
        ]

    # 4. Compound
    if filters.get("compound"):
        val = filters["compound"]
        filtered_df = filtered_df[
            filtered_df["compound"].str.contains(val, case=False, na=False)
        ]

    # ==========================================
    # B. NUMERICAL FILTERS (Logic: >= or <=)
    # ==========================================

    # 5. Price (Budget: User's Max >= Database Price)
    if filters.get("max_price"):
        filtered_df = filtered_df[
            filtered_df["price"] <= filters["max_price"]
        ]

    # 6. Bedrooms (Min Requirement: Database Rooms >= User's Min)
    if filters.get("min_bedrooms") is not None:
        filtered_df = filtered_df[
            filtered_df["bedrooms"] >= filters["min_bedrooms"]
        ]

    # 7. Bathrooms (Min Requirement)
    if filters.get("min_bathrooms") is not None:
        filtered_df = filtered_df[
            filtered_df["bathrooms"] >= filters["min_bathrooms"]
        ]

    # 8. Size (Min Requirement)
    if filters.get("min_size_sqm"):
        filtered_df = filtered_df[
            filtered_df["size_sqm"] >= filters["min_size_sqm"]
        ]

    # ==========================================
    # C. EXACT MATCH CATEGORIES
    # ==========================================

    # 9. Property Type (CSV column is 'type', Filter key is 'property_type')
    if filters.get("property_type"):
        ptype = filters["property_type"]
        filtered_df = filtered_df[
            filtered_df["type"].str.lower() == ptype.lower()
        ]

    # 10. Payment Method
    if filters.get("payment_method"):
        method = filters["payment_method"]
        filtered_df = filtered_df[
            filtered_df["payment_method"].str.lower() == method.lower()
        ]

    # ==========================================
    # D. BOOLEAN EXTRAS
    # ==========================================

    # 11. Maid Room (mid_room)
    # If user explicitly wants a maid room (True), filter for 1.
    # If user doesn't care (None/False), we usually don't filter OUT those with maid rooms.
    if filters.get("mid_room") is True:
        filtered_df = filtered_df[
            filtered_df["mid_room"] == 1
        ]

    return filtered_df

# ==========================================
# 3. TEST ZONE
# ==========================================
if __name__ == "__main__":
    # Load your CSV (Make sure the file exists!)
    # Use the cleaned version you made earlier
    try:
        # df = load_data("/home/saad-naeem/Desktop/Level_4/Graduation_project_saad/chatbot/Baytology/notebooks/egypt_real_estate_preprocessed_analysis-and-segmentation.csv") 
        csv_path = os.getenv("CSV_FILE_PATH")

        # Now load
        df = load_data(csv_path)        
        
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