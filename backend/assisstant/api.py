from fastapi import FastAPI, HTTPException

from handler import handle_chat_request
from models import ChatRequest


app = FastAPI()



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