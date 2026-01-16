# Baytology API - Windows Setup Guide

## For Angular Frontend Development

This guide helps you run the Baytology FastAPI backend on Windows to integrate with your Angular 19 app.

---

## Prerequisites

- **LM Studio** running with `gemma-2-9b-it` model on port `1234`

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
```

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
INFO:     Uvicorn running on http://127.0.0.1:8000
INFO:     Application startup complete.
```

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
| `/chat` | POST | Full conversation flow (main) |
| `/parse` | POST | Parse Arabic text → filters |
| `/question` | POST | Generate follow-up question |
| `/rank` | POST | Rank properties by deal score |
| `/search` | POST | Filter properties |
| `/session/{id}` | DELETE | Clear session |

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
