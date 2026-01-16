"""
Baytology FastAPI Backend

Provides endpoints for:
- /parse - Parse Arabic text to structured filters
- /question - Generate entropy-based follow-up questions
- /rank - Rank properties by ML deal score
- /search - Filter properties from database
"""
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional
import pandas as pd

from config import settings
from parser.parse_user_txt import parse_user_query
from parser.data_validation import RealEstateQuery
from search_engine.search_engine import load_data, filter_properties
from search_engine.calculate_entropy import ask_user_question_Based_on_entropy
from model.helper_functions import score_and_rank
from langchain_openai import ChatOpenAI
import joblib
import os

# --- App Setup ---
app = FastAPI(
    title="Baytology API",
    description="Egyptian Real Estate Chatbot API",
    version="1.0.0"
)

# --- CORS for Angular Frontend ---
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify Angular URL
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# --- Session Storage (In-memory for now) ---
sessions = {}


# --- Pydantic Schemas ---
class ParseRequest(BaseModel):
    text: str
    session_id: str


class ParseResponse(BaseModel):
    filters: dict
    message: str = "Parsed successfully"


class QuestionRequest(BaseModel):
    session_id: str
    properties_count: int
    current_filters: dict
    skipped_attributes: list[str] = []


class QuestionResponse(BaseModel):
    question: str
    attribute: Optional[str] = None
    has_question: bool = True


class RankRequest(BaseModel):
    properties: list[dict]


class RankResponse(BaseModel):
    ranked: list[dict]


class SearchRequest(BaseModel):
    filters: dict


class SearchResponse(BaseModel):
    count: int
    properties: list[dict]


class ChatRequest(BaseModel):
    """Combined endpoint for full chat flow"""
    session_id: str
    message: str


class ChatResponse(BaseModel):
    type: str  # "question" | "results" | "no_results"
    message: str
    question: Optional[str] = None
    attribute: Optional[str] = None
    properties: list[dict] = []
    properties_count: int = 0


# --- Load Resources ---
def get_project_root():
    return os.path.dirname(os.path.abspath(__file__))


def load_resources():
    """Load dataset and models"""
    root = get_project_root()
    
    # Load dataset
    csv_path = os.path.join(root, settings.csv_file_path)
    df = load_data(csv_path)
    
    # Load ML models
    brains_dir = os.path.join(root, 'model', 'brains')
    try:
        model = joblib.load(os.path.join(brains_dir, 'price_model.pkl'))
        le_loc = joblib.load(os.path.join(brains_dir, 'location_encoder.pkl'))
        le_type = joblib.load(os.path.join(brains_dir, 'type_encoder.pkl'))
        le_pay = joblib.load(os.path.join(brains_dir, 'payment_encoder.pkl'))
    except FileNotFoundError:
        model, le_loc, le_type, le_pay = None, None, None, None
    
    return df, model, le_loc, le_type, le_pay


def get_llm():
    """Get LLM for parsing"""
    llm = ChatOpenAI(
        base_url=settings.lm_studio_url,
        api_key="lm-studio",
        model=settings.lm_studio_model,
        temperature=0.3,
    )
    return llm.with_structured_output(RealEstateQuery)


# Load on startup
df, model, le_loc, le_type, le_pay = load_resources()
structured_llm = get_llm()


# --- Helper Functions ---
def get_session(session_id: str) -> dict:
    """Get or create session"""
    if session_id not in sessions:
        sessions[session_id] = {
            "filters": {},
            "skipped_attributes": set(),
            "last_asked_attribute": None
        }
    return sessions[session_id]


def df_to_dict_list(dataframe: pd.DataFrame, limit: int = 20) -> list[dict]:
    """Convert DataFrame to list of dicts for JSON response"""
    return dataframe.head(limit).to_dict(orient="records")


# --- Short Response Enhancement (from app_test.py) ---
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


