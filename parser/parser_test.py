import os
from typing import Optional, Literal
from pydantic import BaseModel, Field
from langchain_google_genai import ChatGoogleGenerativeAI
from data_validation import RealEstateQuery
from parse_user_txt import parse_user_query

# 1. SETUP API KEY
os.environ["GOOGLE_API_KEY"] = "AIzaSyAg1iQn7OCCGnECwI0JCXkvm3uar2mridM"



# 3. CONFIGURE GEMINI
llm = ChatGoogleGenerativeAI(
    model="gemini-2.5-flash",
    temperature=0  # 0 means "be precise, don't be creative"
)

# This binds the Pydantic class to the model
structured_llm = llm.with_structured_output(RealEstateQuery)



if __name__ == "__main__":
    print("--- Testing Gemini Parser ---\n")
    
    # # Example 1: Clear request
    # user_text_1 = "عايز شقة 3 غرف في المعادي في حدود 5 مليون."
    # print(f"User: {user_text_1}")
    # print(f"Extracted as json object: {parse_user_query(structured_llm=structured_llm,text=user_text_1)}")
    # print("-" * 30)

    # # Example 2: Tricky request (Arabic/English mix often works too)
    # user_text_2 = "بدور على فيلا في التجمع مساحتها كبيرة، حوالي 300 متر"
    # print(f"User: {user_text_2}")
    # print(f"Extracted: {parse_user_query(structured_llm=structured_llm,text=user_text_2)}")


    # Test 1: iVilla in October (Specific Type Check)
    t1 = "عايز فيلا في اكتوبر تبع ماونتن فيو تكون 4 غرف و 3 حمامات"
    print(f"Input: {t1}")
    print(f"Result: {parse_user_query(structured_llm=structured_llm,text=t1)}")
    # Expected: property_type='iVilla', location='Mountain View', min_bedrooms=4...

    # Test 2: Madinaty (Location Check)
    t2 = "شقة في مدينتي حدود 5 مليون"
    print(f"\nInput: {t2}")
    print(f"Result: {parse_user_query(structured_llm=structured_llm,text=t2)}")
    # Expected: location='Madinaty', property_type='Apartment'...