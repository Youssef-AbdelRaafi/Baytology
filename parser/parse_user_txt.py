# 4. THE PARSER FUNCTION

def parse_user_query(structured_llm, text: str):
    """
    Takes natural text and returns a Python Dictionary of filters.
    """
    try:
        # The LLM predicts the object
        result = structured_llm.invoke(text)
        # Convert Pydantic object to a standard Dictionary
        return result.dict()
    except Exception as e:
        print(f"Error parsing query: {e}")
        return {}