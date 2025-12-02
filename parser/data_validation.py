from typing import Optional, Literal
from pydantic import BaseModel, Field


# # DEFINE THE "BLUEPRINT" (The Schema)
# class RealEstateQuery(BaseModel):
#     """
#     Extracts user preferences. 
#     Matches specific dataset values:
#     Types: ['Chalet', 'Villa', 'Apartment', 'Penthouse', 'Twin House', 'Duplex', 'iVilla', 
#             'Townhouse', 'Hotel Apartment', 'Cabin', 'Palace', 'Bungalow', etc.]
#     """
    
#     # Location: Your DB has long strings (e.g., 'O West, 6 October...').
#     # We ask the LLM to extract the *Key Name* (Compound or City) so we can use partial matching later.
#     location: Optional[str] = Field(
#         None, 
#         description="The city or compound name in English. Translate Arabic inputs. "
#                     "Examples: 'مدينتي' -> 'Madinaty', 'اكتوبر' -> '6 October', 'التجمع' -> 'New Cairo', "
#                     "'الساحل' -> 'North Coast', 'العاصمة' -> 'Capital', 'مراسي' -> 'Marassi'. "
#                     "If user specifies a compound like 'O West' or 'Hyde Park', output that name."
#     )
    
#     # Type: Updated with YOUR specific unique values
#     property_type: Optional[Literal[
#         'Apartment', 'Villa', 'Chalet', 'Penthouse', 'Twin House', 'Duplex', 
#         'iVilla', 'Townhouse', 'Hotel Apartment', 'Cabin', 'Bulk Sale Unit', 
#         'Palace', 'Land', 'Whole Building', 'Full Floor', 'Roof', 'Bungalow'
#     ]] = Field(
#         None, 
#         description="Map Arabic to these exact English types: "
#                     "'شقة'->'Apartment', 'فيلا'->'Villa', 'توي هاوس'->'Twin House', "
#                     "'اي فيلا'->'iVilla', 'شاليه'->'Chalet', 'روف'->'Roof'."
#     )
    
#     # Bedrooms (Range 1-7 in your data)
#     min_bedrooms: Optional[int] = Field(
#         None, 
#         description="Minimum bedrooms. Map 'اوضتين'->2, '3 غرف'->3."
#     )
    
#     # Bathrooms (Range 1-7 in your data)
#     min_bathrooms: Optional[int] = Field(
#         None, 
#         description="Minimum bathrooms. Map 'حمامين'->2."
#     )
    
#     # Price (Keep as float/int)
#     max_price: Optional[float] = Field(
#         None, 
#         description="Max budget in EGP. '2 مليون' -> 2000000."
#     )

#     # Payment (Assuming 'Cash' or 'Installments' exist in your full data)
#     payment_method: Optional[Literal["Cash", "Installments"]] = Field(
#         None, 
#         description="Payment method: 'كاش'->'Cash', 'قسط'->'Installments'."
#     )




