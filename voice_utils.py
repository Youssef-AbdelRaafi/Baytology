"""
Voice Utilities for Baytology

Handles audio transcription using faster-whisper (100% local, no API needed).
Optimized for Arabic (Modern Standard Arabic + Egyptian dialect).
"""
import tempfile
import os
from faster_whisper import WhisperModel
from config import settings


# --- Global Model (loaded once at import time, reused for all requests) ---
print(f"[VOICE] Loading Whisper model '{settings.whisper_model}' on {settings.whisper_device}...")
_model = WhisperModel(
    settings.whisper_model,
    device=settings.whisper_device,
    compute_type=settings.whisper_compute_type,
)
print(f"[VOICE] ✅ Whisper model loaded successfully.")


def transcribe_audio(audio_bytes: bytes, mime_type: str) -> str:
    """
    Transcribe audio bytes to text using faster-whisper (local).

    Args:
        audio_bytes: Raw audio file bytes
        mime_type: MIME type of the audio (e.g., 'audio/webm', 'audio/wav')

    Returns:
        Transcribed text string

    Raises:
        ValueError: If transcription fails or returns empty
    """
    # Determine file extension from MIME type
    ext = _mime_to_extension(mime_type)

    # Write audio bytes to a temp file (faster-whisper needs a file path)
    with tempfile.NamedTemporaryFile(suffix=ext, delete=False) as tmp:
        tmp.write(audio_bytes)
        tmp_path = tmp.name

    try:
        # Transcribe with Arabic language hint for best accuracy
        segments, info = _model.transcribe(
            tmp_path,
            language="ar",       # Force Arabic for best accuracy
            beam_size=5,         # Higher beam = more accurate
            vad_filter=True,     # Filter out silence/noise
        )

        # Collect all segment texts
        text_parts = []
        for segment in segments:
            text_parts.append(segment.text.strip())

        transcription = " ".join(text_parts).strip()

        if not transcription:
            raise ValueError("Transcription returned empty result — no speech detected")

        print(f"[VOICE] Detected language: {info.language} (prob: {info.language_probability:.2f})")
        return transcription

    finally:
        # Clean up temp file
        if os.path.exists(tmp_path):
            os.unlink(tmp_path)


def _mime_to_extension(mime_type: str) -> str:
    """Map MIME type to file extension."""
    mapping = {
        "audio/webm": ".webm",
        "audio/wav": ".wav",
        "audio/wave": ".wav",
        "audio/x-wav": ".wav",
        "audio/mp3": ".mp3",
        "audio/mpeg": ".mp3",
        "audio/ogg": ".ogg",
        "audio/flac": ".flac",
        "audio/mp4": ".mp4",
        "audio/x-m4a": ".m4a",
    }
    # Handle types with parameters like "audio/webm;codecs=opus"
    base_type = mime_type.lower().split(";")[0].strip()
    return mapping.get(base_type, ".webm")


# Supported MIME types for audio upload
SUPPORTED_AUDIO_TYPES = set(_mime_to_extension.__code__.co_consts) - {None, ""}
# Simpler approach:
SUPPORTED_AUDIO_TYPES = {
    "audio/webm", "audio/wav", "audio/wave", "audio/x-wav",
    "audio/mp3", "audio/mpeg", "audio/ogg", "audio/flac",
    "audio/mp4", "audio/x-m4a",
}


def validate_audio_type(content_type: str) -> str:
    """
    Validate and normalize the audio MIME type.

    Args:
        content_type: The content type from the uploaded file

    Returns:
        Normalized MIME type string

    Raises:
        ValueError: If the audio type is not supported
    """
    if not content_type:
        return "audio/webm"

    ct = content_type.lower().strip()

    if ct in SUPPORTED_AUDIO_TYPES:
        return ct

    # Try partial matching (e.g., "audio/webm;codecs=opus")
    base_type = ct.split(";")[0].strip()
    if base_type in SUPPORTED_AUDIO_TYPES:
        return base_type

    raise ValueError(
        f"Unsupported audio type: '{content_type}'. "
        f"Supported types: {', '.join(sorted(SUPPORTED_AUDIO_TYPES))}"
    )