# --- Intent Classification ---
PROPERTY_KEYWORDS = [
    # Property types
    "شقة", "شقه", "فيلا", "عقار", "بيت", "منزل", "استوديو", "دوبلكس", "تاون", "توين",
    "بنتهاوس", "شاليه", "روف", "دور",
    # Features
    "غرفة", "غرف", "حمام", "حمامات", "مساحة", "متر", "ريسبشن",
    # Price
    "سعر", "ميزانية", "جنيه", "مليون", "الف", "ألف",
    # Locations - with spelling variations
    "التجمع", "اكتوبر", "أكتوبر", "الشيخ زايد", "زايد", "المعادي", "مدينة نصر",
    "العاصمة", "الساحل", "العين السخنة",
    "القاهرة", "القاهره", "قاهرة", "قاهره",  # Cairo variations
    "الجيزة", "الجيزه", "جيزة", "جيزه",  # Giza variations
    "اسكندرية", "اسكندريه", "الاسكندرية",  # Alexandria
    "مصر الجديدة", "الرحاب", "مدينتي",
    # Actions
    "كومبوند", "شراء", "اشتري", "دور", "عايز", "محتاج", "ابحث", "بدور",
    # Payment
    "كاش", "قسط", "تقسيط", "دفع"
]


def classify_intent(text: str, session: dict) -> str:
    """
    Classify if user wants property search or general chat.
    Returns: 'property_search' or 'general_chat'
    """
    # If session already has filters, user is in property search mode
    if session.get("filters") and any(v for v in session["filters"].values()):
        return "property_search"
    
    # Check for property keywords
    for keyword in PROPERTY_KEYWORDS:
        if keyword in text:
            return "property_search"
    
    return "general_chat"


def get_chat_response(text: str) -> str:
    """Get normal conversational response from LLM"""
    from langchain_openai import ChatOpenAI
    from langchain_core.messages import SystemMessage, HumanMessage
    
    llm = ChatOpenAI(
        base_url=settings.lm_studio_url,
        api_key="lm-studio",
        model=settings.lm_studio_model,
        temperature=0.7,
    )
    
    system_prompt = """أنت مساعد عقارات مصري اسمك Baytology. 
أنت متخصص في مساعدة الناس للعثور على العقارات المناسبة في مصر.

إذا سألك أحد عن شيء غير متعلق بالعقارات، رد بأدب واشرح له كيف يمكنك مساعدته في البحث عن عقارات.

مثال:
- إذا قال "مرحبا" أو "هاي": رد بـ "أهلاً! أنا Baytology، مساعدك في البحث عن العقارات في مصر. قولي بتدور على إيه؟ شقة؟ فيلا؟"
- إذا سأل عن خدماتك: اشرح له أنك تساعد في البحث عن شقق وفيلات في مصر

كن ودوداً ومختصراً. رد بالعربية المصرية."""

    messages = [
        SystemMessage(content=system_prompt),
        HumanMessage(content=text)
    ]
    
    response = llm.invoke(messages)
    return response.content


# --- Endpoints ---

@app.get("/")
def root():
    return {"status": "ok", "message": "Baytology API is running"}


