import streamlit as st
import os
import joblib
import pandas as pd

# --- CHANGE 1: Import ChatOpenAI instead of Google ---
from langchain_openai import ChatOpenAI 
import time

# --- Import centralized configuration ---
from config import settings

# --- Import your custom modules ---
from parser.parse_user_txt import parse_user_query
from parser.data_validation import RealEstateQuery
from search_engine.search_engine import load_data, filter_properties
from model.helper_functions import score_and_rank
from search_engine.calculate_entropy import ask_user_question_Based_on_entropy
from ui import display_houses as display_houses_shared

# --- 1. CONFIGURATION & SETUP ---
st.set_page_config(page_title="Baytology Chatbot", page_icon="🏠")
n_houses = settings.max_results

def get_project_root():
    current_dir = os.getcwd()
    if current_dir.endswith("notebooks") or current_dir.endswith("parser"):
        return os.path.dirname(current_dir)
    return current_dir

# --- 2. CACHED RESOURCE LOADING ---
@st.cache_resource
def get_dataset():
    root = get_project_root()
    full_path = os.path.join(root, settings.csv_file_path)
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


# --- CHANGE 2: Updated get_llm to use Local Host ---
@st.cache_resource
def get_llm():
    llm = ChatOpenAI(
        base_url=settings.lm_studio_url,
        api_key="lm-studio",
        model=settings.lm_studio_model,
        temperature=0.1,
    )
    
    # With system prompt in parse_user_txt.py, the model should output English
    return llm.with_structured_output(RealEstateQuery)


# --- 3. INITIALIZE STATE ---
if "messages" not in st.session_state:
    st.session_state.messages = [
        {"role": "assistant", "content": "أهلاً بيك في Baytology! أنا معاك عشان نلاقي بيت أحلامك. بتدور على إيه؟"}
    ]

if "filters" not in st.session_state:
    st.session_state.filters = {}

# Track attributes the user wants to skip (said "no" or "don't care")
if "skipped_attributes" not in st.session_state:
    st.session_state.skipped_attributes = set()

# Track the last attribute we asked about (to know what "no" refers to)
if "last_asked_attribute" not in st.session_state:
    st.session_state.last_asked_attribute = None


# --- Helper: Detect negative/skip responses in Arabic ---
NEGATIVE_RESPONSES = [
    "لا", "لأ", "مش مهم", "مش فارق", "اي حاجة", "اي حاجه", "خلاص",
    "عادي", "مفيش فرق", "كده تمام", "مش عايز", "مش محتاج", "لا شكرا",
    "no", "skip", "any", "doesn't matter", "don't care"
]

def is_negative_response(text: str) -> bool:
    """Check if user response indicates they want to skip the current question."""
    text_lower = text.strip().lower()
    return any(neg in text_lower for neg in NEGATIVE_RESPONSES)


# --- Helper: Map attribute names to Arabic keywords for context ---
ATTRIBUTE_KEYWORDS = {
    "min_bedrooms": "غرف نوم",
    "min_bathrooms": "حمامات", 
    "max_price": "جنيه ميزانية",
    "min_size_sqm": "متر مساحة",
    "compound": "كومبوند",
    "payment_method": "دفع",
    "property_type": "نوع",
    "city": "مدينة",
    "governorate": "محافظة",
    "district": "منطقة",
}


def is_short_response(text: str) -> bool:
    """Check if response is very short (likely just a number or single word)."""
    text = text.strip()
    # It's short if:
    # 1. It's just a number (Arabic or English)
    # 2. It's 3 words or less
    words = text.split()
    return len(words) <= 3 or text.isdigit()


def enhance_short_response(text: str, last_asked_attr: str) -> str:
    """
    Add context to short responses based on what was last asked.
    E.g., if asked about bathrooms and user says "3", return "3 حمامات"
    """
    if not last_asked_attr:
        return text
    
    keyword = ATTRIBUTE_KEYWORDS.get(last_asked_attr, "")
    if keyword:
        return f"{text} {keyword}"
    return text

# --- 4. UI LOGIC ---
st.title("🏠 Baytology Smart Assistant")

df = get_dataset()
model, le_location, le_type, le_payment = get_models()
structured_llm = get_llm()

# Display Chat History
for msg in st.session_state.messages:
    with st.chat_message(msg["role"]):
        st.markdown(msg["content"])

