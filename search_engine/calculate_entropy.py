"""
Entropy-based Question Generator for Real Estate Chatbot.

This module calculates information entropy to identify which attribute
has the most uncertainty, then generates a natural language question
to narrow down search results.
"""
from scipy.stats import entropy
from parser.data_validation import RealEstateQuery
from config import settings
from langchain_core.prompts import PromptTemplate
from langchain_core.output_parsers import StrOutputParser
from langchain_openai import ChatOpenAI


def calculate_column_entropy(column, base=2):
    """Calculate Shannon entropy for a pandas column."""
    value_counts = column.value_counts()
    return entropy(value_counts, base=base)



# 1. ENTROPY CALCULATOR (Returns All Scores)
# ============================================================
def calculate_all_columns_entropy(data_frame):
    """
    Calculates entropy for every column in the dataframe.
    Returns a dictionary: {'col_name': entropy_score}
    """
    entropy_results = {}
    for col in data_frame.columns:
        # We assume calculate_column_entropy is defined elsewhere in your code
        entropy_results[col] = calculate_column_entropy(data_frame[col])
    
    return entropy_results

# ============================================================
# 2. THE SMART QUESTION GENERATOR (V2 Compatible)
# ============================================================
def ask_user_question_Based_on_entropy(data_frame, current_filters, skipped_attributes=None):
    """
    1. Calculates entropy for all columns.
    2. Maps CSV columns to Pydantic V2 Model fields.
    3. Checks current_filters to see what is already answered.
    4. Skips attributes in skipped_attributes set.
    5. Asks about the highest entropy attribute that is missing.
    
    Returns: tuple (question_text, attribute_name) so caller can track what was asked
    """
    if skipped_attributes is None:
        skipped_attributes = set()

    # 1. Calculate entropy for ALL columns
    all_entropies = calculate_all_columns_entropy(data_frame)

    # 2. Define Mapping: "Database Column Name" -> "Pydantic Field Name"
    # This aligns perfectly with your RealEstateQuery V2 Class
    column_mapping = {
        # Location Hierarchy
        "governorate": "governorate",
        "city": "city",
        "district": "district",
        "compound": "compound",
        
        # Numbers (Constraint Logic)
        "price": "max_price",
        "size_sqm": "min_size_sqm",
        "bedrooms": "min_bedrooms",
        "bathrooms": "min_bathrooms",
        
        # Categorical / Boolean
        "type": "property_type",
        "payment_method": "payment_method",
        "mid_room": "mid_room"
    }

    valid_entropies = {}

    # 3. Filter Logic
    for csv_col, entropy_val in all_entropies.items():
        # Get the key used in the filters JSON (Pydantic Field Name)
        filter_key = column_mapping.get(csv_col)
        
        # SKIP if the column is not in our mapping (e.g., 'id', 'url')
        if not filter_key:
            continue
            
        # SKIP if this field is not in the Pydantic Model
        if filter_key not in RealEstateQuery.model_fields:
            continue

        # CHECK: Has the user already provided this info?
        user_value = current_filters.get(filter_key)
        
        # If user_value is NOT None, they already answered it. Skip.
        if user_value is not None:
            continue
        
        # SKIP if user said "no" / "don't care" about this attribute
        if filter_key in skipped_attributes:
            continue
            
        # If we get here, it's a valid candidate for a question
        valid_entropies[filter_key] = entropy_val

    # If no valid attributes remain (User answered everything)
    if not valid_entropies:
        return ("تمام، أنا عندي كل التفاصيل اللي محتاجها. تحب أعرضلك النتائج؟", None)

    # 4. Find the key with the highest entropy among the UNANSWERED ones
    target_key = max(valid_entropies, key=valid_entropies.get)

    # 5. Retrieve the description from Pydantic V2 to give context to Gemini
    field_description = RealEstateQuery.model_fields[target_key].description

    # 6. Generate Question using LLM
    try:
        llm = ChatOpenAI(
            base_url=settings.lm_studio_url,
            api_key="lm-studio",
            model=settings.lm_studio_model,
            temperature=0.1,
        )

        template = """You are a helpful Egyptian real estate assistant.

The user has NOT specified: '{attribute_name}'.
Field context: {attribute_description}

IMPORTANT: Ask ONLY ONE short question about this specific attribute. Do NOT ask about anything else.

Write your question in Egyptian Arabic (Ammiya). Keep it under 15 words.

Examples of good single questions:
- For budget: "ميزانيتك كام تقريباً؟"
- For compound: "في كومبوند معين تفضله؟"
- For payment: "كاش ولا قسط؟"
- For bedrooms: "عايز كام أوضة؟"

Your single question about {attribute_name}:"""
        
        prompt = PromptTemplate.from_template(template)
        chain = prompt | llm | StrOutputParser()

        question = chain.invoke({
            "attribute_name": target_key,
            "attribute_description": field_description
        })
        
        return (question.strip(), target_key)

    except Exception as e:
        print(f"LangChain Error: {e}")
        return (f"ممكن تقولي تفضيلاتك بخصوص {target_key}؟", target_key)