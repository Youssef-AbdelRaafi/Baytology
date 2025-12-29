"""
Centralized Configuration Management for Baytology.

Uses Pydantic Settings to load environment variables from .env file
with type validation and default values.
"""
from pydantic_settings import BaseSettings
from functools import lru_cache
import os


class Settings(BaseSettings):
    """Application settings loaded from environment variables."""
    
    # API Keys
    google_api_key: str = ""
    
    # File Paths
    csv_file_path: str = "egypt_real_estate_preprocessed.csv"
    
    # LLM Configuration  
    lm_studio_url: str = "http://localhost:1234/v1"
    lm_studio_model: str = "gemma-2-9b-it"
    
    # App Settings
    max_results: int = 20
    
    class Config:
        env_file = ".env"
        env_file_encoding = "utf-8"
        extra = "ignore"  # Ignore extra env vars not defined here


@lru_cache()
def get_settings() -> Settings:
    """Get cached settings instance."""
    return Settings()


# Singleton instance for easy import
settings = get_settings()