def display_houses(dataframe, limit=20):
    """Wrapper to call shared display_houses with app-level model context."""
    display_houses_shared(st, dataframe, model, le_location, le_type, le_payment, limit)

# --- 5. THE MAIN INTERACTION LOOP ---
if prompt := st.chat_input("اكتب مواصفات الشقة..."):
    
    st.session_state.messages.append({"role": "user", "content": prompt})

    with st.chat_message("user"):
        st.markdown(prompt)

    with st.spinner("Analyzing your request..."):
        try:
            # Check if user wants to skip the last asked attribute
            if st.session_state.last_asked_attribute and is_negative_response(prompt):
                skipped_attr = st.session_state.last_asked_attribute
                st.session_state.skipped_attributes.add(skipped_attr)
                st.session_state.last_asked_attribute = None
                # Don't parse this as a query, just continue to show results
                new_filters = {}
            else:
                # Enhance short responses with context about what was last asked
                # E.g., if asked about bathrooms and user says "3", we send "3 حمامات" to the parser
                parse_text = prompt
                if st.session_state.last_asked_attribute and is_short_response(prompt):
                    parse_text = enhance_short_response(prompt, st.session_state.last_asked_attribute)
                
                new_filters = parse_user_query(structured_llm=structured_llm, text=parse_text)
            
            # Handle case where small model returns None/Error
            if new_filters is None:
                new_filters = {}
                
            for key, value in new_filters.items():
                if value is not None:
                    st.session_state.filters[key] = value
            
            with st.sidebar:
                st.write("Current Filters:", st.session_state.filters)

            # C. Search Database
            matches = filter_properties(df, st.session_state.filters)
            count = len(matches)

            response_text = ""
            
            # CASE 1: No Results
            if count == 0:
                location_keys = ['governorate', 'city', 'district', 'compound']
                fallback_filters = {
                    k: v for k, v in st.session_state.filters.items() 
                    if k in location_keys and v is not None
                }
                
                fallback_matches = pd.DataFrame()
                if fallback_filters: 
                    fallback_matches = filter_properties(df, fallback_filters)
                
                if not fallback_matches.empty:
                    loc_name = list(fallback_filters.values())[0]
                    response_text = f"للأسف مفيش حاجة بالمواصفات دي بالظبط. بس دي **أفضل الفرص المتاحة في {loc_name}**:"
                    st.session_state.messages.append({"role": "assistant", "content": response_text})
                    with st.chat_message("assistant"):
                        st.markdown(response_text)
                        display_houses(fallback_matches, limit=3)
                else:
                    response_text = "للأسف مفيش أي عقارات مطابقة حالياً."
                    st.session_state.messages.append({"role": "assistant", "content": response_text})
                    with st.chat_message("assistant"):
                        st.markdown(response_text)

            # CASE 2: Too Many Results - Ask questions to narrow down
            elif count > n_houses:
                # Pass skipped attributes so we don't ask about them again
                question, asked_attr = ask_user_question_Based_on_entropy(
                    matches, 
                    st.session_state.filters,
                    st.session_state.skipped_attributes
                )
                
                # If asked_attr is None, all attributes are collected/skipped - show results
                if asked_attr is None:
                    response_text = f"تمام! دي أفضل {min(count, 10)} وحدات ليك:"
                    st.session_state.messages.append({"role": "assistant", "content": response_text})
                    with st.chat_message("assistant"):
                        st.markdown(response_text)
                        display_houses(matches, limit=10)
                else:
                    # Track what we asked so we know what "no" refers to next time
                    st.session_state.last_asked_attribute = asked_attr
                    
                    response_text = f"أنا لقيت {count} وحدة مناسبة. بس عشان أحدد الأفضل ليك:\n\n**{question}**"
                    st.session_state.messages.append({"role": "assistant", "content": response_text})
                    with st.chat_message("assistant"):
                        st.markdown(response_text)

            # CASE 3: Good Results
            elif count <= n_houses:
                response_text = f"تمام! لقيت {count} وحدات ممتازة ليك:"
                st.session_state.messages.append({"role": "assistant", "content": response_text})
                with st.chat_message("assistant"):
                    st.markdown(response_text)
                    for index, row in matches.head(count).iterrows():
                        # ... repeated code for card display ...
                        # (Ideally call display_houses function here to clean up)
                        pass 
                    display_houses(matches, limit=count)
                    time.sleep(4)

        except Exception as e:
            st.error(f"Error connecting to local model: {e}")
            st.info("Ensure LM Studio is running and Server is ON.")