from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import Dict, Any, List, Optional

from handler import handle_chat_request



app = FastAPI()

class Visualization(BaseModel):
    name: str
    desc: str

class ExperimentContext(BaseModel):
    schoolClass: str
    subject: str
    topic: str
    name: str
    theory: str
    procedure: str
    requiredApparatus: List[str]
    instantiatedApparatus: List[str]
    visualizations: List[Visualization]

class ChatRequest(BaseModel):
    sessionID: str
    prompt: str
    base64Image: Optional[str]
    experimentContext: ExperimentContext
    language: str

class ChatResponse(BaseModel):
    message: str
    data: Dict[str, Any]
    audio_base64: Optional[str] = None
    visualizations: Optional[List[Dict[str, str]]] = None

@app.post("/api/chat/")
async def process_ai_request(request: ChatRequest):
    try:
        # Get response from handler and return it directly
        return handle_chat_request(request)
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Error processing request: {str(e)}"
        )

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000) 