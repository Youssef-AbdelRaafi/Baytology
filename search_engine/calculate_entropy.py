from scipy.stats import entropy


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

def calculate_all_columns_entropy(data_frame):
    entropy_results = {}
    for col in data_frame.columns:
        entropy_results[col] = calculate_column_entropy(data_frame[col])

    # max() finds the single highest item based on the value (item[1])
    highest_col, highest_val = max(entropy_results.items(), key=lambda item: item[1])

    # Returns a tuple: ('ColumnName', Score)
    return highest_col, highest_val