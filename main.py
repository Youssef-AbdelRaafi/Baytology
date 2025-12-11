from parser.parse_user_txt import parse_user_query
from parser.data_validation import RealEstateQuery
from search_engine.search_engine import load_data, filter_properties
from model.helper_functions import score_and_rank
from search_engine.calculate_entropy import ask_user_question_Based_on_entropy
from langchain_google_genai import ChatGoogleGenerativeAI
import os
import joblib
from dotenv import load_dotenv  
import time



# 1. Load the .env file immediately
load_dotenv()



# change the current working dir
os.chdir(os.getcwd())

# Load the .env file

os.getenv("GOOGLE_API_KEY")
# get the gemini api key
# os.environ["GOOGLE_API_KEY"] = api_key

# 1. Load Data
csv_path = os.getenv("CSV_FILE_PATH")

# Now load
df = load_data(csv_path)  

# 2. Get User Input
# user_text = "عايز شقه في القاهرة ب 3 مليون و فيها 3 خمامات"
# user_text = "انت ممكن تساعدني ازاي"
user_text_1 = "عايز اشتري شقة في التجمع"
user_text_2 = "بضور على حاجة في بادية بالم هيلز في حدود 5 مليون جنيه"
user_text_3 = "محتاج تاون هاوس في العاصمة الإدارية R7، الميزانية 8 مليون وهدفع قسط. ضروري يكون فيه غرفة شغالة."



# 3. CONFIGURE GEMINI
llm = ChatGoogleGenerativeAI(
    model="gemini-2.5-flash",
    temperature=0  # 0 means "be precise, don't be creative"
)



structured_llm = llm.with_structured_output(RealEstateQuery)


# 3. Phase 1: AI Understanding
print("🤖 AI is thinking...")

# calculate time for the 2 requests to gemini
start_time = time.perf_counter()

# parse the user input
filters = parse_user_query(structured_llm=structured_llm,text=user_text_3)

# filters = {'location': 'Cairo', 'property_type': 'Apartment', 'min_bedrooms': 2, 'min_bathrooms': None, 'max_price': 3000000.0, 'payment_method': None}
print(f"\n🔍 Extracted Filters: {filters}")


# 4. Phase 2: Database Search
matches = filter_properties(df, filters)
# print(type(matches))
print(" ")
print(ask_user_question_Based_on_entropy(matches, filters))

end_time = time.perf_counter()
elapsed_time = end_time - start_time
print(f"Execution time: {elapsed_time:.4f} seconds")
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
    model = joblib.load('../Baytology/model/brains/price_model.pkl')
    le_location = joblib.load('../Baytology/model/brains/location_encoder.pkl')
    le_type = joblib.load('../Baytology/model/brains/type_encoder.pkl')
    le_payment = joblib.load('../Baytology/model/brains/payment_encoder.pkl')
    # print("\n✅ AI Judge loaded successfully.")

except FileNotFoundError:
    print("❌ Error: Model files not found. Run train_model.py first.")



if not matches.empty:
    print("\n⭐ AI Judge is evaluating deals (Value for Money)...")
    
    # 👇 CALL THE RANKER HERE
    # This adds 'fair_price' and 'deal_score' columns and sorts them
    best_houses = score_and_rank(candidates_df=matches,le_location=le_location, le_type=le_type,le_payment=le_payment,model=model)
    
    # Show Top 3 Recommendations
    print(f"TOP 5 RECOMMENDED DEALS:")
    for index, row in best_houses.head(5).iterrows():
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