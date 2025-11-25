from typing import Optional, Literal
from pydantic import BaseModel, Field


# DEFINE THE "BLUEPRINT" (The Schema)
class RealEstateQuery(BaseModel):
    """
    Extracts user preferences. 
    Matches specific dataset values:
    Types: ['Chalet', 'Villa', 'Apartment', 'Penthouse', 'Twin House', 'Duplex', 'iVilla', 
            'Townhouse', 'Hotel Apartment', 'Cabin', 'Palace', 'Bungalow', etc.]
    """
    
    # Location: Your DB has long strings (e.g., 'O West, 6 October...').
    # We ask the LLM to extract the *Key Name* (Compound or City) so we can use partial matching later.
    location: Optional[str] = Field(
        None, 
        description="The city or compound name in English. Translate Arabic inputs. "
                    "Examples: 'مدينتي' -> 'Madinaty', 'اكتوبر' -> '6 October', 'التجمع' -> 'New Cairo', "
                    "'الساحل' -> 'North Coast', 'العاصمة' -> 'Capital', 'مراسي' -> 'Marassi'. "
                    "If user specifies a compound like 'O West' or 'Hyde Park', output that name."
    )
    
    # Type: Updated with YOUR specific unique values
    property_type: Optional[Literal[
        'Apartment', 'Villa', 'Chalet', 'Penthouse', 'Twin House', 'Duplex', 
        'iVilla', 'Townhouse', 'Hotel Apartment', 'Cabin', 'Bulk Sale Unit', 
        'Palace', 'Land', 'Whole Building', 'Full Floor', 'Roof', 'Bungalow'
    ]] = Field(
        None, 
        description="Map Arabic to these exact English types: "
                    "'شقة'->'Apartment', 'فيلا'->'Villa', 'توي هاوس'->'Twin House', "
                    "'اي فيلا'->'iVilla', 'شاليه'->'Chalet', 'روف'->'Roof'."
    )
    
    # Bedrooms (Range 1-7 in your data)
    min_bedrooms: Optional[int] = Field(
        None, 
        description="Minimum bedrooms. Map 'اوضتين'->2, '3 غرف'->3."
    )
    
    # Bathrooms (Range 1-7 in your data)
    min_bathrooms: Optional[int] = Field(
        None, 
        description="Minimum bathrooms. Map 'حمامين'->2."
    )
    
    # Price (Keep as float/int)
    max_price: Optional[float] = Field(
        None, 
        description="Max budget in EGP. '2 مليون' -> 2000000."
    )

    # Payment (Assuming 'Cash' or 'Installments' exist in your full data)
    payment_method: Optional[Literal["Cash", "Installments"]] = Field(
        None, 
        description="Payment method: 'كاش'->'Cash', 'قسط'->'Installments'."
    )