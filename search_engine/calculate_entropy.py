from scipy.stats import entropy
from parser.data_validation import RealEstateQuery
import os
import google.generativeai as genai
from pydantic import BaseModel, Field
from typing import Optional, Literal, Dict
from dotenv import load_dotenv  

import os
from typing import Optional, Literal
from pydantic import BaseModel, Field
from langchain_google_genai import ChatGoogleGenerativeAI
from langchain_core.prompts import PromptTemplate
from langchain_core.output_parsers import StrOutputParser



# Setup Gemini API (Ensure you set your API key in environment variables)
# api_key = os.environ["GOOGLE_API_KEY"]
# Setup API Key
api_key = os.getenv("GOOGLE_API_KEY")


def calculate_column_entropy(column, base=2):
    # Get the counts of unique values in the column
    value_counts = column.value_counts()
    # Pass the counts to scipy.stats.entropy
    # The function normalizes the counts into probabilities internally
    return entropy(value_counts, base=base)


# def calculate_all_columns_entropy(data_frame):
#     entropy_results = {}
#     for col in data_frame.columns:
#         entropy_results[col] = calculate_column_entropy(data_frame[col])

#     sorted_items = sorted(entropy_results.items(), key=lambda item: item[1])
#     # Convert the sorted list of tuples back into a dictionary
#     sorted_data = dict(sorted_items)

#     return sorted_data

# def calculate_all_columns_entropy(data_frame):
#     # 1. Calculate entropy for all columns in one line
#     entropy_results = {
#         col: calculate_column_entropy(data_frame[col]) for col in data_frame.columns
#         }

#     # 2. Sort the dictionary by value (item[1])
#     # key=lambda item: item[1]  -> Sorts by the Value
#     # reverse=True              -> Sorts High to Low (Optional: remove for Low to High)
#     sorted_data = dict(sorted(entropy_results.items(), key=lambda item: item[1], reverse=True))

#     return sorted_data[0]

"""
def calculate_all_columns_entropy(data_frame):
    entropy_results = {}
    for col in data_frame.columns:
        entropy_results[col] = calculate_column_entropy(data_frame[col])

    # max() finds the single highest item based on the value (item[1])
    highest_col, highest_val = max(entropy_results.items(), key=lambda item: item[1])

    # Returns a tuple: ('ColumnName', Score)
    return {highest_col: highest_val}
"""

# def ask_user_question_Based_on_entropy(data_frame):
#     attribute_entropy_dic = calculate_all_columns_entropy(data_frame)

#     for key, value in attribute_entropy_dic.items():
#         if key == 'size_sqm':
#             message = 'what the home size do you want'
#         return message



# i want to edit this function to use langchain instead of google lib and i want
# to edit the validation class to have all data attributes
# def ask_user_question_Based_on_entropy(data_frame):
#     """
#     Identifies the attribute with the highest entropy (uncertainty) 
#     and uses Gemini to generate a natural language question based on 
#     the Pydantic model description.
#     """
    
#     # 1. Calculate entropy for all columns
#     attribute_entropy_dic = calculate_all_columns_entropy(data_frame)

#     if not attribute_entropy_dic:
#         return "I found some properties. Would you like to see them?"

#     # 2. Find the key with the highest entropy (Maximum uncertainty)
#     # We want to ask about the feature that splits the data the most effectively.
#     # target_key = max(attribute_entropy_dic, key=attribute_entropy_dic.get)

#     # 3. Retrieve the description from the Pydantic Model
#     field_description = "General preference"
    
#     # Check if the key exists in the Pydantic model to get the specific description
#     for key, value in attribute_entropy_dic.items():
#         if key in RealEstateQuery.model_fields:
#             field_description = RealEstateQuery.model_fields[key].description
    
#     # 4. Call Gemini to generate the question
#     try:
#         model = genai.GenerativeModel('gemini-2.5-flash')
        
