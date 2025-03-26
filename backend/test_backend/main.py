from fastapi import FastAPI
from pydantic import BaseModel
from typing import Dict, Any, List

app = FastAPI()

class ExperimentRequest(BaseModel):
    schoolClass: str
    subject: str
    topic: str
    name: str
    theory: str
    procedure: str
    requiredApparatus: List[str]
    instantiatedApparatus: List[str]
    visualizations: List[Dict[str, str]]

@app.post("/api/chat/")
async def process_ai_request(request: ExperimentRequest):
    print(request)  # This will print the received JSON
    return {"message": "Request received", "data": request.dict()}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000) 