"""
Parser module for extracting structured real estate queries from Arabic text.
"""
from langchain_core.messages import SystemMessage, HumanMessage


# System prompt to enforce English output (critical for local models)
ARABIC_TO_ENGLISH_SYSTEM_PROMPT = """You are a real estate query parser. Your job is to extract structured data from Egyptian Arabic text.

CRITICAL RULES:
1. ALL output values MUST be in ENGLISH, never Arabic
2. Translate Arabic location names to English: 'التجمع' → 'New Cairo City', 'اكتوبر' → '6 October City', 'مدينتي' → 'Madinaty', 'العاصمة الإدارية' → 'New Capital City', 'زايد' → 'Sheikh Zayed City', 'الساحل' → 'North Coast'
3. Translate Arabic property types: 'شقة' → 'Apartment', 'فيلا' → 'Villa', 'شاليه' → 'Chalet', 'تاون' → 'Townhouse', 'دوبلكس' → 'Duplex'
4. Convert Arabic numbers to integers: 'مليون' = 1000000, 'ألف' = 1000
5. Payment: 'كاش' → 'Cash', 'قسط/تقسيط' → 'Installments'
6. If information is not provided, leave it as null

Extract the following fields: governorate, city, district, compound, property_type, max_price, min_bedrooms, min_bathrooms, min_size_sqm, payment_method"""


def parse_user_query(structured_llm, text: str):
    """
    Takes natural Arabic text and returns a Python Dictionary of filters.
    Uses a system prompt to ensure English output for local models.
    """
    try:
        # Create message list with system prompt + user text
        messages = [
            SystemMessage(content=ARABIC_TO_ENGLISH_SYSTEM_PROMPT),
            HumanMessage(content=text)
        ]
        
        # The LLM predicts the structured object
        result = structured_llm.invoke(messages)
        
        # Convert Pydantic object to a standard Dictionary
        return result.dict()
    except Exception as e:
        print(f"Error parsing query: {e}")
        return {}