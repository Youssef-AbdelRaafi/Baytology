# Baytology API - Windows Setup Guide

## For Angular Frontend Development

This guide helps you run the Baytology FastAPI backend on Windows to integrate with your Angular 19 app.
The API includes both **text chat** and **voice chat** (speech-to-text) capabilities for Arabic real estate search.

---

## Prerequisites

- **LM Studio** running with `gemma-2-9b-it` model on port `1234`
- **FFmpeg** (required for voice recognition audio processing)

### Install FFmpeg on Windows

**Option A — Using winget (recommended):**
```powershell
winget install Gyan.FFmpeg
```

**Option B — Using Chocolatey:**
```powershell
choco install ffmpeg
```

**Option C — Manual install:**
1. Download from https://ffmpeg.org/download.html (choose Windows build)
2. Extract to `C:\ffmpeg`
3. Add `C:\ffmpeg\bin` to your system PATH
4. Restart your terminal

Verify:
```powershell
ffmpeg -version
```

---

## Step 1: Install Miniconda

1. Download Miniconda for Windows: https://docs.conda.io/en/latest/miniconda.html
2. Run the installer
3. ✅ Check "Add Miniconda to my PATH environment variable"
4. Complete installation
5. **Restart your terminal**

Verify installation:
```powershell
conda --version
```

---

## Step 2: Create Conda Environment

```powershell
# Navigate to project folder
cd C:\path\to\Baytology

# Create environment with Python 3.11
conda create -n chatbot_grade_project python=3.11 -y

# Activate environment
conda activate chatbot_grade_project
```

> **If `conda activate` doesn't work:** Run `conda init powershell` then restart PowerShell

---

## Step 3: Install Dependencies

```powershell
pip install -r requirements.txt
pip install faster-whisper python-multipart
```

> **Note:** `faster-whisper` is ~40 MB. On first run, the Whisper `medium` model (~1.5 GB) will be downloaded automatically and cached locally.

---

## Step 4: Setup Environment Variables

Create a file named `.env` in the project root:

```env
GOOGLE_API_KEY=your_google_api_key_here
CSV_FILE_PATH=egypt_real_estate_preprocessed.csv
LM_STUDIO_URL=http://localhost:1234/v1
LM_STUDIO_MODEL=gemma-2-9b-it
MAX_RESULTS=20
```

> **Note:** `GOOGLE_API_KEY` is only needed for LLM features. Voice recognition (speech-to-text) runs 100% locally using Whisper — no API key required.

---

## Step 5: Start LM Studio

1. Open LM Studio
2. Load model: `gemma-2-9b-it`
3. Start server on port `1234`

---

## Step 6: Run the API Server

```powershell
conda activate chatbot_grade_project
uvicorn api:app --reload --port 8000
```

**Expected output:**
```
[VOICE] Loading Whisper model 'medium' on cpu...
[VOICE] ✅ Whisper model loaded successfully.
INFO:     Uvicorn running on http://127.0.0.1:8000
INFO:     Application startup complete.
```

> The first startup takes 1-2 minutes while the Whisper model downloads. Subsequent starts are much faster.

---

## Step 7: Verify API is Working

Open browser: http://localhost:8000/docs

You should see Swagger UI with all endpoints.

---

## Angular Integration

### API Base URL
```typescript
const API_URL = 'http://localhost:8000';
```

### Main Endpoint: `/chat`

```typescript
interface ChatRequest {
  session_id: string;
  message: string;
}

interface ChatResponse {
  type: 'chat' | 'question' | 'results' | 'no_results' | 'fallback';
  message: string;
  question?: string;
  attribute?: string;
  properties: Property[];
  properties_count: number;
}
```

### Example Angular Service

```typescript
@Injectable({ providedIn: 'root' })
export class ChatService {
  private apiUrl = 'http://localhost:8000';
  
  constructor(private http: HttpClient) {}
  
  sendMessage(sessionId: string, message: string) {
    return this.http.post<ChatResponse>(`${this.apiUrl}/chat`, {
      session_id: sessionId,
      message: message
    });
  }
  
  clearSession(sessionId: string) {
    return this.http.delete(`${this.apiUrl}/session/${sessionId}`);
  }
}
```

---

## Available Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/chat` | POST | Full conversation flow — text input |
| `/voice-chat` | POST | Full conversation flow — voice/audio input |
| `/parse` | POST | Parse Arabic text → filters |
| `/question` | POST | Generate follow-up question |
| `/rank` | POST | Rank properties by deal score |
| `/search` | POST | Filter properties |
| `/session/{id}` | DELETE | Clear session |

### Voice Chat Endpoint (`/voice-chat`)

Accepts audio file upload (WAV, WebM, MP3, etc.) + session ID via `multipart/form-data`.

```typescript
// Angular example
sendVoiceMessage(sessionId: string, audioBlob: Blob) {
  const formData = new FormData();
  formData.append('session_id', sessionId);
  formData.append('audio', audioBlob, 'recording.webm');
  return this.http.post<VoiceChatResponse>(`${this.apiUrl}/voice-chat`, formData);
}

interface VoiceChatResponse extends ChatResponse {
  transcription: string;  // The Arabic text recognized from audio
}
```

---

## Troubleshooting

### "uvicorn not found"
```powershell
pip install uvicorn
```

### "ModuleNotFoundError"
```powershell
pip install -r requirements.txt
```

### "Connection refused" from Angular
1. Verify API is running: http://localhost:8000
2. Check firewall isn't blocking port 8000
3. Try `127.0.0.1` instead of `localhost`

### LM Studio Connection Error
- Ensure LM Studio is running
- Check model is loaded
- Verify port is `1234`

### Voice Recognition Issues

**"ffmpeg not found" or audio decoding errors:**
```powershell
# Verify ffmpeg is installed
ffmpeg -version
# If not, install it (see Prerequisites above)
```

**Slow transcription:**
- CPU transcription takes ~5-15 seconds per request (this is normal)
- If you have an NVIDIA GPU, edit `config.py`:
  ```python
  whisper_device: str = "cuda"          # Use GPU
  whisper_compute_type: str = "float16"  # GPU-optimized
  ```

**"ModuleNotFoundError: faster_whisper":**
```powershell
pip install faster-whisper
```

**"Form data requires python-multipart":**
```powershell
pip install python-multipart
```
