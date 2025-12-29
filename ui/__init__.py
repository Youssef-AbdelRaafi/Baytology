"""
Shared UI Components for Baytology.

Contains reusable UI rendering functions for Streamlit applications.
"""


def render_property_card(row: dict, score: float = 0) -> str:
    """
    Generate HTML for a property card.
    
    Args:
        row: Dictionary containing property data (from DataFrame row)
        score: Deal score (positive = good deal, negative = overpriced)
    
    Returns:
        HTML string for the property card
    """
    status_color = "green" if score > 0 else "orange"
    status_text = "صفقة ممتازة (Undervalued)" if score > 0 else "سعر عادل (Fair Price)"
    
    return f"""
    <div style="padding:15px; border-radius:10px; border:1px solid #ddd; margin-bottom:10px;">
        <h3 style="margin:0;">🏠 {row['type']} - {row.get('compound', '')} ({row['location']})</h3>
        <p style="font-size:18px; font-weight:bold; color:#0066cc;">
            💰 {row['price']:,.0f} EGP 
        </p>
        <p>🛏 {row['bedrooms']} Beds | 🛁 {row['bathrooms']} Baths | 📏 {row['size_sqm']} m²</p>
        <p>💳 {row['payment_method']}</p>
        <hr>
        <p style="color:{status_color}; font-weight:bold;">✨ {status_text} (Score: {score:.0f})</p>
    </div>
    """


def display_houses(st, dataframe, model=None, le_location=None, le_type=None, le_payment=None, limit=5):
    """
    Render property cards in Streamlit.
    
    Args:
        st: Streamlit module
        dataframe: DataFrame containing property listings
        model: Optional price prediction model
        le_location, le_type, le_payment: Optional label encoders
        limit: Maximum number of properties to display
    """
    # Import here to avoid circular dependency
    from model.helper_functions import score_and_rank
    
    # Apply ranking if model is available
    if 'deal_score' not in dataframe.columns and model is not None:
        try:
            dataframe = score_and_rank(dataframe, le_location, le_type, le_payment, model)
        except Exception:
            pass
    
    for _, row in dataframe.head(limit).iterrows():
        score = row.get('deal_score', 0)
        card_html = render_property_card(row, score)
        st.markdown(card_html, unsafe_allow_html=True)