class RealEstateQuery(BaseModel):
    """
    Extracts structured real estate preferences from Egyptian Arabic user input.
    Maps colloquial terms (Ammiya) to the specific standardized English values in the database.
    """

    # --- LOCATION HIERARCHY ---
    
    governorate: Optional[str] = Field(
        None,
        description="The broadest region. If the user mentions 'Sahel' or 'North Coast', map to 'North Coast'. "
                    "If 'Gouna' or 'Hurghada', map to 'Red Sea'. "
                    "Map Arabic to English: 'القاهرة'->'Cairo', 'الجيزة'->'Giza', 'إسكندرية'->'Alexandria', 'الساحل'->'North Coast'. "
                    "Ignore sub-details like 'New Cairo City, Cairo', just extract the main Governorate name."
    )

    city: Optional[str] = Field(
        None,
        description="The major city. Match these exact database values: 'New Cairo City', '6 October City', 'Sheikh Zayed City', 'Hurghada', 'Al Alamein', 'Shorouk City', 'Obour City', 'Nasr City', 'Maadi', 'Mokattam', 'New Capital City'. "
                    "Mapping Rules: 'التجمع' -> 'New Cairo City', 'العاصمة الإدارية' -> 'New Capital City', 'زايد' -> 'Sheikh Zayed City', 'أكتوبر' -> '6 October City'."
    )

    district: Optional[str] = Field(
        None,
        description="Specific neighborhood or zone. "
                    "Look for New Capital zones: 'R7', 'R8'. "
                    "Look for North Coast bays: 'Ras Al Hekma', 'Sidi Heneish', 'Ras Sedr'. "
                    "Look for Cairo/Giza districts: '5th Settlement', 'Mohandessin', 'Zamalek', 'Smouha'. "
                    "If user says 'The Seventh Residential District' map to 'R7'."
    )

    compound: Optional[str] = Field(
        None,
        description="The specific project or gated community name. "
                    "Extract names like: 'Swan Lake', 'Mountain View', 'Palm Hills', 'Marassi', 'Badya', 'Madinaty', 'Hyde Park', 'Mivida', 'Azha', 'Sodic'. "
                    "If the user mentions a project name in Arabic (e.g., 'مراسي', 'بادية'), transcribe it to the closest English match from the database list."
    )

    # --- PROPERTY DETAILS ---

    property_type: Optional[Literal[
        'Apartment', 'Villa', 'Chalet', 'Penthouse', 'Twin House', 'Duplex', 
        'iVilla', 'Townhouse', 'Hotel Apartment', 'Cabin', 'Bulk Sale Unit', 
        'Palace', 'Land', 'Whole Building', 'Full Floor', 'Roof', 'Bungalow'
    ]] = Field(
        None, 
        description="The unit type. Map Egyptian terms carefully: "
                    "'شقة'->'Apartment', 'فيلا'->'Villa', 'شاليه'->'Chalet', "
                    "'روف'->'Roof', 'تاون'->'Townhouse', 'توين'->'Twin House', "
                    "'دوبلكس'->'Duplex', 'أرض'->'Land', 'اي فيلا'->'iVilla'."
    )

    # --- NUMERICAL CONSTRAINTS ---

    max_price: Optional[float] = Field(
        None, 
        description="The user's maximum budget in EGP. "
                    "Convert text numbers to floats: '2 مليون' -> 2000000, '500 ألف' -> 500000. "
                    "If user says 'in the range of X', treat X as the max price."
    )

    min_bedrooms: Optional[int] = Field(
        None, 
        description="Minimum number of bedrooms. "
                    "Map 'أوضتين'->2, 'ثلاث غرف'->3, 'ستوديو'->0. "
                    "Valid range: 0 to 7."
    )

    min_bathrooms: Optional[int] = Field(
        None, 
        description="Minimum number of bathrooms. "
                    "Map 'حمامين'->2, '3 حمام'->3. "
                    "Valid range: 1 to 7."
    )

    min_size_sqm: Optional[float] = Field(
        None, 
        description="Minimum area size in square meters. "
                    "Extract the number before 'متر' or 'm'. "
                    "Example: 'عايز 150 متر' -> 150.0"
    )
    
    # --- EXTRAS ---

    mid_room: Optional[bool] = Field(
        None,
        description="Maid/Nanny Room (Odet Shaghala). "
                    "Set to True ONLY if the user explicitly asks for: 'غرفة ناني', 'غرفة شغالة', 'غرفة خدمات', 'Maid Room', or 'Nanny Room'."
    )

    payment_method: Optional[Literal["Cash", "Installments"]] = Field(
        None, 
        description="Payment preference. "
                    "Map 'كاش' or 'Cash' -> 'Cash'. "
                    "Map 'قسط', 'تسهيلات', 'مقدم', 'تقسيط', 'Installments' -> 'Installments'."
    )



# # Define Mapping: "Database Column Name" -> "Pydantic Field Name"
#     column_mapping = {
#         # Location
#         "governorate": "governorate",
#         "city": "city",
#         "district": "district",
#         "compound": "compound",
        
#         # Numbers
#         "price": "max_price",
#         "size_sqm": "min_size_sqm",
#         "bedrooms": "min_bedrooms",
#         "bathrooms": "min_bathrooms",
        
#         # Categorical
#         "type": "property_type",
#         "payment_method": "payment_method",
#         "mid_room": "mid_room"
#     }