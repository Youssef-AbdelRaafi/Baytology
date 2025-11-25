from pydantic import BaseModel

class house(BaseModel):
    price:  int
    city: str

my_home = house(price= 3, city="Cairo")