#         prompt = (
#             f"You are a real estate assistant helping a user find a home. "
#             f"I need to narrow down the search based on the attribute: '{key}'.\n"
#             f"Attribute Description: {field_description}\n\n"
#             f"Task: Generate a short, friendly, natural language question asking the user "
#             f"for their preference regarding {key}. Do not mention technical terms like 'entropy' or 'database'."
#         )
        
#         response = model.generate_content(prompt)
#         return response.text.strip()
        
#     except Exception as e:
#         # Fallback if Gemini fails
#         return f"Could you please specify your preference for {key}?"


# --- The Updated Function using LangChain ---
# def ask_user_question_Based_on_entropy(data_frame):
#     """
#     Identifies the attribute with the highest entropy and uses 
#     LangChain + Gemini to generate a natural language question.
#     """
    
#     # 1. Calculate entropy for all columns
#     attribute_entropy_dic = calculate_all_columns_entropy(data_frame)

#     if not attribute_entropy_dic:
#         return "I found some properties. Would you like to see them?"

#     # 2. Find the key with the highest entropy (Maximum uncertainty)
#     # We want to ask about the feature that splits the data the most effectively.
#     # target_key = max(attribute_entropy_dic, key=attribute_entropy_dic.get)

#     # 3. Retrieve the description from the Pydantic Model
#     field_description = "General preference"
    
#     # Check if the key exists in the Pydantic model to get the specific description
#     for key, value in attribute_entropy_dic.items():
#         if key in RealEstateQuery.model_fields:
#             field_description = RealEstateQuery.model_fields[key].description


#     # 4. Call LangChain to generate the question
#     try:
#         # Initialize the LLM
#         llm = ChatGoogleGenerativeAI(
#             model="gemini-2.5-flash",
#             google_api_key=api_key,
#             temperature=0.7
#         )

#         # Create a Prompt Template
#         template = """You are a helpful real estate assistant.
#         I need to ask the user a question to narrow down their search regarding the attribute: '{attribute_name}'.
        
#         Attribute Context: {attribute_description}
        
#         Task: Write a single, friendly, short question asking the user for their preference on this topic.
#         Do not explain why you are asking. Just ask the question naturally.
#         """
        
#         prompt = PromptTemplate.from_template(template)

#         # Create the chain: Prompt -> LLM -> String Parser
#         chain = prompt | llm | StrOutputParser()

#         # Invoke the chain
#         question = chain.invoke({
#             "attribute_name": key,
#             "attribute_description": field_description
#         })
        
#         return question.strip()

#     except Exception as e:
#         print(f"LangChain Error: {e}")
#         return f"Could you please tell me your preference for {key}?"


'''
def ask_user_question_Based_on_entropy(data_frame):
    """
    Identifies the attribute with the highest entropy and uses 
    LangChain + Gemini to generate a natural language question.
    """
    
    # 1. Calculate entropy for all columns
    attribute_entropy_dic = calculate_all_columns_entropy(data_frame)

    if not attribute_entropy_dic:
        return "I found some properties. Would you like to see them?"

    # 2. FIXED: Uncommented this to find the attribute with Maximum uncertainty
    target_key = max(attribute_entropy_dic, key=attribute_entropy_dic.get)

    # 3. Retrieve the description from the Pydantic Model
    field_description = "General preference"
    
    # FIXED: Check specifically for the target_key, not a loop over everything
    if target_key in RealEstateQuery.model_fields:
        field_description = RealEstateQuery.model_fields[target_key].description

    # 4. Call LangChain to generate the question
    try:
        # Initialize the LLM
        llm = ChatGoogleGenerativeAI(
            model="gemini-1.5-flash", # FIXED: Changed 2.5 to 1.5
            google_api_key=api_key,
            temperature=0.7
        )

        template = """You are a helpful real estate assistant.
        I need to ask the user a question to narrow down their search regarding the attribute: '{attribute_name}'.
        
        Attribute Context: {attribute_description}
        
        Task: Write a single, friendly, short question asking the user for their preference on this topic.
        Do not explain why you are asking. Just ask the question naturally.
        """
        
        prompt = PromptTemplate.from_template(template)
        chain = prompt | llm | StrOutputParser()

        # Invoke the chain using target_key
        question = chain.invoke({
            "attribute_name": target_key, # FIXED: Uses the max entropy key
            "attribute_description": field_description
        })
        
        return question.strip()

    except Exception as e:
        print(f"LangChain Error: {e}")
        return f"Could you please tell me your preference for {target_key}?"
'''



