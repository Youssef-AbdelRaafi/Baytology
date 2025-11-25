# app_test.py
from parser.parse_user_txt import parse_user_query
from parser.data_validation import RealEstateQuery
from search_engine.search_engine import load_data, filter_properties
from model.helper_functions import score_and_rank
import os
from langchain_google_genai import ChatGoogleGenerativeAI
import joblib



# change the current working dir
os.chdir(os.getcwd())

# get the gemini api key
os.environ["GOOGLE_API_KEY"] = "AIzaSyAg1iQn7OCCGnECwI0JCXkvm3uar2mridM"

# 1. Load Data
df = load_data("../Baytology/egypt_real_estate_preprocessed.csv")

# 2. Get User Input
user_text = "عايز شقه في القاهرة ب 3 مليون و فيها 3 خمامات"


# 3. CONFIGURE GEMINI
llm = ChatGoogleGenerativeAI(
    model="gemini-2.5-flash",
    temperature=0  # 0 means "be precise, don't be creative"
)

structured_llm = llm.with_structured_output(RealEstateQuery)


# 3. Phase 1: AI Understanding
print("🤖 AI is thinking...")

# parse the user input
filters = parse_user_query(structured_llm=structured_llm,text=user_text)

# print(f"\n🔍 Extracted Filters: {filters}")


# 4. Phase 2: Database Search
matches = filter_properties(df, filters)
# print(f"\n🏠 Found {len(matches)} houses matching your request.")

# if len(matches) > 0:
#     print(matches.head())
# else:
#     print("No houses found! Try increasing your budget.")


# ==========================================
# 6. PHASE 3: AI JUDGE (The Ranker)
# ==========================================
# 1. LOAD THE SAVED BRAINS


# We load the model and the translators we just created
try:
    model = joblib.load('../Baytology/model/price_model.pkl')
    le_location = joblib.load('../Baytology/model/location_encoder.pkl')
    le_type = joblib.load('../Baytology/model/type_encoder.pkl')
    le_payment = joblib.load('../Baytology/model/payment_encoder.pkl')
    print("\n✅ AI Judge loaded successfully.")

except FileNotFoundError:
    print("❌ Error: Model files not found. Run train_model.py first.")



if not matches.empty:
    print("\n⭐ AI Judge is evaluating deals (Value for Money)...")
    
    # 👇 CALL THE RANKER HERE
    # This adds 'fair_price' and 'deal_score' columns and sorts them
    best_houses = score_and_rank(candidates_df=matches,le_location=le_location, le_type=le_type,le_payment=le_payment,model=model)
    
    # Show Top 3 Recommendations
    print(f"TOP {len(matches)} RECOMMENDED DEALS:")
    for index, row in best_houses.head(len(matches)).iterrows():
        print("-" * 30)
        print(f"{row['type']} in {row['location']}")
        print(f"Price: {row['price']:,.0f} EGP")
        print(f"Fair Value: {row['fair_price']:,.0f} EGP")

        print(f"bedrooms: {row['bedrooms']}")
        print(f"bathrooms: {row['bathrooms']}")
        print(f"payment_method: {row['payment_method']}")
        print(f"size: {row['size_sqm']}")
        
        # Explain the score
        score = row['deal_score']
        status = "Good Deal (Undervalued)" if score > 0 else "Premium/Overpriced"
        print(f"Deal Score: {score:,.0f} ({status})")
        
else:
    print("No houses found! Try increasing your budget or changing location.")