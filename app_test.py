import streamlit as st
import os
import joblib
import pandas as pd
from dotenv import load_dotenv
from langchain_google_genai import ChatGoogleGenerativeAI

# --- Import your custom modules ---
# Ensure your folder structure allows these imports
from parser.parse_user_txt import parse_user_query
from parser.data_validation import RealEstateQuery
from search_engine.search_engine import load_data, filter_properties
from model.helper_functions import score_and_rank
from search_engine.calculate_entropy import ask_user_question_Based_on_entropy

# --- 1. CONFIGURATION & SETUP ---
st.set_page_config(page_title="Baytology Chatbot", page_icon="🏠")

# Load Env
load_dotenv()

# Define project root helper (from our previous conversation)
def get_project_root():
    current_dir = os.getcwd()
    if current_dir.endswith("notebooks") or current_dir.endswith("parser"):
        return os.path.dirname(current_dir)
    return current_dir

# --- 2. CACHED RESOURCE LOADING ---
# We use @st.cache_resource so we only load data/models ONCE, not every reload.

@st.cache_resource
def get_dataset():
    root = get_project_root()
    # Construct absolute path safely
    # Assuming CSV_FILE_PATH in .env is relative like "DataSet/data.csv"
    relative_path = os.getenv("CSV_FILE_PATH")
    if not relative_path:
        st.error("CSV_FILE_PATH not found in .env")
        return pd.DataFrame()
        
    full_path = os.path.join(root, relative_path)
    return load_data(full_path)


@st.cache_resource
def get_models():
    root = get_project_root()
    brains_dir = os.path.join(root, 'model', 'brains')
    
    try:
        model = joblib.load(os.path.join(brains_dir, 'price_model.pkl'))
        le_loc = joblib.load(os.path.join(brains_dir, 'location_encoder.pkl'))
        le_type = joblib.load(os.path.join(brains_dir, 'type_encoder.pkl'))
        le_pay = joblib.load(os.path.join(brains_dir, 'payment_encoder.pkl'))
        return model, le_loc, le_type, le_pay
    except FileNotFoundError:
        st.error("❌ Models not found. Please run your training script first.")
        return None, None, None, None


@st.cache_resource
def get_llm():
    api_key = os.getenv("GOOGLE_API_KEY")
    if not api_key:
        st.error("Google API Key missing.")
        return None
    
    llm = ChatGoogleGenerativeAI(
        model="gemini-2.5-flash", # Updated to 1.5 as discussed
        temperature=0.3,
        google_api_key=api_key
    )
    return llm.with_structured_output(RealEstateQuery)



# --- 3. INITIALIZE STATE ---
# This "remembers" conversation and filters between clicks.
if "messages" not in st.session_state:
    st.session_state.messages = [
        {"role": "assistant", "content": "أهلاً بيك في Baytology! أنا معاك عشان نلاقي بيت أحلامك. بتدور على إيه؟"}
    ]

if "filters" not in st.session_state:
    # Initialize empty filters
    st.session_state.filters = {}



# --- 4. UI LOGIC ---
st.title("🏠 Baytology Smart Assistant")


# Load Data & Models
df = get_dataset()
model, le_location, le_type, le_payment = get_models()
structured_llm = get_llm()



# Display Chat History
for msg in st.session_state.messages:
    with st.chat_message(msg["role"]):
        st.markdown(msg["content"])

def display_houses(dataframe, limit=5):
    """
    Renders HTML cards for houses. Includes ranking logic if models are available.
    """
    # Use the ranker if we have the model and columns are not already scored
    if 'deal_score' not in dataframe.columns and model is not None:
         try:
            dataframe = score_and_rank(dataframe, le_location, le_type, le_payment, model)
         except Exception as e:
            # If ranking fails (e.g. missing columns), just proceed without sorting
            pass
    
    for index, row in dataframe.head(limit).iterrows():
        # Determine status color
        score = row.get('deal_score', 0)
        status_color = "green" if score > 0 else "orange"
        status_text = "صفقة ممتازة (Undervalued)" if score > 0 else "سعر عادل (Fair Price)"
        
        # Card HTML
        card_html = f"""
        <div style="padding:15px; border-radius:10px; border:1px solid #ddd; margin-bottom:10px;">
            <h3 style="margin:0;">🏠 {row['type']} - {row.get('compound', '')} ({row['location']})</h3>
            <p style="font-size:18px; font-weight:bold; color:#0066cc;">
                💰 {row['price']:,.0f} EGP 
            </p>
            <p>🛏 {row['bedrooms']} Beds | 🛁 {row['bathrooms']} Baths | 📏 {row['size_sqm']} m²</p>
            <p>💳 {row['payment_method']}</p>
            <hr>
            <p style="color:{status_color}; font-weight:bold;">✨ {status_text} (Score: {score:.0f})</p>
        </div>
        """
        st.markdown(card_html, unsafe_allow_html=True)


