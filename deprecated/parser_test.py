import os
from dotenv import load_dotenv
from langchain_google_genai import ChatGoogleGenerativeAI
from data_validation import RealEstateQuery
from parse_user_txt import parse_user_query

# Load API key from .env file (NEVER hardcode keys!)
load_dotenv()


# Configure Gemini LLM
llm = ChatGoogleGenerativeAI(
    model="gemini-2.5-flash",
    temperature=0  # 0 means "be precise, don't be creative"
)

# Bind the Pydantic schema for structured output
structured_llm = llm.with_structured_output(RealEstateQuery)


if __name__ == "__main__":
    print("--- Testing Gemini Parser ---\n")
    
    # Test 1: Villa in October with specific requirements
    t1 = "عايز فيلا في اكتوبر تبع ماونتن فيو تكون 4 غرف و 3 حمامات"
    print(f"Input: {t1}")
    print(f"Result: {parse_user_query(structured_llm=structured_llm, text=t1)}")

    # Test 2: Apartment in Madinaty with budget
    t2 = "شقة في مدينتي حدود 5 مليون"
    print(f"\nInput: {t2}")
    print(f"Result: {parse_user_query(structured_llm=structured_llm, text=t2)}")