def calculate_all_columns_entropy(data_frame):
    entropy_results = {}
    for col in data_frame.columns:
        # Calculate and store everyone
        entropy_results[col] = calculate_column_entropy(data_frame[col])
    
    # Return everything! Do not pick the max here.
    return entropy_results




def ask_user_question_Based_on_entropy(data_frame, current_filters):
    """
    1. Calculates entropy.
    2. Checks which attributes are already answered in 'current_filters'.
    3. Asks about the highest entropy attribute that is NOT yet answered.
    """
    
    api_key = os.getenv("GOOGLE_API_KEY")
    if not api_key: return "System Error: API Key missing."

    # 1. Calculate entropy for ALL columns
    all_entropies = calculate_all_columns_entropy(data_frame)

    # 2. Define Mapping: CSV Column -> Pydantic/Filter Key
    # IMPORTANT: These values must match the keys in your 'filters' JSON exactly!
    column_mapping = {
        # Location
        "governorate": "governorate",
        "city": "city",
        "district": "district",
        "compound": "compound",
        
        # Numbers
        "price": "max_price",
        "size_sqm": "min_size_sqm",
        "bedrooms": "min_bedrooms",
        "bathrooms": "min_bathrooms",
        
        # Categorical
        "type": "property_type",
        "payment_method": "payment_method",
        "mid_room": "mid_room"
    }

    valid_entropies = {}

    # 3. Filter Logic
    for csv_col, entropy_val in all_entropies.items():
        # Get the key used in the filters JSON
        filter_key = column_mapping.get(csv_col, csv_col)
        
        # CHECK 1: Is this field in our Pydantic Model?
        if filter_key not in RealEstateQuery.model_fields:
            continue

        # CHECK 2 (NEW): Has the user already provided this info?
        # We look at the value in current_filters. 
        # If it is NOT None, we skip it (because we already know the answer).
        user_value = current_filters.get(filter_key)
        
        if user_value is not None:
            # User already answered this (e.g., 'Cairo'), so don't ask again.
            continue
            
        # If we get here, the value is None, so it's a valid question candidate
        valid_entropies[filter_key] = entropy_val

    # If everything is filled out (valid_entropies is empty)
    if not valid_entropies:
        return "تمام، أنا عندي كل التفاصيل اللي محتاجها. تحب أعرضلك النتائج؟"

    # 4. Find the key with the highest entropy among the UNANSWERED ones
    target_key = max(valid_entropies, key=valid_entropies.get)

    # 5. Retrieve description
    field_description = RealEstateQuery.model_fields[target_key].description

    # 6. Generate Question
    try:
        llm = ChatGoogleGenerativeAI(
            model="gemini-2.5-flash", 
            google_api_key=api_key, 
            temperature=0.7
        )

        template = """You are a friendly Egyptian real estate assistant (Semsar).
        The user has NOT specified their preference for: '{attribute_name}'.
        
        Context: {attribute_description}
        
        Task: Ask a short, natural question in **Egyptian Arabic** to get this information.
        If asking about budget/price, use 'Mizaniya'and if user ask you about.
        """
        
        prompt = PromptTemplate.from_template(template)
        chain = prompt | llm | StrOutputParser()

        question = chain.invoke({
            "attribute_name": target_key,
            "attribute_description": field_description
        })
        
        return question.strip(), valid_entropies

    except Exception as e:
        print(f"Error: {e}")
        return  f"ممكن تقولي تفضيلاتك بخصوص {target_key}؟"