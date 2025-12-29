import pandas as pd
from dotenv import load_dotenv
import os
from pycaret.regression import *



# 1. Load the .env file immediately
load_dotenv()

# 1. Load Data
csv_path = os.getenv("CSV_FILE_PATH")

# Now load
data = pd.read_csv(csv_path)  


# import pycaret regression and init setup
s = setup(data, target = 'charges', session_id = 123)

