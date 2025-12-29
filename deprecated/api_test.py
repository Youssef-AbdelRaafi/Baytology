from fastapi import FastAPI, Query
import pandas as pd
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




api = FastAPI()

@api.get('/search')
async def search(
        user_text: str = Query(..., description="ادخل مواصفات العقار الذي تبحث عنه"),
                ):
    load_dotenv()
    os.chdir(os.getcwd())
    os.getenv("GOOGLE_API_KEY")
    csv_path = os.getenv("CSV_FILE_PATH")
    df = load_data(csv_path)  

    llm = ChatGoogleGenerativeAI(
    model="gemini-2.5-flash",
    temperature=0  # 0 means "be precise, don't be creative"
    )

    structured_llm = llm.with_structured_output(RealEstateQuery)
    filters = parse_user_query(structured_llm=structured_llm,text=user_text)
    matches = filter_properties(df, filters)

    return {"matches": len(matches)}








    

