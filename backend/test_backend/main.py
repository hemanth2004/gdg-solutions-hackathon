from fastapi import FastAPI
from pydantic import BaseModel
from typing import Dict, Any, List, Optional

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

@app.post("/api/chat/")
async def process_ai_request(request: ChatRequest):
    print(request)  # This will print the received JSON
    return {"message": "Request received", "data": request.dict()}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000) 