# --- 5. THE MAIN INTERACTION LOOP ---
# This runs whenever the user types something and hits Enter

if prompt := st.chat_input("اكتب مواصفات الشقة..."):
    
    # A. Display User Message
    st.session_state.messages.append({"role": "user", "content": prompt})
    with st.chat_message("user"):
        st.markdown(prompt)

    # B. Parse Input (AI Phase)
    with st.spinner("Analyzing your request..."):
        # Extract NEW filters from this specific message
        new_filters = parse_user_query(structured_llm=structured_llm, text=prompt)
        
        # SMART UPDATE: Only update filters that are NOT None
        # This ensures we don't lose previous info (e.g. if I say "3 rooms", don't forget I said "Cairo" before)
        for key, value in new_filters.items():
            if value is not None:
                st.session_state.filters[key] = value
        
        # Debugging: Show filters in sidebar
        with st.sidebar:
            st.write("Current Filters:", st.session_state.filters)

    # C. Search Database
    matches = filter_properties(df, st.session_state.filters)
    count = len(matches)

    # D. The Decision Tree (The Loop Logic)
    
    response_text = ""
    
    # CASE 1: No Results
    if count == 0:
        # 1. Create a filter with ONLY location keys
        location_keys = ['governorate', 'city', 'district', 'compound']
        fallback_filters = {
            k: v for k, v in st.session_state.filters.items() 
            if k in location_keys and v is not None
        }
        
        # 2. Search again with relaxed filters
        fallback_matches = pd.DataFrame()
        if fallback_filters: 
            fallback_matches = filter_properties(df, fallback_filters)
        
        # 3. Check Fallback Results
        if not fallback_matches.empty:
            loc_name = list(fallback_filters.values())[0]
            response_text = f"للأسف مفيش حاجة بالمواصفات دي بالظبط (ممكن السعر أو المساحة مش متوفرين). بس دي **أفضل الفرص المتاحة في {loc_name}** حالياً:"
            
            st.session_state.messages.append({"role": "assistant", "content": response_text})
            with st.chat_message("assistant"):
                st.markdown(response_text)
                # Show fallback houses
                display_houses(fallback_matches, limit=3)
        else:
            response_text = "للأسف مفيش أي عقارات مطابقة حالياً. ممكن نوسع نطاق البحث (نغير المنطقة مثلاً)؟"
            st.session_state.messages.append({"role": "assistant", "content": response_text})
            with st.chat_message("assistant"):
                st.markdown(response_text)

    # CASE 2: Too Many Results (> 5) -> Run Entropy Logic
    elif count > 10:
        # Generate the smart question
        # Note: ensuring we pass the DATAFRAME and the FILTERS as expected by your function
        question = ask_user_question_Based_on_entropy(matches, st.session_state.filters)
        
        response_text = f"أنا لقيت {count} وحدة مناسبة. بس عشان أحدد الأفضل ليك:\n\n**{question}**"
        
        st.session_state.messages.append({"role": "assistant", "content": response_text})
        with st.chat_message("assistant"):
            st.markdown(response_text)

    # CASE 3: Small Number of Results (<= 5) -> Run Ranker & Show Deals
    elif count <= 10:
        response_text = f"تمام! لقيت {count} وحدات ممتازة ليك. دي أفضل الترشيحات مرتبة حسب القيمة مقابل السعر:"
        
        st.session_state.messages.append({"role": "assistant", "content": response_text})
        with st.chat_message("assistant"):
            st.markdown(response_text)
            
            # Rank Results
            # ranked_df = score_and_rank(matches, le_location, le_type, le_payment, model)
            
            # Create nice UI cards for the houses
            for index, row in matches.head(5).iterrows():
                # Determine status color
                score = row.get('deal_score', 0)
                status_color = "green" if score > 0 else "orange"
                status_text = "صفقة ممتازة (Undervalued)" if score > 0 else "سعر عادل (Fair Price)"
                
                # Card HTML/Markdown
                # Card HTML/Markdown
                card_html = f"""
                <div style="padding:15px; border-radius:10px; border:1px solid #ddd; margin-bottom:10px;">
                    <h3 style="margin:0;">🏠 {row['type']} - {row.get('compound', '')} ({row['location']})</h3>
                    <p style="font-size:18px; font-weight:bold; color:#0066cc;">
                        💰 {row['price']:,.0f} EGP 
                    </p>
                    <p>🛏 {row['bedrooms']} Beds | 🛁 {row['bathrooms']} Baths | 📏 {row['size_sqm']} m²</p>
                    <p>💳 {row['payment_method']}</p>
                    <hr>
                    <p style="color:{status_color}; font-weight:bold;">✨ {status_text} (Score: {score:.0f})</p>
                </div>
                """
                st.markdown(card_html, unsafe_allow_html=True)
                
            # Optional: Add a "Reset" button logic or message
            st.markdown("---")
            st.caption("لو عايز تبدأ بحث جديد، اكتب 'جديد' او غير المواصفات.")