@app.post("/parse", response_model=ParseResponse)
def parse_text(request: ParseRequest):
    """Parse Arabic text to structured filters"""
    try:
        result = parse_user_query(structured_llm, request.text)
        
        if result is None:
            result = {}
        
        # Filter out None values
        filters = {k: v for k, v in result.items() if v is not None}
        
        # Update session
        session = get_session(request.session_id)
        session["filters"].update(filters)
        
        return ParseResponse(filters=filters)
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/question", response_model=QuestionResponse)
def generate_question(request: QuestionRequest):
    """Generate entropy-based follow-up question"""
    try:
        # Get matching properties
        matches = filter_properties(df, request.current_filters)
        
        # Generate question
        question, attribute = ask_user_question_Based_on_entropy(
            matches,
            request.current_filters,
            set(request.skipped_attributes)
        )
        
        return QuestionResponse(
            question=question,
            attribute=attribute,
            has_question=attribute is not None
        )
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/rank", response_model=RankResponse)
def rank_properties(request: RankRequest):
    """Rank properties by ML deal score"""
    try:
        if model is None:
            # If no model, return as-is
            return RankResponse(ranked=request.properties)
        
        # Convert to DataFrame
        properties_df = pd.DataFrame(request.properties)
        
        # Rank
        ranked_df = score_and_rank(properties_df, model, le_loc, le_type, le_pay)
        
        return RankResponse(ranked=df_to_dict_list(ranked_df))
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/search", response_model=SearchResponse)
def search_properties(request: SearchRequest):
    """Filter properties from database"""
    try:
        matches = filter_properties(df, request.filters)
        
        return SearchResponse(
            count=len(matches),
            properties=df_to_dict_list(matches, limit=settings.max_results)
        )
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/chat", response_model=ChatResponse)
def chat(request: ChatRequest):
    """
    Full chat endpoint - handles the complete conversation flow.
    This is the main endpoint for frontend integration.
    """
    try:
        session = get_session(request.session_id)
        
        # --- Step 1: Classify Intent ---
        intent = classify_intent(request.message, session)
        
        if intent == "general_chat":
            # Normal chat response
            chat_response = get_chat_response(request.message)
            return ChatResponse(
                type="chat",
                message=chat_response,
                properties_count=0
            )
        
        # --- Step 2: Property Search Flow ---
        
        # Check for skip response
        NEGATIVE_RESPONSES = [
            "لا", "لأ", "مش مهم", "مش فارق", "اي حاجة", "اي حاجه", "خلاص", "عادي",
            "مفيش فرق", "كده تمام", "مش عايز", "مش محتاج", "لا شكرا",
            "no", "skip", "any", "doesn't matter", "don't care"
        ]
        is_skip = any(neg in request.message for neg in NEGATIVE_RESPONSES)
        
        if is_skip and session["last_asked_attribute"]:
            session["skipped_attributes"].add(session["last_asked_attribute"])
            session["last_asked_attribute"] = None
            print(f"[DEBUG] Skipped attribute: {session['skipped_attributes']}")
        else:
            # Enhance short responses with context
            parse_text = request.message
            if session["last_asked_attribute"] and is_short_response(request.message):
                parse_text = enhance_short_response(request.message, session["last_asked_attribute"])
                print(f"[DEBUG] Enhanced text: '{parse_text}'")
            
            # Parse the message
            result = parse_user_query(structured_llm, parse_text)
            print(f"[DEBUG] Parse result: {result}")
            if result:
                filters = {k: v for k, v in result.items() if v is not None}
                session["filters"].update(filters)
        
        print(f"[DEBUG] Session filters: {session['filters']}")
        
        # Search properties
        matches = filter_properties(df, session["filters"])
        count = len(matches)
        print(f"[DEBUG] Match count: {count}")
        
        # --- CASE 1: No Results ---
        if count == 0:
            # Try fallback with location only
            location_keys = ['governorate', 'city', 'district', 'compound']
            fallback_filters = {k: v for k, v in session["filters"].items() if k in location_keys and v}
            
            fallback_matches = filter_properties(df, fallback_filters) if fallback_filters else pd.DataFrame()
            
            if not fallback_matches.empty:
                return ChatResponse(
                    type="fallback",
                    message="للأسف مفيش حاجة بالمواصفات دي بالظبط. بس دي أفضل الفرص المتاحة:",
                    properties=df_to_dict_list(fallback_matches, limit=5),
                    properties_count=len(fallback_matches)
                )
            else:
                return ChatResponse(
                    type="no_results",
                    message="للأسف مفيش أي عقارات مطابقة حالياً.",
                    properties_count=0
                )
        
        # --- CASE 2: Too Many Results ---
        elif count > settings.max_results:
            question, attribute = ask_user_question_Based_on_entropy(
                matches,
                session["filters"],
                session["skipped_attributes"]
            )
            
            session["last_asked_attribute"] = attribute
            
            if attribute is None:
                # No more questions, show results
                if model:
                    ranked = score_and_rank(matches, model, le_loc, le_type, le_pay)
                else:
                    ranked = matches
                
                return ChatResponse(
                    type="results",
                    message=f"تمام! دي أفضل {min(count, settings.max_results)} وحدة ليك:",
                    properties=df_to_dict_list(ranked),
                    properties_count=count
                )
            
            return ChatResponse(
                type="question",
                message=f"أنا لقيت {count} وحدة مناسبة.",
                question=question,
                attribute=attribute,
                properties_count=count
            )
        
        # --- CASE 3: Good Results ---
        else:
            if model:
                ranked = score_and_rank(matches, model, le_loc, le_type, le_pay)
            else:
                ranked = matches
            
            return ChatResponse(
                type="results",
                message=f"تمام! لقيت {count} وحدات ممتازة ليك:",
                properties=df_to_dict_list(ranked),
                properties_count=count
            )
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.delete("/session/{session_id}")
def clear_session(session_id: str):
    """Clear session data"""
    if session_id in sessions:
        del sessions[session_id]
    return {"status": "ok", "message": "Session cleared"}


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
